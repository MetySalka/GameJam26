using Godot;
using System;

public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	private RandomNumberGenerator _rng = new RandomNumberGenerator();

	public override void _Ready()
	{

		PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");


		for(int i = 0; i < 500; i++) {
		Fishfood food = foodScene.Instantiate<Fishfood>();
        food.Position = new Vector2(100 * _rng.RandfRange(0,1), 100* _rng.RandfRange(0,1));
        AddChild(food);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
