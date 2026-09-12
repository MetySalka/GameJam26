using Godot;

namespace GameJam26.Code.UI;

public partial class Startingmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	[Export] private Main _main;
	[Export] private Player _player;
	[Export] private ProgressBar _progressBar;
	[Export] private Background _background;
	[Export] private Control _gameOver;
	[Export] private Health _health;
	public void ShowMenu()
	{
		GetTree().Paused = true;
		_background.Hide();
		_player.Hide();
		_progressBar.Hide();
		_health.Hide();
		Show();
	}

	public void HideMenu()
	{
		_main.prepareLevel();
		_background.Show();
		GetTree().Paused = false;
		_player.Show();
		_progressBar.Show();
		GetNode<Control>("/root/Main/StartingMenu").Hide();
		_health.Show();
	}
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		ShowMenu();
		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
	}

	public void OnClickButtonStart()
	{
		GetNode<Control>("/root/Main/GameOver").Hide();
		HideMenu();
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
