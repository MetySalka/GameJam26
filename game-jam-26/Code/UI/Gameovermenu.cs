using Godot;
using System;
using GameJam26.Code.UI;



public partial class Gameovermenu : Control
{
	private Startingmenu _startingMenu;
	[Export] private Player _player;
	[Export] private Health _health;
	[Export] private ColorRect _fihWarn;
	
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Hide();
		_startingMenu = GetNode<Startingmenu>("/root/Main/ScreenUI/StartingMenu");
		_health.Hide();
		_fihWarn.Hide();

	}

	public override void _Process(double delta)
	{

	}
	
	private void OnTryAgainPressed()
	{
		_player.ResetForNewRun();
		_startingMenu.OnClickButtonStart();
	}

	private void OnExitPressed()
	{
		GetTree().Quit(1);
	}
}
