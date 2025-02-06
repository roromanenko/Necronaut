using Godot;
using System;

public partial class Player : CharacterBody2D
{
  private const float Speed = 300.0f;
  private const float JumpVelocity = -500.0f;
  private float Gravity = 2000f;
  private float direction = 0;
  private bool isJumped = false;
  private bool isPunch = false;
  private float _lastDirection = -1;
  [Export] private float _rayLength = 100f;
  private AnimatedSprite2D _sprite;

  public override void _Ready()
  {
	_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
	_sprite.AnimationFinished += OnAnimationFinished;
  }

  public override void _PhysicsProcess(double delta)
  {
	HandleMovement();
	HandleJump();
	MoveAndSlide();
	if (Input.IsActionJustPressed("jump"))
	  PerformRaycast();
	HandleAnimation();
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
	  isJumped = true;
	}
	else if (IsOnFloor())
	{
	  isJumped = false;
	}
  }

  private void HandleAnimation()
  {
	if (_sprite == null) return;
	if (isJumped)
	{
	  if (Velocity.Y <= 0)
		_sprite.Play("jump");
	  else
		_sprite.Play("fall");
	}
	else if (direction != 0)
	{
	  _sprite.Play("run");
	}
	else if (!isPunch)
	{
	  _sprite.Play("idle");
	}
	if (isPunch)
	{
	  _sprite.Play("attack");
	}
  }

  private void PerformRaycast()
  {
	isPunch = true;
	Vector2 start = GlobalPosition;
	Vector2 end = start + new Vector2(_lastDirection * _rayLength, 0);
	var spaceState = GetWorld2D().DirectSpaceState;
	var query = PhysicsRayQueryParameters2D.Create(start, end);
	var result = spaceState.IntersectRay(query);
	if (result.Count > 0)
	{
	  Node collider = result["collider"].As<Node>();
	  if (collider != null)
	  {
	  }
	}
  }

  private void OnAnimationFinished()
  {
	if (_sprite.Animation == "attack")
	  isPunch = false;
	  _sprite.Play("idle");
  }
}
