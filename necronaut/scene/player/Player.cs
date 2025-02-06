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

	[Export] private float _rayLength = 100f;
	private AnimatedSprite2D _sprite;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.AnimationFinished += OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		ApplyGravity(delta);
		HandleMovement();
		HandleJump();
		MoveAndSlide();
		if (Input.IsActionJustPressed("jump")) PerformRaycast();
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
	direction = Input.GetActionStrength("ui_right") - Input.GetActionStrength("ui_left");
	if (direction != 0)
	{
	  velocity.X = direction * Speed;
	  if ((_lastDirection < 0 && direction > 0) || (_lastDirection > 0 && direction < 0))
	  {
		_lastDirection = direction;
		_sprite.FlipH = !_sprite.FlipH;
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
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, JumpVelocity);
			_isJumped = true;
		}
		else if (IsOnFloor())
		{
			_isJumped = false;
		}
	}

	private void HandleAnimation()
	{
		if (_sprite == null) return;

		if (_isPunch)
			_sprite.Play("attack");
		else if (_isJumped)
			_sprite.Play(Velocity.Y <= 0 ? "jump" : "fall");
		else if (Velocity.X != 0)
			_sprite.Play("run");
		else
			_sprite.Play("idle");
	}

	private void PerformRaycast()
	{
		_isPunch = true;
		Vector2 start = GlobalPosition;
		Vector2 end = start + new Vector2(_lastDirection * _rayLength, 0);
		var spaceState = GetWorld2D().DirectSpaceState;
		var query = PhysicsRayQueryParameters2D.Create(start, end);
		var result = spaceState.IntersectRay(query);

		//if (result.Count > 0 && result["collider"] is Node collider)
		//{
			//// Обработка попадания
		//}
	}

	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "attack")
			_isPunch = false;
	}
}
