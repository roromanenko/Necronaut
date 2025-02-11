using Godot;
using System;

public partial class Player : CharacterBody2D
{
	CollisionShape2D collision_shape;
	Node2D weaponInstance;
	
	private const float Speed = 300.0f;
	private const float JumpVelocity = -650.0f;
	private const float Gravity = 1000f;
	

	private float direction = 0;
	private float _lastDirection = -1;
	private bool _isJumped = false;
	private bool _isPunch = false;
	private bool _isAirAttack = false;
	private bool _isGroundAttack = false;
	
	bool isDead = false;

	
	
	int healPointLevel = 1;
	int shieldLevel = 1;
	int damageLevel =  1;
	
	int healPoints = 200;
	
	int shieldPoints = 10;
	
	int damageMultiplier = 1;
	int instantDamage = 0;
	
	
	   [Export] private float _rayWidth = 10f;
	


	[Export] private float _rayLength = 100f;
	private AnimatedSprite2D _sprite;

	private MeleeWeapon _weapon;
	private bool _wasOnFloor = true;

	
	private float _airAttackStallTime = 0.2f;
	private float _airAttackTimer = 0f;
	private float _diveSpeed = 2000f;
	private bool _hasStartedDive = false;
	private float _airAttackDeceleration = 2000f;

	
	private float _defaultSpeedScale = 1.5f;
	private float _attackSpeedScale = 2.0f;
	private float _groundAttackSpeedScale = 2.0f;

	
	[Export] private int maxJumps = 2;
	[Export] private float doubleJumpMultiplier = 0.65f;
	private int jumpsUsed = 0;

