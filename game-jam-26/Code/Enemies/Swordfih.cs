using Godot;

public partial class Swordfih : Area2D
{
	[Export] public float Speed { get; set; } = 500f;
	// Clockwise degrees from screen-right, independent of the sprite's facing.
	[Export] public float HeadingDegrees { get; set; }
	[Export] public float SpawnDistance { get; set; } = 2000f;
	public Vector2 Velocity { get; private set; }
	private AnimatedSprite2D _sprite;
	private Node2D _world;
	private bool? _facingRight;
	private double _secondsRemaining = 15;
	private double _ghostTimer;
	private const double GhostInterval = 0.035;
	private CpuParticles2D _wakeParticles;
	private static readonly Texture2D[] BubbleTextures = new[]
	{
		GD.Load<Texture2D>("res://Assets/Sprites/Misc/Bubble/Bubble_0001.png"),
		GD.Load<Texture2D>("res://Assets/Sprites/Misc/Bubble/Bubble_0002.png"),
		GD.Load<Texture2D>("res://Assets/Sprites/Misc/Bubble/Bubble_0003.png"),
	};

	public float ScreenHeight => _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame)
		.GetHeight() * _sprite.GetGlobalTransformWithCanvas().Y.Length();

	public bool IsAboutToEnterViewport()
	{
		// Include the whole sprite and a short lead-in so the warning vanishes before entry.
		Vector2 size = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize();
		Vector2 origin = _sprite.Offset - (_sprite.Centered ? size * 0.5f : Vector2.Zero);
		Rect2 screenBounds = _sprite.GetGlobalTransformWithCanvas() * new Rect2(origin, size);


		return screenBounds.Intersects(GetViewport().GetVisibleRect().Grow(Mathf.Abs(Speed) * 0.1f));

	}

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_world = GetParent<Node2D>();
		UpdateMotion();
		SetupWakeParticles();
	}

	private void SetupWakeParticles()
	{
		_wakeParticles = new CpuParticles2D
		{
			Name = "WakeParticles",
			Emitting = true,
			Amount = 24,
			Lifetime = 0.6,
			Direction = Vector2.Right,
			Spread = 25f,
			InitialVelocityMin = 20f,
			InitialVelocityMax = 60f,
			ScaleAmountMin = 0.4f,
			ScaleAmountMax = 1.1f,
			Gravity = Vector2.Zero,
			Texture = BubbleTextures[0],
			Color = new Color(1f, 1f, 1f, 0.8f),
			LocalCoords = false,
		};
		_wakeParticles.ColorRamp = BuildFadeGradient();
		AddChild(_wakeParticles);
	}

	private static Gradient BuildFadeGradient()
	{
		Gradient gradient = new Gradient();
		gradient.SetColor(0, new Color(1f, 1f, 1f, 0.8f));
		gradient.SetColor(1, new Color(1f, 1f, 1f, 0f));
		return gradient;
	}

	private void SpawnGhost()
	{
		Sprite2D ghost = new Sprite2D
		{
			Texture = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame),
			FlipH = _sprite.FlipH,
			Modulate = new Color(0.6f, 0.85f, 1f, 0.5f),
			ZIndex = ZIndex - 1,
		};
		_world.AddChild(ghost);
		ghost.GlobalTransform = _sprite.GlobalTransform;
		Tween tween = ghost.CreateTween();
		tween.TweenProperty(ghost, "modulate:a", 0f, 0.25f);
		tween.TweenCallback(Callable.From(ghost.QueueFree));
	}

	public void SpawnInStream(float angle, Vector2 targetPosition, float sidewaysOffset)
	{
		HeadingDegrees = angle;
		Vector2 heading = Vector2.Right.Rotated(Mathf.DegToRad(HeadingDegrees));
		Vector2 sideways = new Vector2(-heading.Y, heading.X);
		Vector2 screenPosition = targetPosition - heading * SpawnDistance + sideways * sidewaysOffset;
		Position = _world.GetGlobalTransformWithCanvas().AffineInverse() * screenPosition;
		GetNode<AudioStreamPlayer>("LaserRay").Play();

		UpdateMotion();
	}

	private void UpdateMotion()
	{
		Vector2 heading = Vector2.Right.Rotated(Mathf.DegToRad(HeadingDegrees));
		Transform2D viewportToLocal = _world.GetGlobalTransformWithCanvas().AffineInverse();
		Vector2 localHeading = viewportToLocal.X * heading.X + viewportToLocal.Y * heading.Y;
		Velocity = localHeading * Speed;

		float angle = Mathf.RadToDeg(localHeading.Angle());
		bool facingRight = angle <= 90f && angle > -90f;
		RotationDegrees = facingRight ? angle : Mathf.Wrap(angle + 180f, -180f, 180f);
		if (_facingRight != facingRight)
		{
			_facingRight = facingRight;
			_sprite.Play(facingRight ? "Right" : "Left");
		}

		if (_wakeParticles != null && Velocity.LengthSquared() > 0.01f)
			_wakeParticles.GlobalRotation = Velocity.Angle() + Mathf.Pi;

		// if (GetViewport().GetVisibleRect().HasPoint(Position * 1.3f))
		// {
		// 	GetNode<AudioStreamPlayer>("WhateverThatIs").Play();
		// }
	}

	public override void _Process(double delta)
	{
		UpdateMotion();
		Position += Velocity * (float)delta;

		_ghostTimer += delta;
		if (_ghostTimer >= GhostInterval)
		{
			_ghostTimer = 0;
			SpawnGhost();
		}

		// Allow time to fly in from outside the screen before removing the fish.
		_secondsRemaining -= delta;
		if (_secondsRemaining <= 0)
			QueueFree();
	}
}
