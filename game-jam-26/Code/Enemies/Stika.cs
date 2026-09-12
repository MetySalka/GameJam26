using Godot;

public partial class Stika : Area2D
{
	public const float SimulationMargin = 0.1f;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node2D _world;

	public override void _Ready()
	{
		_world = GetParent<Node2D>();
	}

	public override void _Process(double delta)
	{
		Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
		Rect2 simulationBounds = Helpers.GetSimulationBounds(visibleBounds, SimulationMargin);
		if (simulationBounds.HasPoint(Position))
			return;

		// Reuse this enemy when it escapes, keeping its count unchanged.
		Position = Helpers.RandomPointInMargin(visibleBounds, SimulationMargin, _rng);
	}
}
