using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public partial class MainMenu : Control
{
	[Export] public float ScrollSpeed = 50f; // Speed in pixels per second
	private string _newGameScenePath = "res://scene/Test level/first_level.tscn";

	private bool _isNewGameSceneLoading = false;
	private Stopwatch _stopwatch = new Stopwatch();

	private ParallaxBackground _background;
	private ProgressBar _progressBar;
	private VBoxContainer _buttonsContainer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_background = GetNode<ParallaxBackground>("ParallaxBackground");
		_progressBar = GetNode<ProgressBar>("ProgressBar");

		_progressBar.Visible = false;

		_buttonsContainer = GetNode<VBoxContainer>("ButtonsList");
		(_buttonsContainer.GetChildren().First() as Button).GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_background.ScrollOffset += new Vector2((float)(-ScrollSpeed * delta), 0);

		// New Game Loading Scene
		if (!_isNewGameSceneLoading)
			return;

		ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(_newGameScenePath);

		// immitate loading bar since ResourceLoader.LoadThreadedGetStatus progress is not correct
		// Approximate time is 2000 milliseconds, function was found imperially
		// https://www.desmos.com/calculator/ibhsbklt0w
		var currentProgress = 100 * (1 - Math.Pow(Math.E, -0.0011 * _stopwatch.ElapsedMilliseconds));
		_progressBar.Value = currentProgress;

		if (status == ResourceLoader.ThreadLoadStatus.Loaded)
		{
			_progressBar.Value = _progressBar.MaxValue;
			GD.Print($"Loaded in {_stopwatch.ElapsedMilliseconds}");
			GD.Print("Scene loaded!");
			PackedScene newScene = (PackedScene)ResourceLoader.LoadThreadedGet(_newGameScenePath);
			GetTree().ChangeSceneToPacked(newScene);
			_isNewGameSceneLoading = false;
			_stopwatch.Stop();
		}
	}

	public void OnNewGameButtonPressed()
	{
		ResourceLoader.LoadThreadedRequest(_newGameScenePath);
		_isNewGameSceneLoading = true;
		_progressBar.Visible = true;
		_stopwatch.Start();
		foreach (var child in _buttonsContainer.GetChildren())
		{
			if (child is Button button)
			{
				button.Disabled = true;
			}
		}
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
