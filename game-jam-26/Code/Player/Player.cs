using Godot;

public partial class Player : CharacterBody2D
{
	[Export] private Main _main;
	[Export] private Player _player;
	[Export] private CanvasLayer _progressBar;
	public const float Speed = 300.0f;
	public const float DecelFactor = 5.0f;
	public const float AccelFactor = 1.0f;
	public int Health = 1;

	[Export] public global::Background Background { get; set; }
	public Vector2 MovementArea = new Vector2I(384, 128);
	public const float JumpVelocity = -400.0f;
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private AnimatedSprite2D _sprite;

	private FoodPickup _pickupArea;
	private PlayerHitbox _hitbox;

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
			Background.Position -= new Vector2(overflow.X, 0);
			Background.ScrollVertically(-overflow.Y);
		}

		if (Background.Position.X >= 1024)
		{

			Vector2 offset = new Vector2(-1024, 0);
			Background.Position += offset;
			Helpers.MoveWorldObjects(-offset, Background);

			GD.Print("Whoops, too LEFT");

		}
		else if (Background.Position.X <= -1024)
		{
			Vector2 offset = new Vector2(1024, 0);
			Background.Position += offset;
			Helpers.MoveWorldObjects(-offset, Background);
			GD.Print("Whoops, too RIGHT");

		}


	}

	public void OnPlayerHit()
	{
		Health--;
	}

	private void OnPlayerDeath()
	{
		ProcessMode = Node.ProcessModeEnum.WhenPaused;
		GetTree().Paused = true;
		Background.Hide();
		_player.Hide();
		_progressBar.Hide();
		GetNode<Control>("/root/Main/GameOver").Show();
	}

	public override void _Ready()
	{
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_pickupArea = GetNode<FoodPickup>("PickupRadius");
		_hitbox = GetNode<PlayerHitbox>("Hitbox");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Health <= 0)
			OnPlayerDeath();

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

		if (velocity.Y >= 0.2)
		{
			_sprite.Play("Down");
			_pickupArea.SetMouthDir(0);
			_hitbox.SetHitDir(0);
		}
		else if (velocity.Y <= -0.2)
		{
			_sprite.Play("Up");
			_pickupArea.SetMouthDir(1);
			_hitbox.SetHitDir(1);
		}

		if (velocity.X >= 0.2)
		{
			_sprite.Play("Right");
			_pickupArea.SetMouthDir(2);
			_hitbox.SetHitDir(2);
		}
		else if (velocity.X <= -0.2)
		{

			_sprite.Play("Left");
			_pickupArea.SetMouthDir(3);
			_hitbox.SetHitDir(3);
		}

		Velocity = velocity;
		MoveAndSlide();
		ScrollBackgroundAtMovementEdge();
	}
}
