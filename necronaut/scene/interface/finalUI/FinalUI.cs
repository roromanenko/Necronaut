using Godot;

public partial class FinalUI : CanvasLayer
{
	public void GoToMainMenu()
	{
		GetTree().Paused = false;
		GetTree().ChangeSceneToFile("res://scene/interface/menu/main_menu.tscn");
	}
}
