using Godot;
using System;
using System.Drawing;

public partial class Player : CharacterBody2D
{
	public const float Speed = 300.0f;
	public const float DecelFactor = 5.0f;
	public const float AccelFactor = 1.0f;

	[Export] public Node2D Background { get; set; }
	public Vector2 MovementArea = new Vector2I(384, 128);
	public const float JumpVelocity = -400.0f;
	Rect2 Viewport;
	Rect2 MoveRect;




private Vector2 ClampToRect(Vector2 position, Rect2 rect)
{
    return new Vector2(
        Mathf.Clamp(position.X, rect.Position.X, rect.End.X),
        Mathf.Clamp(position.Y, rect.Position.Y, rect.End.Y)
    );
}


	private void ScrollBackgroundAtMovementEdge()
	{
		Vector2 clampedPosition = ClampToRect(Position, MoveRect);

		// Distance the player moved beyond the permitted rectangle.
		Vector2 overflow = Position - clampedPosition;

		// Keep the player inside MoveRect.
		Position = clampedPosition;

		// Move the background in the opposite direction.
		if (!overflow.IsZeroApprox() && Background != null)
		{
			Background.Position -= overflow;
		}
	}

	public override void _Ready()
	{

		Viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);

	}
	public override void _PhysicsProcess(double delta)
	{
		MovementArea = Viewport.Size * 0.87f;
		Viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		Vector2 center = Viewport.GetCenter();
		Vector2 topLeft = center - (Vector2)MovementArea / 2f;
		MoveRect = new Rect2(topLeft, MovementArea);



		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			// velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");
		// if (direction != Vector2.Zero)
		// {
		// velocity.X = direction.X * Speed;
		// velocity.Y = direction.Y * Speed;



		velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Speed / DecelFactor);
		velocity.Y = Mathf.MoveToward(velocity.Y, direction.Y * Speed, Speed / DecelFactor);



		Velocity = velocity;
		MoveAndSlide();
		ScrollBackgroundAtMovementEdge();
		Position = ClampToRect(Position, MoveRect);

		GD.Print(MoveRect + "  --moverect");
		GD.Print(Position + "  --pozice");



		if (!MoveRect.HasPoint(Position))
		{
			velocity.X = 0;
			velocity.Y = 0;

		}


	}

}

