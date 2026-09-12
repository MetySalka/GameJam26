using Godot;

public partial class Stika : Area2D
{
	public const float SimulationMargin = 0.1f;



	public float RotSpeed = 3f;
	private bool Seek = true;
	private float Angle;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node2D _world;

	private AnimatedSprite2D _sprite;


	public override void _Ready()
	{
		_world = GetParent<Node2D>();
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");

	}

	public override void _Process(double delta)
	{


		if (Seek)
		{
			Angle += RotSpeed;
			Angle %= 360;
			RotationDegrees = Angle;
			GD.Print(RotationDegrees);
			if (Angle <= 90 || Angle >= 270)
			{
				_sprite.Play("Right");
			}
			else
			{
				_sprite.Play("Left");

			}
		}

		Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
		Rect2 simulationBounds = Helpers.GetSimulationBounds(visibleBounds, SimulationMargin);
		if (!simulationBounds.HasPoint(Position))
		{
			// Reuse enemy when it escapes
			Position = Helpers.RandomPointInMargin(visibleBounds, SimulationMargin, _rng);
		}


	}
}
