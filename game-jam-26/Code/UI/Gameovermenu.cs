using Godot;
using System;
using GameJam26.Code.UI;



public partial class Gameovermenu : Control
{
	[Export] private Control _startingMenu;
	[Export] private Player _player;

	Background background;
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Hide();
		background = GetNode<Background>("../Background");
	}

	public override void _Process(double delta)
	{

	}

	private void OnTryAgainPressed()
	{
		Hide();
		_player.Health = 1;
		Helpers.ClearScene(background);
	}
}
