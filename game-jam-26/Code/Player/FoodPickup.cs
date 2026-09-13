using Godot;

public partial class FoodPickup : Area2D
{
	[Export] private Player _player;
	private CollisionShape2D _mouth;
	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;

		_mouth = GetNode<CollisionShape2D>("Mouth");
	}

	// Consume food when it enters the pickup area.
	private void OnAreaEntered(Area2D area)
	{
		if (area is Fishfood food)
		{
			GD.Print("Yum Yum");
			_player.PlayPlop();
			food.Consume();
		}
		else if (area is Worm worm)
		{
			GD.Print("Yum Yum");
			_player.PlayPlop();
			_player.PlayEatAnimation();
			worm.Consume();
		}
	}
}
