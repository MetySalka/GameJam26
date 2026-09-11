using Godot;
using System;
using System.Net.Quic;

public partial class Startingmenu : Control
{
	// Called when the node enters the scene tree for the first time.
	
	private TextureButton _startButton;
	private TextureButton _exitButton;
	public override void _Ready()
	{

	
		var zoom = 5f;
		var lookAt = new Vector2(15, 15);
		var half = GetViewportRect().Size / 2f;

		Scale = new Vector2(zoom, zoom);
		Position = half - lookAt * zoom;

	}
	public void OnClickButtonStart()
	{
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