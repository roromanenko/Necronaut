using Godot;
using System;

public partial class CounterTemplate : NinePatchRect
{
	protected CharacterBody2D player;
	protected Label counter;

	public override void _Ready()
	{
		if (GetParent()?.GetParent() != null)
		{
			player =  GetParent().GetParent().GetParent().GetNodeOrNull<CharacterBody2D>("Player");
		}

		counter = GetNodeOrNull<Label>("Counter");

		if (player == null)
		{
			GD.PrintErr("Player not found!");
		}

		if (counter == null)
		{
			GD.PrintErr("Counter label not found!");
		}
	}

	public override void _Process(double delta)
	{
		if (player != null && counter != null)
		{
			counter.Text = player.Get("healPoints").ToString();
		}
	}
}
