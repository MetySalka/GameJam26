using System.Net.Mime;
using Godot;

public partial class Player : CharacterBody2D
{
	[Export] private Main _main;
	[Export] private Player _player;
	[Export] private CanvasLayer _progressBar;
	[Export] private Control _health;
	[Export] private ColorRect _fihWarn;

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
	public bool OnLand => Level >= 2;
	public const float JumpVelocity = -400.0f;
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private AnimatedSprite2D _sprite;
	private Vector2 _startingPosition;
	private StringName _startingAnimation;
	private Node2D _landStuff;
	private CollisionShape2D _landCollider;

	private FoodPickup _pickupArea;
	private PlayerHitbox _hitbox;

	private Vector2 _startingCameraPosition;

	private void FollowCameraAtMovementEdge()
	{
		// Ignore the temporary shake offset when measuring the camera's follow area.
		Vector2 screenPosition = GlobalPosition - _camera.GlobalPosition;
		Vector2 overflow = screenPosition - Helpers.ClampToRect(screenPosition, _movementBounds);
		_camera.GlobalPosition += overflow;
		_camera.ForceUpdateScroll();
		Background.SetCameraOffset(_camera.GlobalPosition - _startingCameraPosition);
	}

	public void OnPlayerHit()
	{
		if (Invincible)
			return;

		GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer--;
		cameraShakeCnt = 20 + (4 - GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer) * 5;

	}

	public void LevelUp()
	{
		Level++;
		UpdateLandState();
		if (Level == 2)
		{
			_main.BeginLandPhase();
			Velocity = new Vector2(Velocity.X, 0f);
		}
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
		GetNode<Control>("/root/Main/ScreenUI/GameOver").Show();
	}

	public override void _Ready()
	{
		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		_sprite = GetNode<AnimatedSprite2D>("TadpoleSprite");
		_startingPosition = Position;
		_startingCameraPosition = _camera.GlobalPosition;
		_startingAnimation = _sprite.Animation;
		_pickupArea = GetNode<FoodPickup>("PickupRadius");
		_hitbox = GetNode<PlayerHitbox>("Hitbox");
		_landStuff = GetNode<Node2D>("../LandStuff");
		_landCollider = GetNode<CollisionShape2D>("LandCollider");
		UpdateLandState();
	}

	private void UpdateLandState()
	{
		bool onLand = OnLand;
		_landStuff.Visible = onLand;
		_landCollider.SetDeferred(CollisionShape2D.PropertyName.Disabled, !onLand);
	}

	public void ResetForNewRun()
	{
		Level = 0;
		UpdateLandState();
		GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer = 5;
		Position = _startingPosition;
		Velocity = Vector2.Zero;
		cameraShakeCnt = 0;
		_camera.Offset = Vector2.Zero;
		_camera.GlobalPosition = _startingCameraPosition;
		_camera.ForceUpdateScroll();
		Background.SetCameraOffset(Vector2.Zero);
		AnimatedSprite2D pike = GetNode<AnimatedSprite2D>("PikeSprite");
		pike.Stop();
		pike.Hide();
		_sprite = GetNode<AnimatedSprite2D>("TadpoleSprite");
		_sprite.Stop();
		_sprite.Animation = _startingAnimation;
		_sprite.Frame = 0;
		_sprite.Show();
		//_fihWarn.Show();
		int direction = _startingAnimation.ToString() switch
		{
			"Down" => 0,
			"Up" => 1,
			"Right" => 2,
			_ => 3
		};
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



		if (GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer <= 0)
		{	OnPlayerDeath();
		}

		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		MovementArea = _viewport.Size * (OnLand ? new Vector2(0.75f, 0.75f) : new Vector2(0.92f, 0.79f));
		Vector2 center = _viewport.GetCenter();
		center.Y = center.Y - _viewport.Size.Y * 0.045f; 
		Vector2 topLeft = center - MovementArea / 2f;
		_movementBounds = new Rect2(topLeft, MovementArea);

		Vector2 velocity = Velocity;

		// Ease toward the requested swimming speed on each axis.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if (OnLand)
		{
			direction.X = Input.GetAxis("left", "right");
			if (!IsOnFloor())
				velocity += GetGravity() * (float)delta;
			if (IsOnFloor() && (Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("jump")))
				velocity.Y = JumpVelocity;
		}
		else
		{
			if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
				velocity.Y = JumpVelocity;
			if (Input.IsActionJustPressed("jump"))
				velocity.Y += 500.0f;
			velocity.Y = Mathf.MoveToward(velocity.Y, direction.Y * Speed, Speed / DecelFactor);
		}

		velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Speed / DecelFactor);

		if (velocity.Y >= 0.2)
		{
			_sprite.Play("Down");
			_hitbox.SetHitDir(0);
		}
		else if (velocity.Y <= -0.2)
		{
			_sprite.Play("Up");
			_hitbox.SetHitDir(1);
		}

		if (velocity.X >= 0.2)
		{
			_sprite.Play("Right");
			_hitbox.SetHitDir(2);
		}
		else if (velocity.X <= -0.2)
		{

			_sprite.Play("Left");
			_hitbox.SetHitDir(3);
		}

		Velocity = velocity;
		MoveAndSlide();
		if (OnLand)
			FollowCameraAtMovementEdge();
		else
			Position = Helpers.ClampToRect(Position, _movementBounds);
	}
}
