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

	private bool _externalInvincibility;
	private double _hitGraceSeconds;
	public bool Invincible
	{
		get => _externalInvincibility || _hitGraceSeconds > 0;
		set => _externalInvincibility = value;
	}

	public const float Speed = 300.0f;
	public const float DecelFactor = 5.0f;
	public const float AccelFactor = 1.0f;
	[Export] public global::Background Background { get; set; }

	RandomNumberGenerator _rng = new RandomNumberGenerator();
	public Vector2 MovementArea;
	public int Level { get; set; } = 0;
	public bool OnLand => Level >= 2;
	private Rect2 _viewport;
	private Rect2 _movementBounds;

	private AnimatedSprite2D _sprite;
	private Vector2 _startingPosition;
	private StringName _startingAnimation;
	private Node2D _landStuff;

	private bool _isEvolving;
	private ColorRect _evolveFlash;
	private bool _isEating;

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
		_hitGraceSeconds = 0.2;


		GetNode<AudioStreamPlayer>("Hit").Play();

		GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer--;
		cameraShakeCnt = (int)((40 + (4 - GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer) * 5) * intensity);
		_shakeMagnitude = 3f * intensity;

		_flashMaterial.SetShaderParameter("flash_color", flashColor ?? DefaultFlashColor);
		_flashMaterial.SetShaderParameter("flash_amount", 1f);
		_flashFramesTotal = Mathf.Max(1, (int)(10 * intensity));
		_flashFramesLeft = _flashFramesTotal;
	}

	public void ApplyKnockback(Vector2 velocity)
	{
		Velocity += velocity;
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

	private async void EvolveInto(string spriteName)
	{
		if (_isEvolving)
			return;
		_isEvolving = true;
		Velocity = Vector2.Zero;

		// Keep the current facing direction when evolving.
		StringName animation = _sprite.Animation;
		AnimatedSprite2D oldSprite = _sprite;
		AnimatedSprite2D newSprite = GetNode<AnimatedSprite2D>(spriteName);
		Vector2 oldScale = oldSprite.Scale;
		Vector2 targetScale = newSprite.Scale;

		PlayWhoosh();

		Tween sparkle = CreateTween();
		sparkle.TweenProperty(_evolveFlash, "color:a", 0.5f, 0.08);
		sparkle.TweenProperty(_evolveFlash, "color:a", 0f, 0.15);

		// Squash the old body down as if it's melting into the new form.
		Tween squash = CreateTween();
		squash.TweenProperty(oldSprite, "scale", new Vector2(oldScale.X * 1.35f, oldScale.Y * 0.1f), 0.14)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
		await ToSignal(squash, Tween.SignalName.Finished);

		// Cross-fade: old form finishes collapsing while the new form stretches up out of it.
		oldSprite.Modulate = new Color(1f, 1f, 1f, 1f);
		newSprite.Scale = new Vector2(targetScale.X * 1.35f, targetScale.Y * 0.1f);
		newSprite.Modulate = new Color(1f, 1f, 1f, 0f);
		newSprite.Show();
		newSprite.Play(animation);
		_sprite = newSprite;

		Tween morph = CreateTween();
		morph.SetParallel();
		morph.TweenProperty(oldSprite, "modulate:a", 0f, 0.16)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
		morph.TweenProperty(oldSprite, "scale:y", 0f, 0.16)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
		morph.TweenProperty(newSprite, "modulate:a", 1f, 0.16)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
		morph.TweenProperty(newSprite, "scale", targetScale * 1.2f, 0.2)
			.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
		await ToSignal(morph, Tween.SignalName.Finished);

		oldSprite.Stop();
		oldSprite.Hide();
		oldSprite.Scale = oldScale;
		oldSprite.Modulate = new Color(1f, 1f, 1f, 1f);

		Tween settle = CreateTween();
		settle.TweenProperty(newSprite, "scale", targetScale, 0.1)
			.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
		await ToSignal(settle, Tween.SignalName.Finished);

		cameraShakeCnt = 16;
		_shakeMagnitude = 4f;

		if (OnLand && Velocity.IsZeroApprox())
			_sprite.Stop();

		_isEvolving = false;
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

		_evolveFlash = new ColorRect
		{
			Color = new Color(1f, 1f, 1f, 0f),
			MouseFilter = Control.MouseFilterEnum.Ignore,
		};
		_evolveFlash.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		CanvasLayer evolveLayer = new CanvasLayer { Layer = 100 };
		evolveLayer.AddChild(_evolveFlash);
		AddChild(evolveLayer);

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

	public Rect2 GetOccupiedGlobalRect()
	{
		Vector2 size = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize();
		Vector2 origin = _sprite.Offset - (_sprite.Centered ? size * 0.5f : Vector2.Zero);
		Rect2 bounds = _sprite.GlobalTransform * new Rect2(origin, size);
		// Include every facing hitbox so a direction change cannot expose a spawn overlap.
		foreach (Node child in _hitbox.GetChildren())
			if (child is CollisionShape2D collider && collider.Shape != null)
				bounds = bounds.Merge(collider.GlobalTransform * collider.Shape.GetRect());
		return bounds;
	}

	public void ResetForNewRun()
	{
		_hitGraceSeconds = 0;
		_externalInvincibility = false;
		_isEvolving = false;
		Engine.TimeScale = 1f;
		if (_evolveFlash != null)
			_evolveFlash.Color = new Color(1f, 1f, 1f, 0f);
		Level = 0;
		UpdateLandState();
		Health health = GetNode<Health>("/root/Main/ScreenUI/Health");
		health.HealthPlayer = health.MaxHealth;
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
		_hitGraceSeconds = System.Math.Max(0, _hitGraceSeconds - delta);
		if (cameraShakeCnt > 0)
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
		{
			OnPlayerDeath();
		}

		_viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		_movementBounds = GetMovementBounds();
		MovementArea = _movementBounds.Size;

		if (_isEvolving)
		{
			Velocity = Vector2.Zero;
			return;
		}

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
		if (OnLand && !_isEating)
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
		if (velocity.X >= 0.2f)
		{
			_sprite.Play("Right");
			_hitbox.SetHitDir(2);
		}
		else if (velocity.X <= -0.2f)
		{
			_sprite.Play("Left");
			_hitbox.SetHitDir(3);
		}
		else if (velocity.Y >= 0.2f)
		{
			_sprite.Play("Down");
			_hitbox.SetHitDir(0);
		}
		else if (velocity.Y <= -0.2f)
		{
			_sprite.Play("Up");
			_hitbox.SetHitDir(1);
		}


	}

	// Dodo's bite: briefly swap to the Eat left/right pose facing whichever way
	// it was last walking, then hand animation control back to movement.
	public async void PlayEatAnimation()
	{
		if (!OnLand || _isEvolving)
			return;
		bool facingLeft = _sprite.Animation == "Left" || _sprite.Animation == "Eat left";
		_isEating = true;
		_sprite.Play(facingLeft ? "Eat left" : "Eat right");
		await ToSignal(GetTree().CreateTimer(0.35), SceneTreeTimer.SignalName.Timeout);
		_isEating = false;
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
