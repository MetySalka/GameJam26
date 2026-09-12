using Godot;

public partial class Puffer : Area2D
{
	public const float SimulationMargin = 0.15f;
	[Export] public float FollowSpeed { get; set; } = 60f;
	[Export] public float PuffRadius { get; set; } = 60f;
	[Export] public float BounceSpeed { get; set; } = 220f;
	[Export] public float SpinSpeedDegrees { get; set; } = 140f;
	public Player Target { get; set; }
	public bool HasPuffed { get; private set; }

	private AnimatedSprite2D _sprite;
	private Vector2 _bounceDirection;
	private float _spinDirection;
	private bool _hasEnteredViewport;

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
			_sprite.FlipH = toPlayer.X > 0f;
			// Stop at the trigger radius so even a long frame cannot overshoot the player.
			float travel = Mathf.Min(     Mathf.Max(0f, toPlayer.Length() - PuffRadius * 0.3f), FollowSpeed * seconds);
			GlobalPosition += toPlayer.Normalized() * travel;
			if (GlobalPosition.DistanceTo(Target.GlobalPosition) <= PuffRadius * 0.35f - 0.001f)
				Puff();
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
		_bounceDirection = (GlobalPosition - Target.GlobalPosition).Normalized();
		if (_bounceDirection.IsZeroApprox())
			_bounceDirection = Vector2.Right;
		_spinDirection = _bounceDirection.X >= 0f ? 1f : -1f;
		_sprite.Play("Bloat");
		Target.OnPlayerHit();
	}
}
