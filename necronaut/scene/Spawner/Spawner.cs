using Godot;
using System;

public partial class Spawner : Node2D
{
	private PackedScene enemyScene;

	public override void _Ready()
	{
		GD.Print("Spawner готов!");

		enemyScene = ResourceLoader.Load<PackedScene>("res://scene/enemies/Goblin scout.tscn");
		if (enemyScene == null)
		{
			GD.PrintErr("Не удалось загрузить сцену: res://scene/enemies/Goblin scout.tscn");
			return;
		}
		GD.Print("Сцена загружена успешно!");
		
		Node2D enemyInstance = (Node2D)enemyScene.Instantiate();
		GD.Print("Enemy instance создан: " + enemyInstance.Position);
		AddChild(enemyInstance);
	}
}
