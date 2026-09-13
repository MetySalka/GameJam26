using Godot;

public partial class Coconut : Area2D
{
    [Export] public float FallSpeed = 250f;
    [Export] public float FallDistance = 800f;
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
    private Sprite2D _shadow;

    public override void _Ready()
    {
        _startY = GlobalPosition.Y;
        _groundY = _startY + FallDistance;
        _rollDirection = _rng.Randf() < 0.5f ? -1f : 1f;
        ZIndex = -1;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
        _shadow = Helpers.AttachShadow(this, new Vector2(0, 24), new Vector2(62, 16));
        _shadow.Hide();
        AreaEntered += OnAreaEntered;
    }

    public override void _Process(double delta)
    {
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
                _shadow.Show();
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