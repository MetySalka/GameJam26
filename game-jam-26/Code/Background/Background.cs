using Godot;
using System;
using System.Reflection.Metadata;
using System.Transactions;

public partial class Background : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public const float WobbleSpeed = 0.3f;
	public const float WobbleRange = 0.01f;
	public const float RisingSpeed = 0.05f;
	float phase = 0.0f;
	public override void _Process(double delta)
	{
		phase += WobbleSpeed;
		phase %= 360;

		
		Position += new Vector2((float)(Math.Sin((phase / 360) * 0.9f * Math.PI * 2) * WobbleRange / 2 + Math.Sin((phase / 360) * 1.1f *  Math.PI * 2) * WobbleRange / 2), 0);
	}
}
