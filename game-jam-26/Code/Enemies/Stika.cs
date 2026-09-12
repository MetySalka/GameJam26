using System;
using Godot;

public partial class Stika : Area2D
{
	public const float SimulationMargin = 0.05f;

	public Player Target { get; set; }

	public float RotSpeed = 100f;
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

			float targetAngle = Helpers.GetAngleToObject(
				GlobalPosition, Target.GlobalPosition);

			float difference = Mathf.Wrap(targetAngle - Angle, -180f, 180f);
			float step = RotSpeed * (float)delta;

			Angle += Mathf.Clamp(difference, -step, step);
			Angle = Mathf.Wrap(Angle, -180f, 180f);


			if (Angle <= 90 && Angle > -90)
			{
				_sprite.Play("Right");
				RotationDegrees = Angle;
			}
			else
			{
				_sprite.Play("Left");
				RotationDegrees = Mathf.Wrap(Angle+180, -180f, 180f);

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
