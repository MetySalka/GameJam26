using Godot;

public partial class Coconut : Area2D
{
    [Export] public float FallSpeed = 250f;
    // Distance from the spawn point (crown of the palm) down to the ground it
    // rests on. The palm overrides this with its own scaled trunk length.
    [Export] public float FallDistance = 150f;
    [Export] public float EmergeDistance = 40f;
    [Export] public float RollSpeed = 120f;
    [Export] public float RollDuration = 1.5f;
    [Export] public float KnockbackForce = 220f;

    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
    private float _startY;
    private float _groundY;
    private float _rollDirection;
    private float _rollTimeLeft;
    private bool _emerged;
    private bool _landed;
    private bool _started;

    public override void _Ready()
    {
        _rollDirection = _rng.Randf() < 0.5f ? -1f : 1f;
        ZIndex = -1;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
        // The palm adds the coconut to the tree first and only then places it at
        // the crown, so the drop target is anchored on the first frame we run.
        if (!_started)
        {
            _started = true;
            _startY = GlobalPosition.Y;
            _groundY = _startY + FallDistance;
        }

        if (!_landed)
        {
            GlobalPosition += new Vector2(0, FallSpeed * (float)delta);

            if (!_emerged && GlobalPosition.Y - _startY >= EmergeDistance)
            {
                _emerged = true;
                ZIndex = 0;
            }

            if (GlobalPosition.Y >= _groundY)
            {
                _landed = true;
                _rollTimeLeft = RollDuration;
            }
            return;
        }

        if (_rollTimeLeft <= 0) return;

        GlobalPosition += new Vector2(_rollDirection * RollSpeed * (float)delta, 0);
        _rollTimeLeft -= (float)delta;
        if (_rollTimeLeft <= 0) QueueFree();
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not PlayerHitbox hitbox) return;

        Vector2 direction = (hitbox.GlobalPosition - GlobalPosition).Normalized();
        hitbox.TakeCoconutHit(direction, KnockbackForce);
        QueueFree();
    }
}