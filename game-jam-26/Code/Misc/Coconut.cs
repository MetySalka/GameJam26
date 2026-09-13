using Godot;

public partial class Coconut : Area2D
{
    [Export] public float Gravity = 400f;
    [Export] public float FallDistance = 800f;
    [Export] public float LifeAfterLanding = 1.5f;

    private float _velocity;
    private float _groundY;
    private bool _landed;

    public override void _Ready()
    {
        _groundY = GlobalPosition.Y + FallDistance;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
    }

    public override void _Process(double delta)
    {
        if (_landed) return;

        _velocity += Gravity * (float)delta;
        GlobalPosition += new Vector2(0, _velocity * (float)delta);

        if (GlobalPosition.Y >= _groundY)
        {
            _landed = true;
            GetTree().CreateTimer(LifeAfterLanding).Timeout += QueueFree;
        }
    }
}