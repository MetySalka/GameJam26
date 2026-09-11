using Godot;

public partial class FoodPickup : Area2D
{
	[Export] private Player _player;
	private CollisionShape2D _MouthUp;
	private CollisionShape2D _MouthDown;
	private CollisionShape2D _MouthLeft;
	private CollisionShape2D _MouthRight;
	public override void _Ready()
	{
		AreaEntered += OnAreaEntered;

		_MouthUp = GetNode<CollisionShape2D>("MouthUp");
		_MouthDown = GetNode<CollisionShape2D>("MouthDown");
		_MouthLeft = GetNode<CollisionShape2D>("MouthLeft");
		_MouthRight = GetNode<CollisionShape2D>("MouthRight");

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
	public void SetMouthDir(int dir)
	{
		switch (dir)
		{
			case 0:
				_MouthUp.Disabled = false;
				_MouthDown.Disabled = true;
				_MouthLeft.Disabled = true;
				_MouthRight.Disabled = true;
				break;

			case 1:
				_MouthUp.Disabled = true;
				_MouthDown.Disabled = false;
				_MouthLeft.Disabled = true;
				_MouthRight.Disabled = true;
				break;

			case 2:
				_MouthUp.Disabled = true;
				_MouthDown.Disabled = true;
				_MouthLeft.Disabled = false;
				_MouthRight.Disabled = true;
				break;

			case 3:

				_MouthUp.Disabled = true;
				_MouthDown.Disabled = true;
				_MouthLeft.Disabled = true;
				_MouthRight.Disabled = false;

				break;

		}
	}

}
