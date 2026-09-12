using Godot;

public partial class Fishfood : Area2D
{
	[Export] public float FoodValue { get; set; } = 0.5f;
	private bool _consumed;
	public const float SimulationMargin = 0.1f;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node2D _world;
	private AnimatedSprite2D _animatedSprite;

	// Assigned by Main before adding this food to the scene tree.
	public Main Spawner { get; set; }
	public PackedScene SourceScene { get; set; }

	// Local movement in pixels per frame, preserving the existing drift speed.
	public Vector2 MoveVector { get; set; }

	// Pick once so each piece stays consistently faster or slower.
	public float StarWarsSpeedMultiplier { get; private set; }

	public override void _Ready()
	{
		_world = GetParent<Node2D>();
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		UpdateAnimation();
		StarWarsSpeedMultiplier = (float)GD.RandRange(0.1, 1);
	}

	public override void _Process(double delta)
	{
		FoodUpdate();
	}

	public void FoodUpdate()
	{
		if (_consumed || IsQueuedForDeletion())
			return;

		UpdateAnimation();
		Position += MoveVector;
		Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
		Rect2 simulationBounds = Helpers.GetSimulationBounds(visibleBounds, SimulationMargin);
		if (simulationBounds.HasPoint(Position))
			return;

		Vector2 respawnPosition = Helpers.RandomPointInMargin(visibleBounds, SimulationMargin, _rng);
		Spawner.ExactFood(SourceScene, respawnPosition);
		Consume(false);
	}


	private void UpdateAnimation()
	{
		if (_animatedSprite == null || MoveVector.IsZeroApprox())
			return;

		// Face along the dominant drift axis; world scrolling isn't swimming.
		string animation = Mathf.Abs(MoveVector.X) >= Mathf.Abs(MoveVector.Y)
			? (MoveVector.X >= 0 ? "Right" : "Left")
			: (MoveVector.Y >= 0 ? "Down" : "Up");
		if (_animatedSprite.Animation != animation || !_animatedSprite.IsPlaying())
			_animatedSprite.Play(animation);
	}

	public void Consume(bool Do = true)
	{
		// QueueFree runs later, so ignore duplicate pickups in the same frame.

		if (_consumed)
			return;
		_consumed = true;
		
		if (Do)
		{
			GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += FoodValue;
		}
		
		QueueFree();
		
	}
}
