using Godot;
using System;

public partial class GoblinScout : CharacterBody2D
{
	private CollisionShape2D collisionShape;
	private AnimatedSprite2D animatedSprite;
	private CharacterBody2D player;

	private Vector2 movementDirection = Vector2.Right;
	private float movementSpeed = 100f;
	private float movementTimer = 0f;
	private float movementChangeInterval = 2f;

	private float gravity = 500f;
	private float maxFallSpeed = 400f;

	private bool isFacingLeft = false;
	private float idleDistanceThreshold = 20f;
	private float attackDistanceThreshold = 50f;
	private float followDistanceThreshold = 250f;

	private int healPoints = 100;
	private bool isHit = false;
	private bool isDead = false;
	private bool isAttacking = false;

	public override void _Ready()
	{
		collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		player = GetParent().GetNode<CharacterBody2D>("Player");

		if (player == null)
		{
			GD.Print("Player not found!");
		}

		animatedSprite.Play("idle");
	}

	public override void _Process(double delta)
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
		UpdateAnimation();
		MoveAndSlide();

		CheckForAttack();
	}

	private void FollowPlayer(Vector2 directionToPlayer, double delta)
	{
		directionToPlayer = directionToPlayer.Normalized();
		Velocity = new Vector2(directionToPlayer.X * movementSpeed, Velocity.Y);

		if ((directionToPlayer.X < 0 && !isFacingLeft) || (directionToPlayer.X > 0 && isFacingLeft))
		{
			isFacingLeft = !isFacingLeft;
			animatedSprite.FlipH = isFacingLeft;
		}
	}

	private void PatrolMovement(double delta)
	{
		if (movementTimer >= movementChangeInterval)
		{
			movementDirection = (movementDirection == Vector2.Right) ? Vector2.Left : Vector2.Right;
			movementTimer = 0f;

			isFacingLeft = !isFacingLeft;
			animatedSprite.FlipH = isFacingLeft;
		}

		Velocity = new Vector2(movementDirection.X * (movementSpeed/2), Velocity.Y);
	}

	private void ApplyGravity(double delta)
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

	private void UpdateAnimation()
	{
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
		bool isPlayerAbove = player.GlobalPosition.Y < GlobalPosition.Y;

		if (distanceToPlayer <= idleDistanceThreshold || isPlayerAbove)
		{
			animatedSprite.Play("idle");
			return;
		}

		if (!IsOnFloor() && Velocity.Y > 0)
		{
			animatedSprite.Play("fall");
		}
		else if (Velocity.X != 0)
		{
			if ((Velocity.X < 0 && !isFacingLeft) || (Velocity.X > 0 && isFacingLeft))
			{
				isFacingLeft = !isFacingLeft;
				animatedSprite.FlipH = isFacingLeft;
			}
			
			animatedSprite.Play("walk");
		}
		else
		{
			animatedSprite.Play("idle");
		}
	}

	private void CheckForAttack()
	{
		float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

		if (distanceToPlayer <= attackDistanceThreshold && !isAttacking)
		{
			StartAttack();
		}
	}

	private void StartAttack()
	{
		isAttacking = true;
		animatedSprite.Play("attack");
		Velocity = Vector2.Zero;

		animatedSprite.Connect("animation_finished", new Callable(this, nameof(OnAttackAnimationFinished)));
		Attack();
	}

	private void OnAttackAnimationFinished()
	{
		isAttacking = false;
		animatedSprite.Disconnect("animation_finished", new Callable(this, nameof(OnAttackAnimationFinished)));
		UpdateAnimation();
	}

	public void OnHit(int damage)
	{
		if (isDead) return;

		healPoints -= damage;
		isHit = true;
		animatedSprite.Play("hit");

		if (healPoints <= 0)
		{
			isDead = true;
			animatedSprite.Play("death");
			CallDeferred("queue_free");
		}

		GetNode<Timer>("HitTimer").Start();
	}

	public void OnHitAnimationFinished()
	{
		isHit = false;
	}

	private void Attack()
	{
		if (collisionShape == null) return;

		Vector2 start = collisionShape.GlobalPosition;
		Vector2 offset = new Vector2((movementDirection.X > 0 ? 1 : -1) * (1000 / 2), 0);

		var shape = new RectangleShape2D { Size = new Vector2(1000, 1000) };
		var query = new PhysicsShapeQueryParameters2D
		{
			Shape = shape,
			Transform = new Transform2D(0, start + offset),
			CollideWithBodies = true
		};

		var spaceState = GetWorld2D().DirectSpaceState;
		var results = spaceState.IntersectShape(query);

		foreach (var result in results)
		{
			Node collider = (Node)result["collider"];
			if (collider == this)
				continue;

			if (collider is Player)
			{
				GD.Print("Hit: " + collider.Name);
				collider.Call("OnHit", this, 0);
			}
		}
	}
}
ввф
