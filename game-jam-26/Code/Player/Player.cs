using System.Net.Mime;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export] private Main _main;
	[Export] private Player _player;
	[Export] private CanvasLayer _progressBar;
	[Export] private Control _health;

	[Export] private Camera2D _camera;
	int cameraShakeCnt = 0;

	public bool Invincible { get; set; } = false;

	public const float Speed = 300.0f;
	public const float DecelFactor = 5.0f;
	public const float AccelFactor = 1.0f;
	[Export] public global::Background Background { get; set; }

	RandomNumberGenerator _rng = new RandomNumberGenerator();
	public Vector2 MovementArea;
	public int Level { get;  set; } = 0;
	public const float JumpVelocity = -400.0f;
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private AnimatedSprite2D _sprite;
	private Vector2 _startingPosition;
	private StringName _startingAnimation;

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
		// if (!overflow.IsZeroApprox() && Background != null)
		// {
		// 	Background.Position -= new Vector2(overflow.X, 0);
		// 	Background.ScrollVertically(-overflow.Y);
		// }

		// if (Background.Position.X >= 1024)
		// {

		// 	Vector2 offset = new Vector2(-1024, 0);
		// 	Background.Position += offset;
		// 	Helpers.MoveFood(-offset, Background);

		// 	GD.Print("Whoops, too LEFT");

		// }
		// else if (Background.Position.X <= -1024)
		// {
		// 	Vector2 offset = new Vector2(1024, 0);
		// 	Background.Position += offset;
		// 	Helpers.MoveFood(-offset, Background);
		// 	GD.Print("Whoops, too RIGHT");

		// }


	}

	public void OnPlayerHit()
	{
		if (Invincible)
			return;

		GetNode<Health>("/root/Main/Health").HealthPlayer--;
		cameraShakeCnt = 20 + (4 - GetNode<Health>("/root/Main/Health").HealthPlayer) * 5;

	}

	public void LevelUp()
	{
		Level++;
		_main.SpawnLevelUpFood();
		if (Level != 1)
			return;

		// Keep the current facing direction when evolving into the pike.
		StringName animation = _sprite.Animation;
		_sprite.Stop();
		_sprite.Hide();
		_sprite = GetNode<AnimatedSprite2D>("PikeSprite");
		_sprite.Show();
		_sprite.Play("Up");
	}

	private void OnPlayerDeath()
	{
		GetTree().Paused = true;
		Background.Hide();
		Hide();
		_progressBar.Hide();
		_health.Hide();
		GetNode<Control>("/root/Main/GameOver").Show();
	}

	public override void _Ready()
	{
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		_sprite = GetNode<AnimatedSprite2D>("TadpoleSprite");
		_startingPosition = Position;
		_startingAnimation = _sprite.Animation;
		_pickupArea = GetNode<FoodPickup>("PickupRadius");
		_hitbox = GetNode<PlayerHitbox>("Hitbox");
	}

	public void ResetForNewRun()
	{
		Level = 0;
		GetNode<Health>("/root/Main/Health").HealthPlayer = 5;
		Position = _startingPosition;
		Velocity = Vector2.Zero;
		AnimatedSprite2D pike = GetNode<AnimatedSprite2D>("PikeSprite");
		pike.Stop();
		pike.Hide();
		_sprite = GetNode<AnimatedSprite2D>("TadpoleSprite");
		_sprite.Stop();
		_sprite.Animation = _startingAnimation;
		_sprite.Frame = 0;
		_sprite.Show();
		int direction = _startingAnimation.ToString() switch
		{
			"Down" => 0,
			"Up" => 1,
			"Right" => 2,
			_ => 3
		};
		_pickupArea.SetMouthDir(direction);
		_hitbox.SetHitDir(direction);
	}

	public override void _PhysicsProcess(double delta)
	{
		if(cameraShakeCnt > 0)
		{
			_camera.Offset = new Vector2(_rng.RandfRange(-3, 3), _rng.RandfRange(-3, 3));
			cameraShakeCnt--;
		}
		else
		{
			_camera.Offset = Vector2.Zero;
		}



		if (GetNode<Health>("/root/Main/Health").HealthPlayer <= 0)
		{	OnPlayerDeath();
		}

		MovementArea = _viewport.Size * new Vector2(0.92f, 0.79f);
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		Vector2 center = _viewport.GetCenter();
		center.Y = center.Y - _viewport.Size.Y * 0.045f; 
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

		if(Input.IsActionJustPressed("jump"))
		{
			velocity.Y += 500.0f;
		}

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
