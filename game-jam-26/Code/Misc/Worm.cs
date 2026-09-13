using Godot;

// Beach-only food. Unlike the drifting water food, the worm wanders around at
// random on the sand and gets eaten the same way once the player reaches it.
public partial class Worm : Area2D
{
	public const float SimulationMargin = 0.1f;

	[Export] public float FoodValue { get; set; } = 50.0f;
	[Export] public float MoveSpeed { get; set; } = 40f;

	// Set by Main right after spawning, so the worm can stay inside the beach
	// and disappear if the player leaves land, the same way Crab tracks its Target.
	public Player Target { get; set; }

	private bool _consumed;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	// Worm lives directly under Main (a plain Node, not Node2D), like Crab does,
	// so this can't be typed Node2D the way Fishfood's world is.
	private Node _world;
	private AnimatedSprite2D _sprite;
	private Vector2 _direction = Vector2.Right;
	private float _moveTimeLeft;
	private float _pauseTimeLeft;
	private bool _paused;

	public override void _Ready()
	{
		_world = GetParent();
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		PickNewDirection();
	}

	public override void _Process(double delta)
	{
		if (_consumed || IsQueuedForDeletion())
			return;

		if (!GodotObject.IsInstanceValid(Target) || Target.IsQueuedForDeletion() || !Target.OnLand)
		{
			QueueFree();
			return;
		}

		float seconds = (float)delta;
		if (_paused)
		{
			_pauseTimeLeft -= seconds;
			if (_pauseTimeLeft <= 0f)
				PickNewDirection();
			return;
		}

		_moveTimeLeft -= seconds;
		Rect2 bounds = Target.GetMovementBounds();
		Position += _direction * MoveSpeed * seconds;
		Position = Helpers.ClampToRect(Position, bounds);

		UpdateAnimation();

		bool atEdge = Position.X <= bounds.Position.X || Position.X >= bounds.End.X
			|| Position.Y <= bounds.Position.Y || Position.Y >= bounds.End.Y;
		if (_moveTimeLeft <= 0f || atEdge)
		{
			_paused = true;
			_pauseTimeLeft = _rng.RandfRange(0.4f, 1.4f);
		}
	}

	// The art only has left/right poses, so face along the horizontal component
	// instead of rotating the body like Crab does.
	private void UpdateAnimation()
	{
		string animation = _direction.X >= 0 ? "Right" : "Left";
		if (!_sprite.SpriteFrames.HasAnimation(animation))
			animation = "Right";
		if (_sprite.Animation != animation || !_sprite.IsPlaying())
			_sprite.Play(animation);
	}

	private void PickNewDirection()
	{
		_paused = false;
		_direction = Vector2.Right.Rotated(_rng.RandfRange(0f, Mathf.Tau));
		_moveTimeLeft = _rng.RandfRange(0.6f, 1.8f);
	}

	public void Consume(bool Do = true)
	{
		if (_consumed)
			return;
		_consumed = true;

		if (Do)
		{
			GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += FoodValue;
			EatBurst.SpawnAt(_world, GlobalPosition, new Color(0.86f, 0.55f, 0.62f));
		}

		QueueFree();
	}
}
