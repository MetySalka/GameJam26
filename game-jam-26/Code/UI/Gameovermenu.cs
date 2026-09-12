using Godot;
using System;

public partial class Gameovermenu : Control
{
	[Export] private Control _startingmenu;
	
	public override void _Ready()
	{

	}

	public override void _Process(double delta)
	{

	}

	private void OnTryAgainPressed()
	{
		_startingmenu.Show();
		Hide();
	}
}
