using Godot;
using System.Collections.Generic;

public partial class SpawnManager : Node
{
	[Export]
	public int SpawnCounter = 4;


	private List<Node2D> _spawnPoints = new List<Node2D>();

	public void Init()
	{
		InitializeSpawnPoints();
	}

	private void InitializeSpawnPoints()
	{
		for (int i = 1; i <= SpawnCounter; i++)
		{
			var point = GetNodeOrNull<Node2D>("SpawnPoint" + i);
			if (point != null)
			{
				_spawnPoints.Add(point);
				GD.Print($"Found SpawnPoint{i} at {point.Position}");
			}
			else
			{
				GD.PrintErr($"SpawnPoint{i} not found!");
			}
		}
	}

	public Vector2 GetSpawnPosition(int index)
	{
		if (index >= 0 && index < _spawnPoints.Count)
			return _spawnPoints[index].GlobalPosition;

		GD.PrintErr("Invalid spawn index!");
		return Vector2.Zero;
	}

	public Vector2 GetRandomSpawnPosition()
	{
		if (_spawnPoints.Count == 0)
		{
			GD.PrintErr("No spawn points found!");
			return Vector2.Zero;
		}

		var random = new RandomNumberGenerator();
		random.Randomize();
		return _spawnPoints[random.RandiRange(0, _spawnPoints.Count - 1)].GlobalPosition;
	}

	public SpawnPoint GetRandomSpawner()
	{
		if (_spawnPoints.Count == 0)
		{
			GD.PrintErr("No spawn points found!");
			return null;
		}

		var random = new RandomNumberGenerator();
		random.Randomize();
		return (SpawnPoint)_spawnPoints[random.RandiRange(0, _spawnPoints.Count - 1)];
	}
}
