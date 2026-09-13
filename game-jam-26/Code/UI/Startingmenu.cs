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
	[Export] private Control _menuBackground;
	[Export] private TextureButton _debugToggle;
	[Export] private VBoxContainer _debugPanel;
	public void ShowMenu()
	{
		GetTree().Paused = true;
		_background.Hide();
		_player.Hide();
		_progressBar.Hide();
		_health.Hide();
		_menuBackground?.Show();
		_debugToggle?.Show();
		if (_debugPanel != null)
			_debugPanel.Visible = false;
		Show();
	}

	public void HideMenu()
	{
		_main.prepareLevel();
		_background.Show();
		GetTree().Paused = false;
		_player.Show();
		_progressBar.Show();
		GetNode<Control>("/root/Main/ScreenUI/StartingMenu").Hide();
		_health.Show();
		_menuBackground?.Hide();
		_debugToggle?.Hide();
		if (_debugPanel != null)
			_debugPanel.Visible = false;
	}
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		ShowMenu();
		DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
	}

	public void OnClickButtonStart()
	{
		GetNode<Control>("/root/Main/ScreenUI/GameOver").Hide();
		HideMenu();
}

	public void OnClickButtonExit()
	{
		GetTree().Quit(0); 
	}

	// Hidden helper for testing: toggles the debug level-jump buttons.
	public void OnClickDebugToggle()
	{
		_debugPanel.Show();
	}

	// Skips straight to a given evolution level instead of playing from the start.
	public void OnClickDebugLevel(int level)
	{
		GetNode<Control>("/root/Main/ScreenUI/GameOver").Hide();
		HideMenu();
		_main.DebugSetLevel(level);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{ 
		
	}	
}
