using Godot;
using System;

public partial class MeleeWeapon : Node2D
{
	// Animation speed parametrs
	private AnimatedSprite2D _sprite;
	private float _defaultSpeedScale = 1.5f;
	private float _attackSpeedScale = 2.0f;
	private float _groundAttackSpeedScale = 2.0f;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.SpeedScale = _defaultSpeedScale;
		if (_sprite != null)
		{
			_sprite.AnimationFinished += OnAnimationFinished;
		}
	}

	public void PlayAttackAnimation()
	{
		if (_sprite != null)
		{
			_sprite.SpeedScale = _attackSpeedScale;
			_sprite.Play("attack");
		}
	}
	
	public void PlayIdleAnimation()
	{
		if (_sprite != null)
		{
			_sprite.SpeedScale = _defaultSpeedScale;
			_sprite.Play("idle");
		}
	}
	
	public void PlayRunAnimation()
	{
		if (_sprite != null)
		{
			_sprite.Play("run");
		}
	}
	
	public void PlayJumpAnimation()
	{
		if (_sprite != null)
		{
			_sprite.Play("jump");
		}
	}
	
	public void PlayFallAnimation()
	{
		if (_sprite != null)
		{
			_sprite.Play("fall");
		}
	}
	
	public void PlayFallingAttackAnimation()
	{
		if (_sprite != null)
		{
			_sprite.SpeedScale = _attackSpeedScale;
			_sprite.Play("falling attack");
		}
	}
	
	public void PlayGroundAttackAnimation()
	{
		if (_sprite != null)
		{
			_sprite.SpeedScale = _groundAttackSpeedScale;
			_sprite.Play("ground attack");
		}
	}

	private void OnAnimationFinished()
	{
		if (_sprite.Animation == "attack" || _sprite.Animation == "ground attack")
		{
			_sprite.SpeedScale = _defaultSpeedScale;
		}
		if (_sprite.Animation == "attack")
		{
			PlayIdleAnimation();
		}
	}
	
	public void SetFlipH(bool flip)
	{
		if (_sprite != null)
			_sprite.FlipH = flip;
	}
}
