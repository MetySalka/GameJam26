using Godot;

public partial class Fishfood : Area2D
{
	[Export] public float FoodValue { get; set; } = 0.5f;
	// Chosen once at spawn; types 1–4 use frames 0–3 of the Types animation.
	public int FoodType { get; private set; } = 1;
	private bool _usesTypeFrames;
	// Set true for sprites without up/down facing frames, so they tumble instead.
	[Export] public bool SpinWhileDrifting { get; set; }
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

	// Pick once so each piece tumbles at its own slow, gentle rate.
	private float _rotationSpeed;

	public override void _Ready()
	{
		_world = GetParent<Node2D>();
		_animatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_usesTypeFrames = _animatedSprite != null && _animatedSprite.SpriteFrames.HasAnimation("Types");
		if (_usesTypeFrames)
		{
			FoodType = _rng.RandiRange(1, 30);
			if(FoodType != 7)
			{
				FoodType %= 3;
			} else {
				FoodType = 4;
			}
			_animatedSprite.Stop();
			_animatedSprite.Animation = "Types";
			_animatedSprite.SetFrameAndProgress(FoodType - 1, 0f);
		}
		UpdateAnimation();
		StarWarsSpeedMultiplier = (float)GD.RandRange(0.1, 1);
		_rotationSpeed = (float)GD.RandRange(-0.03, 0.03);
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
		// Sprites with up/down facing animations shouldn't also spin.
		if (_animatedSprite == null || SpinWhileDrifting)
			Rotation += _rotationSpeed;

		Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
		Vector2 position = Position;
		Vector2 moveVector = MoveVector;

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
		MoveVector = moveVector;
	}


	private void UpdateAnimation()
	{
		if (_usesTypeFrames || _animatedSprite == null || MoveVector.IsZeroApprox())
			return;

		// Face along the dominant drift axis; world scrolling isn't swimming.
		string animation = Mathf.Abs(MoveVector.X) >= Mathf.Abs(MoveVector.Y)
			? (MoveVector.X >= 0 ? "Right" : "Left")
			: (MoveVector.Y >= 0 ? "Down" : "Up");
		// Some food sprites (e.g. shrimp) only have left/right frames.
		if (!_animatedSprite.SpriteFrames.HasAnimation(animation))
			animation = MoveVector.X >= 0 ? "Right" : "Left";
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
			EatBurst.SpawnAt(_world, GlobalPosition, new Color(1f, 0.9f, 0.4f));
		}
		
		QueueFree();
		
	}
}
