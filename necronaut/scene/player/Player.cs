using Godot;

public partial class Player : CharacterBody2D
{
	// Узлы и константы
	CollisionShape2D collision_shape;
	Node2D weaponInstance;

	private const float Speed = 300.0f;
	private const float JumpVelocity = -750.0f;
	private const float Gravity = 2000f;

	// Параметры движения и состояния
	private float direction = 0;
	private float _lastDirection = -1;
	private bool _isJumped = false;
	private bool _isPunch = false;
	private bool _isAirAttack = false;
	private bool _isGroundAttack = false;
	public bool isDead = false;

	// Характеристики персонажа
	int healPointLevel = 1;
	int shieldLevel = 1;
	int damageLevel = 1;

	int healPoints = 100;
	int shieldPoints = 10;
	int damageMultiplier = 1;
	int instantDamage = 0;

	[Export] private float _rayWidth = 10f;
	[Export] private float _rayLength = 50f;


	private AnimatedSprite2D _sprite;
	private MeleeWeapon _weapon;
	private bool _wasOnFloor = true;

	// Параметры воздушной атаки
	private float _airAttackStallTime = 0.2f;
	private float _airAttackTimer = 0f;
	private float _diveSpeed = 2000f;
	private bool _hasStartedDive = false;
	private float _airAttackDeceleration = 2000f;

	// Скорости анимаций
	private float _defaultSpeedScale = 1.5f;
	private float _attackSpeedScale = 2.0f;
	private float _groundAttackSpeedScale = 2.0f;

	[Export] private int maxJumps = 1;
	[Export] private float doubleJumpMultiplier = 0.7f;
	private int jumpsUsed = 0;

	public bool isAlive()
	{
		return !isDead;
	}
	public override void _Ready()
	{
		collision_shape = GetNode<CollisionShape2D>("CollisionShape2D");

		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.SpeedScale = _defaultSpeedScale;
		_sprite.AnimationFinished += OnAnimationFinished;

		PackedScene weaponScene = ResourceLoader.Load<PackedScene>("res://scene/weapons/Iron Axe.tscn");
		if (weaponScene == null)
		{
			//	GD.PrintErr("res://scene/weapons/Iron Axe.tscn не найден");
		}
		else
		{
			weaponInstance = (Node2D)weaponScene.Instantiate();
			Node2D weaponSocket = GetNode<Node2D>("WeaponSocket");
			if (weaponSocket == null)
			{
				//GD.PrintErr("Узел WeaponSocket не найден в сцене персонажа!");
			}
			else
			{
				weaponSocket.AddChild(weaponInstance);
				_weapon = weaponInstance as MeleeWeapon;
				weaponInstance.Position = Vector2.Zero;
			}
		}
	}

	public override void _Process(double delta)
	{
		if (isDead)
			return;
		ProcessAnimation();
	}


	public override void _PhysicsProcess(double delta)
	{
		if (isDead)
			return;

		// Обрабатываем воздушную атаку и приземление
		ProcessAirAttack(delta);
		ProcessLanding();

		// Применяем гравитацию
		ProcessGravity(delta);

		// Если персонаж не атакует (на земле или в прыжке), разрешаем движение и прыжки.
		if (!_isAirAttack && !_isGroundAttack && !_isPunch)
		{
			ProcessJump();
			ProcessMovement(delta);
		}
		else
		{
			// Во время атаки запрещаем горизонтальное движение
			Velocity = new Vector2(0, Velocity.Y);
		}

		MoveAndSlide();
		ProcessAttackInput();
	}


	// Применение гравитации
	private void ProcessGravity(double delta)
	{
		if (!IsOnFloor())
			Velocity += new Vector2(0, Gravity * (float)delta);
	}

