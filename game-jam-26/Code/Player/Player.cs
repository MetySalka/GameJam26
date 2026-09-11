using Godot;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float DecelFactor = 5.0f;
	public const float AccelFactor = 1.0f;

	[Export] public Node2D Background { get; set; }
	public Vector2 MovementArea = new Vector2I(384, 128);
	public const float JumpVelocity = -400.0f;
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private void ScrollBackgroundAtMovementEdge()
	{
		Vector2 clampedPosition = Helpers.ClampToRect(Position, _movementBounds);

		// Distance the player moved beyond the permitted rectangle.
		Vector2 overflow = Position - clampedPosition;

		// Keep the player inside the movement area.
		Position = clampedPosition;

		// Move the background in the opposite direction.
		if (!overflow.IsZeroApprox() && Background != null)
		{
			Background.Position -= overflow;
		}
	}

	public override void _Ready()
	{
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
	}

	public override void _PhysicsProcess(double delta)
	{
		MovementArea = _viewport.Size * 0.87f;
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		Vector2 center = _viewport.GetCenter();
		Vector2 topLeft = center - MovementArea / 2f;
		_movementBounds = new Rect2(topLeft, MovementArea);

		Vector2 velocity = Velocity;

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Ease toward the requested swimming speed on each axis.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Speed / DecelFactor);
		velocity.Y = Mathf.MoveToward(velocity.Y, direction.Y * Speed, Speed / DecelFactor);

		Velocity = velocity;
		MoveAndSlide();
		ScrollBackgroundAtMovementEdge();
	}
}
