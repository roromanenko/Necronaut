using Godot;
using System;

public partial class GoblinScout : Enemy
{
	
	override protected void StartAttack()
	{
		if(isHit) return;
		isAttacking = true;
		isWaitAttack = true;
		Velocity = Vector2.Zero;
	}
	
	
	override protected void Attack()
	{
		if (collisionShape == null)
			return;

		Vector2 start = collisionShape.GlobalPosition;
		Vector2 offset = new Vector2(_lastDirection * (_rayLength / 2), 0);

		var shape = collisionShape.Shape;  

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
	override public void OnHit(int damage)
	{
		if (isDead) return;

		healPoints -= damage;
		isHit = true;
		animatedSprite.Play("hit");

		if (healPoints <= 0)
		{
			isDead = true;
			animatedSprite.Play("death");
		}
	}
	override protected void OnAnimationFinished()
	{
		if (animatedSprite.Animation == "hit")
		{
			isHit = false;
			isAttacking = false;
			UpdateAnimation();
		}
		else if (animatedSprite.Animation == "attack")
		{
			isAttacking = false;
			Attack();
			attackTimer.Start();
			UpdateAnimation();
		}
		else if (animatedSprite.Animation == "death")
		{
			CallDeferred("queue_free");
		}
	}
	
	
}
