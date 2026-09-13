using Godot;

// Beach-only enemy. The art only has a single forward walking pose and a single
// forward attack pose (no left/right/up/down variants), so instead of swapping
// animations per direction its sprite is rotated to face the player.
public partial class Crab : Area2D
{
	public const float SimulationMargin = 0.15f;

	[Export] public float MoveSpeed { get; set; } = 55f;
	[Export] public float AttackRange { get; set; } = 48f;
	[Export] public float AttackInterval { get; set; } = 1.1f;
	// Fraction of AttackInterval into the swing where the claws actually connect.
	[Export] public float AttackHitFraction { get; set; } = 0.5f;

	public Player Target { get; set; }

	private AnimatedSprite2D _sprite;
	private bool _isAttacking;
	private float _attackTimeLeft;
	private bool _hasHitThisSwing;

	public override void _Ready()
	{
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_sprite.Play("Forward");
		Helpers.AttachShadow(this, new Vector2(0, 12), new Vector2(28, 9));
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!GodotObject.IsInstanceValid(Target) || Target.IsQueuedForDeletion() || !Target.OnLand)
		{
			QueueFree();
			return;
		}

		Vector2 toPlayer = Target.GlobalPosition - GlobalPosition;
		float distance = toPlayer.Length();
		float seconds = (float)delta;

		if (_isAttacking)
		{
			_attackTimeLeft -= seconds;
			if (!_hasHitThisSwing && _attackTimeLeft <= AttackInterval * (1f - AttackHitFraction))
			{
				_hasHitThisSwing = true;
				// The claws are the weapon: only land the hit if the player is still
				// roughly in front of the crab when the swing connects.
				if (distance <= AttackRange * 1.4f)
					Target.OnPlayerHit(1f, new Color(1f, 0.35f, 0.2f));
			}
			if (_attackTimeLeft <= 0f)
			{
				_isAttacking = false;
				_sprite.Play("Forward");
			}
			return;
		}

		if (distance <= AttackRange)
		{
			_isAttacking = true;
			_hasHitThisSwing = false;
			_attackTimeLeft = AttackInterval;
			FaceDirection(toPlayer);
			GetNode<AudioStreamPlayer>("CrabClap").Play();
			_sprite.Play("Attack");
			return;
		}

		FaceDirection(toPlayer);
		GlobalPosition += toPlayer.Normalized() * MoveSpeed * seconds;
	}

	// Assumption: the art is drawn facing up (claws toward the top of the frame) at
	// rotation 0. Flip the sign of the offset below if the sprite actually faces down.
	private void FaceDirection(Vector2 direction)
	{
		_sprite.Rotation = direction.Angle() + Mathf.Pi / 2f;
	}

	public void KYS()
	{
		QueueFree();
	}
}
