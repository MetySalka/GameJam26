using System;
using Godot;

public partial class Stika : Area2D
{
	public const float SimulationMargin = 0.15f;

	public Player Target { get; set; }

	public float RotSpeed = 140f;
	public Vector2 Velocity { get; set; } = new Vector2(0, 0);
	private bool Seek = true;
	private int countDown;
	private float Angle;
	private Vector2 targetLock;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node2D _world;

	private AnimatedSprite2D _sprite;
	private CollisionShape2D _rightHitbox;
	private CollisionShape2D _leftHitbox;
	private bool? _facingRight;


	public override void _Ready()
	{
		_world = GetParent<Node2D>();
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_rightHitbox = GetNode<CollisionShape2D>("CollisionShape2D");
		_leftHitbox = GetNode<CollisionShape2D>("CollisionShape2D2");
		UpdateFacing();

	}

	public override void _Process(double delta)
	{
		Position += Velocity * (float)delta;

		if (!Seek)
		{




			if (countDown > 0)
			{
				countDown--;
				if (countDown <= 0)
				{
					Seek = false;
					Launch(targetLock);
				}




			}
			else
			{
				countDown++;
				if (countDown >= 0)
				{
					Seek = true;
				}
			}
		}

		if (Seek)
		{
			Velocity = new Vector2(0, 0);
			float targetAngle = Helpers.GetAngleToObject(
				GlobalPosition, Target.GlobalPosition);

			float difference = Mathf.Wrap(targetAngle - Angle, -180f, 180f);


			float step = RotSpeed * (float)delta;

			Angle += Mathf.Clamp(difference, -step, step);
			Angle = Mathf.Wrap(Angle, -180f, 180f);


			UpdateFacing();



			if (Math.Abs(difference) < 0.1)
			{
				Seek = false;
				countDown = _rng.RandiRange(10, 60);
				targetLock = Target.Position;
			}
		}




	}
	private void UpdateFacing()
	{
		bool facingRight = Angle <= 90 && Angle > -90;
		RotationDegrees = facingRight ? Angle : Mathf.Wrap(Angle + 180, -180f, 180f);
		if (_facingRight == facingRight)
			return;

		_facingRight = facingRight;
		_sprite.Play(facingRight ? "Right" : "Left");
		// Change collision state safely outside any physics overlap callbacks.
		_rightHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, !facingRight);
		_leftHitbox.SetDeferred(CollisionShape2D.PropertyName.Disabled, facingRight);
	}

	public void Launch(Vector2 Target)
	{
		int v = _rng.RandiRange(450, 950);
		float angle = Helpers.GetAngleToObject(GlobalPosition, Target);
		Velocity = new Vector2(Mathf.Cos(Mathf.DegToRad(angle)) * v, Mathf.Sin(Mathf.DegToRad(angle)) * v);
		countDown = -960;
		Seek = false;
	}

	public void KYS()
	{
		QueueFree();
	}
}
