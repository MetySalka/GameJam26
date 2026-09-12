using Godot;

namespace GameJam26.Code.UI;

public partial class Startingmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] private Main _main;
	[Export] private Player _player;
	[Export] private ProgressBar _progressBar;
	[Export] private Background _background;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		GetTree().Paused = true;
		_background.Hide();
		_player.Hide();
		_progressBar.Hide();
		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
	}
	public void OnClickButtonStart()
	{
		_background.Show();
		GetTree().Paused = false;
		_main.Call("prepareLevel");
		_player.Show();
		_progressBar.Show();
		GetNode<Control>("/root/Main/StartingMenu").Hide();
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