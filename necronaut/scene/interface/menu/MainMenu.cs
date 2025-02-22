using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] public float ScrollSpeed = 50f; // Speed in pixels per second
	private ParallaxBackground _background;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_background = GetNode<ParallaxBackground>("ParallaxBackground");

		var newGameButton = GetNode<Button>("ButtonsList/NewGameButton");
		newGameButton.GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_background.ScrollOffset += new Vector2((float)(-ScrollSpeed * delta), 0);
	}

	public void OnNewGameButtonPressed()
	{
		GetTree().ChangeSceneToFile("res://scene/Test level/first_level.tscn");
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
