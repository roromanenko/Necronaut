using Godot;
using System;

public partial class HealthBar : TextureProgressBar
{
	private CharacterBody2D player;

	public override void _Ready()
	{
		if (GetParent()?.GetParent() != null)
		{
			player = GetParent().GetParent().GetParent().GetNodeOrNull<CharacterBody2D>("Player");
		}

		if (player == null)
		{
			GD.PrintErr("Player not found!");
		}
		MaxValue = (int)player.Get("healPoints");  
		Value = (int)player.Get("healPoints");

	}

	public override void _Process(double delta)
	{
		if (player != null)
		{
			Value = (int)player.Get("healPoints");
		}
	}
}
