using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
	protected CollisionShape2D collisionShape;
	protected AnimatedSprite2D animatedSprite;
	protected CharacterBody2D player;

	protected Vector2 movementDirection = Vector2.Right;
	protected float movementSpeed = 100f;
	protected float movementTimer = 0f;
	protected float movementChangeInterval = 2f;

	protected float gravity = 500f;
	protected float maxFallSpeed = 400f;

	protected bool isFacingLeft = false;
	protected float idleDistanceThreshold = 20f;
	protected float attackDistanceThreshold = 50f;
	protected float followDistanceThreshold = 250f;

	protected int healPoints = 100;
	protected bool isHit = false;
	protected bool isDead = false;
	protected bool isAttacking = false;
	protected bool isWaitAttack = false;
	protected Timer attackTimer;
	
	protected float _lastDirection = 1f;
	protected float _rayLength = 50f; 

	public override void _Ready()
	{
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetParent().GetParent().GetNode<CharacterBody2D>("Player");
		animatedSprite.AnimationFinished += OnAnimationFinished;

		attackTimer = GetNode<Timer>("Timer");
		attackTimer.Timeout += OnAttackTimerTimeout;

		if (player == null)
		{
			GD.Print("Player not found!");
		}

		animatedSprite.Play("idle");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (isHit || isDead || isAttacking)
			return;

		movementTimer += (float)delta;

		Vector2 directionToPlayer = player.GlobalPosition - GlobalPosition;
		float distanceToPlayer = directionToPlayer.Length();

		if (distanceToPlayer <= followDistanceThreshold)
		{
			FollowPlayer(directionToPlayer, delta);
		}
		else
		{
			PatrolMovement(delta);
		}

		ApplyGravity(delta);

		if(!isWaitAttack)CheckForAttack();
		UpdateAnimation();
		MoveAndSlide();
	}

	protected void FollowPlayer(Vector2 directionToPlayer, double delta)
	{
		directionToPlayer = directionToPlayer.Normalized();
		Velocity = new Vector2(directionToPlayer.X * movementSpeed, Velocity.Y);

		if ((directionToPlayer.X < 0 && !isFacingLeft) || (directionToPlayer.X > 0 && isFacingLeft))
		{
			isFacingLeft = !isFacingLeft;
			animatedSprite.FlipH = isFacingLeft;
			_lastDirection = isFacingLeft ? -1f : 1f;
		}
	}

	protected void PatrolMovement(double delta)
	{
		if (movementTimer >= movementChangeInterval)
		{
			movementDirection = (movementDirection == Vector2.Right) ? Vector2.Left : Vector2.Right;
			movementTimer = 0f;

			isFacingLeft = !isFacingLeft;
			animatedSprite.FlipH = isFacingLeft;
			_lastDirection = isFacingLeft ? -1f : 1f;
		}

		Velocity = new Vector2(movementDirection.X * (movementSpeed / 2), Velocity.Y);
	}

	protected void ApplyGravity(double delta)
	{
		if (!IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + gravity * (float)delta);

			if (Velocity.Y > maxFallSpeed)
			{
				Velocity = new Vector2(Velocity.X, maxFallSpeed);
			}
		}
		else
		{
			Velocity = new Vector2(Velocity.X, 0);
		}
	}

	protected void UpdateAnimation()
	{
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		bool isPlayerAbove = player.GlobalPosition.Y < GlobalPosition.Y;

		if(isAttacking)
		{
			animatedSprite.Play("attack");
			return;
		}
		if (distanceToPlayer <= idleDistanceThreshold || isPlayerAbove)
		{
			animatedSprite.Play("walk");
			isAttacking = false;
			return;
		}
		if (!IsOnFloor() && Velocity.Y > 0)
		{
			animatedSprite.Play("fall");
			isAttacking = false;
		}
		else if (Velocity.X != 0)
		{
			if ((Velocity.X < 0 && !isFacingLeft) || (Velocity.X > 0 && isFacingLeft))
			{
				isFacingLeft = !isFacingLeft;
				animatedSprite.FlipH = isFacingLeft;
			}

			animatedSprite.Play("walk");
			isAttacking = false;
		}
		else
		{
			animatedSprite.Play("idle");
		}
	}

	protected void CheckForAttack()
	{
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

		if (distanceToPlayer <= attackDistanceThreshold && !isAttacking)
		{
			StartAttack();
		}
	}

	virtual protected void StartAttack()
	{

	}

	virtual protected void OnAnimationFinished()
	{

	}

	virtual public void OnHit(int damage)
	{
	}

	virtual protected void Attack()
	{
	}

	protected void OnAttackTimerTimeout()
	{
		isWaitAttack = false;
	}
}
