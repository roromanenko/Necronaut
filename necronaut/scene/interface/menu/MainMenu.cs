using Godot;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public partial class MainMenu : Control
{
	[Export] public float ScrollSpeed = 50f; // Speed in pixels per second
	private string _newGameScenePath = "res://scene/Test level/first_level.tscn";

	private bool _isNewGameSceneLoading = false;
	private double _approximateLoadingTimeMilliseconds = 2000;
	private Stopwatch _stopwatch = new Stopwatch();

	private ParallaxBackground _background;
	private ProgressBar _progressBar; // Reference to the loading bar

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_background = GetNode<ParallaxBackground>("ParallaxBackground");
		_progressBar = GetNode<ProgressBar>("ProgressBar");

		_progressBar.Visible = false;
		var newGameButton = GetNode<Button>("ButtonsList/NewGameButton");
		newGameButton.GrabFocus();
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
		var currentProgress = _stopwatch.ElapsedMilliseconds * 100 / _approximateLoadingTimeMilliseconds;
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
	}

	public void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}
