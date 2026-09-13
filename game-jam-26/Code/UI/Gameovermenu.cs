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
		VisibilityChanged += UpdateDeathSprite;
		_startingMenu = GetNode<Startingmenu>("/root/Main/ScreenUI/StartingMenu");
		_health.Hide();
		_fihWarn.Hide();

	}

	private void UpdateDeathSprite()
	{
		if (!Visible)
			return;
		bool earlyLevel = _player.Level <= 1;
		GetNode<Sprite2D>("Death1").Visible = earlyLevel;
		GetNode<Sprite2D>("Death2").Visible = !earlyLevel;
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
