using Godot;
using System;
using GameJam26.Code.UI;



public partial class Gameovermenu : Control
{
	private Startingmenu _startingMenu;
	[Export] private Player _player;
	
	
	Background background;
	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		Hide();
		_startingMenu = GetNode<Startingmenu>("/root/Main/StartingMenu");
		background = GetNode<Background>("../Background");
	}

	public override void _Process(double delta)
	{

	}

	private void OnTryAgainPressed()
	{
		Helpers.ClearScene(background);

		GetNode<Health>("/root/Health/").HealthPlayer = 5;
		Hide();
		_startingMenu.ShowMenu();
}
}