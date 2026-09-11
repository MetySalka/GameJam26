using Godot;
using System;

public partial class FoodPickup : Area2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] private Player _player;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	private void OnAreaEntered(Area2D area)
	{
		if (area is Fishfood food)
		{
			//_player.Eat(food.HealAmount);
			GD.Print("Yum Yum");
			
			food.Consume();
		}
	}
}
