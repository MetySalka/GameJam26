using Godot;

public partial class FoodPickup : Area2D
{
	[Export] private Player _player;

	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;
	}

	// Consume food when it enters the pickup area.
	private void OnAreaEntered(Area2D area)
	{
		if (area is Fishfood food)
		{
			GD.Print("Yum Yum");

			food.Consume();
		}
	}
}
