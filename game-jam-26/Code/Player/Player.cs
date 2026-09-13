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
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private AnimatedSprite2D _sprite;
	private Vector2 _startingPosition;
	private StringName _startingAnimation;
	private Node2D _landStuff;

	private FoodPickup _pickupArea;
	private PlayerHitbox _hitbox;

	private Vector2 _startingCameraPosition;
	private Vector2 _startingCameraZoom;

	private float _shakeMagnitude = 4f;

	private const string HitFlashShaderPath = "res://Assets/Shaders/hit_flash.gdshader";
	private ShaderMaterial _flashMaterial;
	private int _flashFramesLeft = 0;
	private int _flashFramesTotal = 1;
	private static readonly Color DefaultFlashColor = new Color(1f, 0.2f, 0.2f);

	public void OnPlayerHit(float intensity = 1f, Color? flashColor = null)
	{
		if (Invincible)
			return;


						GetNode<AudioStreamPlayer>("Hit").Play();

		GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer--;
		cameraShakeCnt = (int)((40 + (4 - GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer) * 5) * intensity);
		_shakeMagnitude = 3f * intensity;

		_flashMaterial.SetShaderParameter("flash_color", flashColor ?? DefaultFlashColor);
		_flashMaterial.SetShaderParameter("flash_amount", 1f);
		_flashFramesTotal = Mathf.Max(1, (int)(10 * intensity));
		_flashFramesLeft = _flashFramesTotal;
	}

	public void LevelUp()
	{

		Level++;
		UpdateLandState();
		if (Level == 2)
			_main.BeginLandPhase();
		_main.SpawnLevelUpFood();
		if (Level == 1)
			EvolveInto("PikeSprite");
		else if (Level == 2)
			EvolveInto("LizardSprite");
	}

	private void EvolveInto(string spriteName)
	{
		// Keep the current facing direction when evolving.
		StringName animation = _sprite.Animation;
		_sprite.Stop();
		_sprite.Hide();
		_sprite = GetNode<AnimatedSprite2D>(spriteName);
		_sprite.Show();
		_sprite.Play(animation);
		if (OnLand && Velocity.IsZeroApprox())
			_sprite.Stop();
	}

	private void OnPlayerDeath()
	{

		_main.HideSwordfishWarning();
		_fihWarn.Hide();
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
		_startingCameraZoom = _camera.Zoom;
		_startingAnimation = _sprite.Animation;
		_pickupArea = GetNode<FoodPickup>("PickupRadius");
		_hitbox = GetNode<PlayerHitbox>("Hitbox");
		_landStuff = GetNode<Node2D>("../LandStuff");

		_flashMaterial = new ShaderMaterial { Shader = GD.Load<Shader>(HitFlashShaderPath) };
		_flashMaterial.SetShaderParameter("flash_amount", 0f);
		GetNode<AnimatedSprite2D>("TadpoleSprite").Material = _flashMaterial;
		GetNode<AnimatedSprite2D>("PikeSprite").Material = _flashMaterial;
		GetNode<AnimatedSprite2D>("LizardSprite").Material = _flashMaterial;

		UpdateLandState();
	}

	private void UpdateLandState()
	{
		_landStuff.Visible = OnLand;
	}

	// The rectangle the player can actually swim/walk within. Safe to call even
	// while paused/before the first physics frame (e.g. when setting up a new run).
	public Rect2 GetMovementBounds()
	{
		Rect2 viewport = new Rect2(Vector2.Zero, GetViewport().GetVisibleRect().Size);
		Vector2 area = viewport.Size * (OnLand ? new Vector2(0.75f, 0.75f) : new Vector2(0.92f, 0.79f));
		Vector2 center = viewport.GetCenter();
		center.Y -= viewport.Size.Y * 0.045f;
		Vector2 topLeft = center - area / 2f;
		return new Rect2(topLeft, area);
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
		_camera.Zoom = _startingCameraZoom;
		_camera.ForceUpdateScroll();
		Background.SetCameraOffset(Vector2.Zero);
		AnimatedSprite2D pike = GetNode<AnimatedSprite2D>("PikeSprite");
		pike.Stop();
		pike.Hide();
		AnimatedSprite2D lizard = GetNode<AnimatedSprite2D>("LizardSprite");
		lizard.Stop();
		lizard.Hide();
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
			_camera.Offset = new Vector2(_rng.RandfRange(-_shakeMagnitude, _shakeMagnitude), _rng.RandfRange(-_shakeMagnitude, _shakeMagnitude));
			cameraShakeCnt--;
		}
		else
		{
			_camera.Offset = Vector2.Zero;
		}

		if (_flashFramesLeft > 0)
		{
			_flashFramesLeft--;
			float flashAmount = (float)_flashFramesLeft / _flashFramesTotal;
			_flashMaterial.SetShaderParameter("flash_amount", flashAmount);
		}



		if (GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer <= 0)
		{	OnPlayerDeath();
		}

		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		_movementBounds = GetMovementBounds();
		MovementArea = _movementBounds.Size;

		Vector2 velocity = Velocity;

		// Ease toward the requested swimming speed on each axis.
		Vector2 direction = Input.GetVector("left", "right", "up", "down");

		if (Input.IsActionJustPressed("jump"))
			velocity.Y += 900.0f;

		velocity.Y = Mathf.MoveToward(velocity.Y, direction.Y * Speed, Speed / DecelFactor);
		velocity.X = Mathf.MoveToward(velocity.X, direction.X * Speed, Speed / DecelFactor);

		if (!OnLand)
			UpdateMovementAnimation(velocity);

		Velocity = velocity;
		Vector2 previousPosition = Position;
		MoveAndSlide();
		Position = Helpers.ClampToRect(Position, _movementBounds);
		if (OnLand)
		{
			Vector2 movement = Position - previousPosition;
			if (movement.IsZeroApprox())
				_sprite.Stop();
			else
				UpdateMovementAnimation(movement.Normalized());
		}
	}
	private void UpdateMovementAnimation(Vector2 velocity)
	{
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

	}

	public void PlayPlop()
	{
		GetNode<AudioStreamPlayer>("FoodPlop").Play();
	}

	public void PlayWhoosh()
	{
				GetNode<AudioStreamPlayer>("LevelUpWhoosh").Play();

	}



}
