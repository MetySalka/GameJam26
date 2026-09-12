using System;
using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	float Counter = 0.0f;
	public override void _Process(double delta)
	{

		Counter += 5;
		var bar = GetNode<TextureProgressBar>("TextureProgressBar");
		GD.Print(bar.Value);
		
		bar.Value = (Math.Sin(Counter / 1000) + 1) * 100;
	}
}