using Godot;

namespace GameJam26.Code.UI;

public partial class Startingmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	
	private TextureButton _startButton;
	private TextureButton _exitButton;

	public override void _Ready()
	{
		var main = GetParent();
		main.GetNode<CanvasItem>("Background").Hide();
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		GetTree().Paused = true;
		GetNode<Node2D>("../Player").Hide();
		GetNode<Node2D>("../ProgressBar").Hide();
		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
	}
	public void OnClickButtonStart()
	{
		var main =  GetParent();	
		main.GetNode<CanvasItem>("Background").Show();
		GetTree().Paused = false;
		GetParent().Call("prepareLevel");
		GetNode<Node2D>("../Player").Show();
		GetNode<Node2D>("../ProgressBar").Show();
		Hide();
	}

	public void OnClickButtonExit()
	{
		GetTree().Quit(0); 
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		
	}	
}