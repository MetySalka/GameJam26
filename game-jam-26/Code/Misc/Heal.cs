using Godot;

public partial class Heal : Area2D
{
	[Export] public float DriftSpeed { get; set; } = 0.4f;
	[Export] public float SpinSpeed { get; set; } = 0.03f;

	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private Node2D _world;
	private Vector2 _moveVector;
	private float _spin;
	private bool _consumed;

	public override void _Ready()
	{
		_world = GetParent<Node2D>();
		_moveVector = new Vector2(
			_rng.RandfRange(-DriftSpeed, DriftSpeed),
			_rng.RandfRange(-DriftSpeed, DriftSpeed));
		_spin = _rng.RandfRange(-SpinSpeed, SpinSpeed);
	}

	public override void _Process(double delta)
	{
		if (_consumed || IsQueuedForDeletion())
			return;

		Position += _moveVector;
		Rotation += _spin;

		Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
		Vector2 position = Position;
		Vector2 moveVector = _moveVector;

		if (position.X < visibleBounds.Position.X)
		{
			position.X = visibleBounds.Position.X;
			moveVector.X = Mathf.Abs(moveVector.X);
		}
		else if (position.X > visibleBounds.End.X)
		{
			position.X = visibleBounds.End.X;
			moveVector.X = -Mathf.Abs(moveVector.X);
		}

		if (position.Y < visibleBounds.Position.Y)
		{
			position.Y = visibleBounds.Position.Y;
			moveVector.Y = Mathf.Abs(moveVector.Y);
		}
		else if (position.Y > visibleBounds.End.Y)
		{
			position.Y = visibleBounds.End.Y;
			moveVector.Y = -Mathf.Abs(moveVector.Y);
		}

		Position = position;
		_moveVector = moveVector;
	}

	public void Consume()
	{
		if (_consumed)
			return;
		_consumed = true;

		Health health = GetNode<Health>("/root/Main/ScreenUI/Health");
		health.HealthPlayer = Mathf.Min(health.HealthPlayer + 1, health.MaxHealth);
		EatBurst.SpawnAt(GetParent(), GlobalPosition, new Color(1f, 0.4f, 0.5f));
		QueueFree();
	}
}
