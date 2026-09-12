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
	}

	public void SpawnInStream(float angle, Vector2 targetPosition, float sidewaysOffset)
	{
		HeadingDegrees = angle;
		Vector2 heading = Vector2.Right.Rotated(Mathf.DegToRad(HeadingDegrees));
		Vector2 sideways = new Vector2(-heading.Y, heading.X);
		Vector2 screenPosition = targetPosition - heading * SpawnDistance + sideways * sidewaysOffset;
		Position = _world.GetGlobalTransformWithCanvas().AffineInverse() * screenPosition;
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
	}

	public override void _Process(double delta)
	{
		UpdateMotion();
		Position += Velocity * (float)delta;
		// Allow time to fly in from outside the screen before removing the fish.
		_secondsRemaining -= delta;
		if (_secondsRemaining <= 0)
			QueueFree();
	}
}