	public override void _Ready()
	{
		 collision_shape = GetNode<CollisionShape2D>("CollisionShape2D");
		
		
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.SpeedScale = _defaultSpeedScale;
		_sprite.AnimationFinished += OnAnimationFinished;

		PackedScene weaponScene = ResourceLoader.Load<PackedScene>("res://scene/weapons/Magic Sword.tscn");
		if (weaponScene == null)
		{
			GD.PrintErr("res://scene/weapons/Magic Sword.tscn");
		}
		else
		{
			 weaponInstance = (Node2D)weaponScene.Instantiate();
			Node2D weaponSocket = GetNode<Node2D>("WeaponSocket");
			if (weaponSocket == null)
			{
				GD.PrintErr("Узел WeaponSocket не найден в сцене персонажа!");
			}
			else
			{
				weaponSocket.AddChild(weaponInstance);
				_weapon = weaponInstance as MeleeWeapon;
				weaponInstance.Position = Vector2.Zero;
			}
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if(isDead) return;        
		if (_isAirAttack && !IsOnFloor())
		{
			if (_airAttackTimer > 0)
			{
				float newVelX = Mathf.MoveToward(Velocity.X, 0, _airAttackDeceleration * (float)delta);
				float newVelY = Mathf.MoveToward(Velocity.Y, 0, _airAttackDeceleration * (float)delta);
				Velocity = new Vector2(newVelX, newVelY);
				_airAttackTimer -= (float)delta;
				MoveAndSlide();
				HandleAnimation();
				return;
			}
			else if (!_hasStartedDive)
			{
				_hasStartedDive = true;
				Velocity = new Vector2(0, _diveSpeed);
			}
		}

		bool justLanded = (!_wasOnFloor && IsOnFloor());
		_wasOnFloor = IsOnFloor();
		if (justLanded && _isAirAttack)
		{
			_sprite.SpeedScale = _groundAttackSpeedScale;
			_sprite.Play("ground attack");
			if (_weapon != null)
				_weapon.PlayGroundAttackAnimation();
			_isAirAttack = false;
			_isPunch = false;
			_isGroundAttack = true;
			return;
		}

		if (IsOnFloor() && _isGroundAttack)
		{
			return;
		}

		ApplyGravity(delta);
		HandleMovement();
		HandleJump();

		if (IsOnFloor() && _isPunch && !_isAirAttack)
		{
			Velocity = new Vector2(0, Velocity.Y);
		}
		MoveAndSlide();

		if (Input.IsActionJustPressed("attack"))
		{
			if (!IsOnFloor())
			{
				_isAirAttack = true;
				_isPunch = true;
				_airAttackTimer = _airAttackStallTime;
				_hasStartedDive = false;
			}
			else
			{
				_isPunch = true;
			}
			Attack();
		}

		HandleAnimation();
	}

	private void ApplyGravity(double delta)
	{
		if (!IsOnFloor())
			Velocity += new Vector2(0, Gravity * (float)delta);
	}

	private void HandleMovement()
	{
		Vector2 velocity = Velocity;
		if (!IsOnFloor())
			velocity.Y += Gravity * (float)GetProcessDeltaTime();

		direction = Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left");
		if (direction != 0)
		{
			velocity.X = direction * Speed;
			if ((_lastDirection < 0 && direction > 0) || (_lastDirection > 0 && direction < 0))
			{
				_lastDirection = direction;
				_sprite.FlipH = !_sprite.FlipH;
				if (_weapon != null)
					_weapon.SetFlipH(_sprite.FlipH);
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}
		Velocity = velocity;
	}

	private void HandleJump()
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

	private void HandleAnimation()
	{
		if (_sprite == null) return;

		if (!IsOnFloor())
		{
			if (_isAirAttack)
			{
				_isPunch = true;
				_sprite.SpeedScale = _attackSpeedScale;
				_sprite.Play("falling attack");
				if (_weapon != null)
					_weapon.PlayFallingAttackAnimation();
				return;
			}
			else
			{
				if (Velocity.Y > 0)
				{
					_isPunch = false;
					_sprite.Play("fall");
					if (_weapon != null)
						_weapon.PlayFallAnimation();
					return;
				}
				else if (Velocity.Y < 0)
				{
					_isPunch = false;
					_sprite.Play("jump");
					if (_weapon != null)
						_weapon.PlayJumpAnimation();
					return;
				}
			}
		}
		else
		{
			if (_isPunch)
			{
				_sprite.SpeedScale = _attackSpeedScale;
				_sprite.Play("attack");
				if (_weapon != null)
					_weapon.PlayAttackAnimation();
			}
			else if (Mathf.Abs(Velocity.X) > 0.1f)
			{
				_sprite.Play("run");
				if (_weapon != null)
					_weapon.PlayRunAnimation();
			}
			else
			{
				_sprite.Play("idle");
				if (_weapon != null)
					_weapon.PlayIdleAnimation();
			}
		}
	}

	private void Attack()
	{
		if (collision_shape == null)
			return;

		Vector2 start = collision_shape.GlobalPosition;
		Vector2 offset = new Vector2(_lastDirection * (_rayLength / 2), 0);

		var shape = collision_shape.Shape;  

		PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D();
		query.SetShape(shape);
		query.Transform = new Transform2D(0, start + offset);
		query.CollideWithBodies = true;

		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		Godot.Collections.Array<Godot.Collections.Dictionary> results = spaceState.IntersectShape(query);

		foreach (var result in results)
		{
			if (!result.ContainsKey("collider"))
				continue;

			Node2D collider = result["collider"].As<Node2D>();
			if (collider == null)
				continue;

			GD.Print("Hit: " + collider.Name);

			if (collider == this)
				continue;

			GD.Print("Hit: " + collider.Name);
			var parent = collider;
			if (parent != null && parent.HasMethod("OnHit"))
			{
				GD.Print("Calling OnHit on parent...");
				parent.Call("OnHit", 50);
			}
			else
			{
				GD.Print("No OnHit method found on parent.");
			}
		}
	}

	public void OnHit(int damage)
	{
		healPoints -= damage;

		if (healPoints <= 0)
		{
			_sprite.Play("death");
			weaponInstance.CallDeferred("queue_free");
			isDead = true;
		}
	}

	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "attack" || _sprite.Animation == "ground attack")
		{
			_isPunch = false;
			_isGroundAttack = false;
			_sprite.SpeedScale = _defaultSpeedScale;
		}
		if(_sprite.Animation == "death")
		{
				CallDeferred("queue_free");
		}
	}
}
