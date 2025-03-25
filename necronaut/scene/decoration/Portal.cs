using Godot;
using System;

public partial class Portal : Area2D
{
	[Export]
	public PackedScene FinalUiScene;

	private bool _activated = false;

	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;

		var sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		sprite.Play();
	}

	private void OnBodyEntered(Node2D body)
	{
		if (_activated || !body.IsInGroup("Player"))
			return;

		_activated = true;

		var finalUi = FinalUiScene.Instantiate<CanvasLayer>();
		GetTree().CurrentScene.AddChild(finalUi);

		var animPlayer = finalUi.GetNode<AnimationPlayer>("AnimationPlayer");
		animPlayer?.Play("fade_and_show_text");

		var timer = new Timer
		{
			WaitTime = 5.0,
			OneShot = true,
			Autostart = true
		};
		timer.Timeout += () =>
		{
			animPlayer?.Play("fade_out"); // после задержки проигрываем fade_out
		};
		finalUi.AddChild(timer);
	}
}
