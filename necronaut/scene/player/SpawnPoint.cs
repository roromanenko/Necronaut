using Godot;

public partial class SpawnPoint : Node2D
{
	public PackedScene CharecterScene;
	public void SpawnCharecter(Vector2 position)
	{
		if (CharecterScene != null)
		{
			var character = CharecterScene.Instantiate<CharacterBody2D>();
			
			character.Position = position;
			
			GetParent().GetParent().AddChild(character);
		}
		else
		{
			GD.PrintErr("Character scene is not assigned!");
		}
	}

	public override void _Ready()
	{
		CharecterScene =  ResourceLoader.Load<PackedScene>("res://scene/player/Player.tscn");
	}
}
