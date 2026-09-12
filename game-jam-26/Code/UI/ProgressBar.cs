using System;
using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : Node2D
{
	[Export] private TextureProgressBar _textureBar;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var barValue = _textureBar;
		GD.Print(barValue);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}