using Godot;
using System;
using GameJam26.Code.UI;

public partial class Gameovermenu : Control
{
	[Export] private Control _startingMenu;
	[Export] private Player _player;
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Hide();
	}

	public override void _Process(double delta)
	{

	}

	private void OnTryAgainPressed()
	{
		Hide();
		GetNode<GameJam26.Code.UI.Startingmenu>("/root/Main/StartingMenu").ShowMenu();
		_player.Health = 1;
	}
}
