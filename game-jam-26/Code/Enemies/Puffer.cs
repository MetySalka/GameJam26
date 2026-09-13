using Godot;

public partial class Puffer : Area2D
{
	public const float SimulationMargin = 0.15f;
	[Export] public float FollowSpeed { get; set; } = 60f;
	[Export] public float PuffRadius { get; set; } = 60f;
	[Export] public float BounceSpeed { get; set; } = 220f;
	[Export] public float SpinSpeedDegrees { get; set; } = 140f;
	[Export] public float WarnRadiusMultiplier { get; set; } = 0.6f;
	[Export] public float WarnDuration { get; set; } = 0.35f;
	public Player Target { get; set; }
	public bool HasPuffed { get; private set; }

	private AnimatedSprite2D _sprite;
	private Vector2 _bounceDirection;
	private float _spinDirection;
	private bool _hasEnteredViewport;
	private bool _isWarning;
	private float _warnTimeLeft;
	private double _warnPulseClock;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.Stop();
		_sprite.Animation = "Bloat";
		_sprite.Frame = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsQueuedForDeletion())
			return;
		float seconds = (float)delta;
		if (HasPuffed)
		{
			GlobalPosition += _bounceDirection * BounceSpeed * seconds;
			RotationDegrees += _spinDirection * SpinSpeedDegrees * seconds;
		}
		else
		{
			if (!GodotObject.IsInstanceValid(Target) || Target.IsQueuedForDeletion())
			{
				QueueFree();
				return;
			}
			Vector2 toPlayer = Target.GlobalPosition - GlobalPosition;
			// Point the sprite at the player; flip vertically past the halfway turn so it
			// never looks upside down while swimming left. The art faces tail-first, so
			// add a half turn to have the nose lead instead.
			float angle = Mathf.Wrap(toPlayer.Angle() + Mathf.Pi, -Mathf.Pi, Mathf.Pi);
			_sprite.FlipV = Mathf.Abs(angle) > Mathf.Pi / 2f;
			_sprite.Rotation = angle;

			float distanceToPlayer = toPlayer.Length();
			float warnRadius = PuffRadius * WarnRadiusMultiplier;

			if (!_isWarning && distanceToPlayer <= warnRadius)
			{
				_isWarning = true;
				_warnTimeLeft = WarnDuration;
				_warnPulseClock = 0;
			}
			else if (_isWarning && distanceToPlayer > warnRadius)
			{
				// The player escaped the danger zone in time; call off the attack.
				_isWarning = false;
				_sprite.Modulate = Colors.White;
			}

			if (_isWarning)
			{
				// Freeze in place while telegraphing so the player gets a fair, readable window to flee.
				_warnPulseClock += delta;
				float pulse = 0.5f + 0.5f * Mathf.Sin((float)_warnPulseClock * 25f);
				_sprite.Modulate = new Color(1f, Mathf.Lerp(1f, 0.15f, pulse), Mathf.Lerp(1f, 0.15f, pulse));

				_warnTimeLeft -= seconds;
				if (_warnTimeLeft <= 0f)
					Puff();
			}
			else
			{
				// Stop at the trigger radius so even a long frame cannot overshoot the player.
				float travel = Mathf.Min(Mathf.Max(0f, distanceToPlayer - PuffRadius * 0.3f), FollowSpeed * seconds);
				GlobalPosition += toPlayer.Normalized() * travel;
			}
		}

		// Use sprite bounds so the whole fish leaves the screen before it is removed.
		Vector2 size = _sprite.SpriteFrames.GetFrameTexture(_sprite.Animation, _sprite.Frame).GetSize();
		Vector2 origin = _sprite.Offset - (_sprite.Centered ? size * 0.5f : Vector2.Zero);
		Rect2 screenBounds = _sprite.GetGlobalTransformWithCanvas() * new Rect2(origin, size);
		bool onScreen = screenBounds.Intersects(GetViewport().GetVisibleRect());
		if (onScreen)
			_hasEnteredViewport = true;
		else if (_hasEnteredViewport || HasPuffed)
			QueueFree();
	}

	private void Puff()
	{
		HasPuffed = true;
		_isWarning = false;
		_sprite.Modulate = Colors.White;
		_bounceDirection = (GlobalPosition - Target.GlobalPosition).Normalized();
		if (_bounceDirection.IsZeroApprox())
			_bounceDirection = Vector2.Right;
		_spinDirection = _bounceDirection.X >= 0f ? 1f : -1f;
		_sprite.Play("Bloat");
		Target.OnPlayerHit(1.6f, new Color(1f, 0.6f, 0.1f));
	}
}
