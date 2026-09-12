using Godot;

public partial class Main : Node
{
	private const float FoodSpeed = 0.1f;
	private const float SimulationMargin = 0.1f;

	[Export] public Node2D Background { get; set; }

	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private readonly PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	private Rect2 Viewport;
	private int Wait;
	private int _framesSinceSpawn;

	public override void _Ready()
	{
		Viewport = GetViewport().GetVisibleRect();
	}

	public void prepareLevel()
	{

		for (int i = 0; i < 500; i++)
		{
			generateFood(foodScene);
		}
		Wait = _rng.RandiRange(300, 1200);
	}

	public override void _Process(double delta)
	{
		FoodUpdate();
		_framesSinceSpawn++;

		// Keep the existing frame-based spawn timing.
		if (_framesSinceSpawn >= Wait)
		{
			_framesSinceSpawn = 0;
			Wait = _rng.RandiRange(300, 1200);
			generateFood(foodScene);
		}
	}

	public void generateFood(PackedScene scene)
	{
		Rect2 visibleBounds = Helpers.GetLocalViewport(Background);
		ExactFood(scene, Helpers.RandomPointInRect(visibleBounds, _rng));
	}


	public void ExactFood(PackedScene scene, Vector2 position)
	{
		Fishfood food = scene.Instantiate<Fishfood>();
		food.Position = position;
		food.MoveVector = new Vector2(
			_rng.RandfRange(-FoodSpeed, FoodSpeed),
			_rng.RandfRange(-FoodSpeed, FoodSpeed));
		Background.AddChild(food);
	}

	public void FoodUpdate()
	{
		Rect2 visibleBounds = Helpers.GetLocalViewport(Background);
		// The simulation extends 10% beyond each side of the screen.
		Vector2 margin = visibleBounds.Size * SimulationMargin;
		Rect2 simulationBounds = new Rect2(
			visibleBounds.Position - margin, visibleBounds.Size + margin * 2f);

		foreach (Node child in Background.GetChildren())
		{
			if (child is not Fishfood food || food.IsQueuedForDeletion())
				continue;

			// Food velocity is measured in local units per frame.
			food.Position += food.MoveVector;
			if (simulationBounds.HasPoint(food.Position))
				continue;

			// Replace escaped food somewhere in the offscreen margin.
			Vector2 respawnPosition;
			do
			{
				respawnPosition = Helpers.RandomPointInRect(simulationBounds, _rng);
			} while (visibleBounds.HasPoint(respawnPosition));

			ExactFood(foodScene, respawnPosition);
			food.Consume(false);
		}
	}
}