	// Обработка перемещения и поворота спрайта
	private void ProcessMovement(double delta)
	{
		Vector2 velocity = Velocity;

		direction = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			if ((_lastDirection < 0 && direction > 0) || (_lastDirection > 0 && direction < 0))
			{
				_lastDirection = direction;
				_sprite.FlipH = !_sprite.FlipH;
				_weapon?.SetFlipH(_sprite.FlipH);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		Velocity = velocity;
	}

	// Обработка прыжков (одинарный и двойной)
	private void ProcessJump()
	{
		if (Input.IsActionJustPressed("jump"))
		{
			if (IsOnFloor())
			{
				Velocity = new Vector2(Velocity.X, JumpVelocity);
				_isJumped = true;
				jumpsUsed = 1;
			}
			else if (jumpsUsed < maxJumps)
			{
				Velocity = new Vector2(Velocity.X, JumpVelocity * doubleJumpMultiplier);
				jumpsUsed++;
			}
		}
		if (IsOnFloor())
		{
			_isJumped = false;
			jumpsUsed = 0;
		}
	}

	// Обработка ввода для атаки
	private void ProcessAttackInput()
	{
		if (Input.IsActionJustPressed("attack"))
		{
			if (!IsOnFloor())
			{
				_isAirAttack = true;
				_isPunch = true;
				_airAttackTimer = _airAttackStallTime;
				_hasStartedDive = false;
				_wasOnFloor = false;

			}
			else
			{
				_isPunch = true;
			}

		}
	}

	// Логика воздушной атаки (замедление, отсчёт времени и запуск погружения)
	private void ProcessAirAttack(double delta)
	{
		if (_isAirAttack && !IsOnFloor())
		{
			if (_airAttackTimer > 0)
			{
				float newVelX = Mathf.MoveToward(Velocity.X, 0, _airAttackDeceleration * (float)delta);
				float newVelY = Mathf.MoveToward(Velocity.Y, 0, _airAttackDeceleration * (float)delta);
				Velocity = new Vector2(newVelX, newVelY);
				_airAttackTimer -= (float)delta;
			}
			else if (!_hasStartedDive)
			{
				_hasStartedDive = true;
				Velocity = new Vector2(0, _diveSpeed);
			}
		}
	}

	// Обработка приземления во время воздушной атаки
	private void ProcessLanding()
	{
		bool justLanded = (!_wasOnFloor && IsOnFloor());
		if (justLanded && _isAirAttack)
		{
			_wasOnFloor = IsOnFloor();

			_isAirAttack = false;
			_isPunch = false;
			_isGroundAttack = true;
		}
	}

	// Отдельная функция для обработки анимаций
	private void ProcessAnimation()
	{
		if (_sprite == null)
			return;

		if (!IsOnFloor())
		{
			if (_isAirAttack)
			{
				_isPunch = true;
				_sprite.SpeedScale = _attackSpeedScale;
				_sprite.Play("falling attack");
				_weapon?.PlayFallingAttackAnimation();
				return;
			}
			else
			{
				if (Velocity.Y > 0)
				{
					_isPunch = false;
					_sprite.Play("fall");
					_weapon?.PlayFallAnimation();
					return;
				}
				else if (Velocity.Y < 0)
				{
					_isPunch = false;
					_sprite.Play("jump");
					_weapon?.PlayJumpAnimation();
					return;
				}
			}
		}
		else
		{
			if (Mathf.Abs(Velocity.X) > 0.1f)
			{
				_sprite.SpeedScale = _defaultSpeedScale;
				_sprite.Play("run");
				_weapon?.PlayRunAnimation();
			}
			else if (_isGroundAttack)
			{
				if (_sprite.Animation == "ground attack") return;
				_sprite.SpeedScale = _groundAttackSpeedScale;
				_sprite.Play("ground attack");
				_weapon?.PlayGroundAttackAnimation();
			}
			else if (_isPunch)
			{
				if ((_sprite.Animation == "attack")) return;
				_sprite.SpeedScale = _attackSpeedScale;
				_sprite.Play("attack");
				_weapon?.PlayAttackAnimation();
			}
			else
			{
				_sprite.SpeedScale = _defaultSpeedScale;
				_sprite.Play("idle");
				_weapon?.PlayIdleAnimation();
			}
		}
	}

	// Функция выполнения ближнего боя (атака при ударе)
	private void ExecuteMeleeAttack()
	{
		if (collision_shape == null)
			return;

		Vector2 start = collision_shape.GlobalPosition;
		Vector2 offset = new Vector2(_lastDirection * _rayLength, 0);

		var shape = collision_shape.Shape;
		PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D();
		query.SetShape(shape);
		query.Transform = new Transform2D(0, start + offset);
		query.CollideWithBodies = true;

		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		var results = spaceState.IntersectShape(query);

		foreach (var result in results)
		{
			if (!result.ContainsKey("collider"))
				continue;

			Node2D collider = result["collider"].As<Node2D>();
			if (collider == null || collider == this)
				continue;

			//	GD.Print("Hit: " + collider.Name);
			if (collider.HasMethod("OnHit"))
			{
				//	GD.Print("Calling OnHit on " + collider.Name);
				collider.Call("OnHit", 50);
			}
			else
			{
				//	GD.Print("No OnHit method found on " + collider.Name);
			}
		}
	}

	private void ExecuteAirAttack()
	{
		if (collision_shape == null)
			return;

		var circleShape = new CircleShape2D();
		circleShape.Radius = 50;

		Vector2 attackCenter = collision_shape.GlobalPosition;

		PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D();
		query.SetShape(circleShape);
		query.Transform = new Transform2D(0, attackCenter);
		query.CollideWithBodies = true;

		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		var results = spaceState.IntersectShape(query);

		foreach (var result in results)
		{
			if (!result.ContainsKey("collider"))
				continue;

			Node2D collider = result["collider"].As<Node2D>();
			if (collider == null)
				continue;

			//GD.Print("Hit: " + collider.Name);

			if (collider == this)
				continue;

			if (collider.HasMethod("OnHit"))
			{
				//GD.Print("Calling OnHit on " + collider.Name);
				collider.Call("OnHit", 50);
			}
			else
			{
				//GD.Print("No OnHit method found on " + collider.Name);
			}
		}
	}

	// Обработка завершения анимаций
	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "attack")
		{
			_isPunch = false;
			_isGroundAttack = false;
			_sprite.SpeedScale = _defaultSpeedScale;
			ExecuteMeleeAttack();
		}
		else if (_sprite.Animation == "ground attack")
		{
			_isPunch = false;
			_isGroundAttack = false;
			_sprite.SpeedScale = _defaultSpeedScale;
			ExecuteAirAttack();
		}
		else if (_sprite.Animation == "death")
		{
			//CallDeferred("queue_free");
		}
	}

	// Обработка получения урона
	public void OnHit(int damage)
	{
		healPoints -= damage;

		if (healPoints <= 0)
		{
			GD.Print("Popki");
			_sprite.SpeedScale = _defaultSpeedScale;
			_sprite.Play("death");
			GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
			weaponInstance.CallDeferred("queue_free");
			isDead = true;
		}
	}
}
