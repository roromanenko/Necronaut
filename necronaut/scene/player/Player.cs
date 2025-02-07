using Godot;
using System;

public partial class Player : CharacterBody2D
{
	private const float Speed = 300.0f;
	private const float JumpVelocity = -650.0f;
	private const float Gravity = 1000f;

	private float direction = 0;
	private float _lastDirection = -1;
	private bool _isJumped = false;
	private bool _isPunch = false;
	private bool _isAirAttack = false;
	private bool _isGroundAttack = false;

	[Export] private float _rayLength = 100f;
	private AnimatedSprite2D _sprite;

	private MeleeWeapon _weapon;
	private bool _wasOnFloor = true;

	// Air attack parametrs
	private float _airAttackStallTime = 0.2f;     // Время заморозки (секунд)
	private float _airAttackTimer = 0f;           // Таймер заморозки
	private float _diveSpeed = 2000f;             // Скорость устремления вниз после заморозки
	private bool _hasStartedDive = false;         // Флаг, что фаза dive началась
	private float _airAttackDeceleration = 2000f; // Скорость замедления (для плавного freeze)

	// Animation speed parametrs
	private float _defaultSpeedScale = 1.5f;
	private float _attackSpeedScale = 2.0f;
	private float _groundAttackSpeedScale = 2.0f;

	// Double jump parametrs
	[Export] private int maxJumps = 2;
	[Export] private float doubleJumpMultiplier = 0.65f;
	private int jumpsUsed = 0;

	public override void _Ready()
	{
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
			Node2D weaponInstance = (Node2D)weaponScene.Instantiate();
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
			PerformRaycast();
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

	private void PerformRaycast()
	{
		Vector2 start = GlobalPosition;
		Vector2 end = start + new Vector2(_lastDirection * _rayLength, 0);
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(start, end);
		var result = spaceState.IntersectRay(query);
	}

	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "attack" || _sprite.Animation == "ground attack")
		{
			_isPunch = false;
			_isGroundAttack = false;
			_sprite.SpeedScale = _defaultSpeedScale;
		}
	}
}
