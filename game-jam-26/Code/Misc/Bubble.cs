using Godot;
using System;

public partial class Bubble : Area2D
{
	private bool _consumed = false;
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		
	}
	public void Consume(bool Do = true)
	{
		if (_consumed)
			return;

		_consumed = true;
		QueueFree();
		GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += 5;
	}
}
