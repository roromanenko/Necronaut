using Godot;

public partial class FirstLevel : Node2D
{
	public SpawnManager spawnerManager;
	

	public override void _Ready()
	{
		var spawnPoint = spawnerManager.GetRandomSpawner();
		spawnPoint.SpawnCharecter(new Vector2(100, 200));
	}
}
