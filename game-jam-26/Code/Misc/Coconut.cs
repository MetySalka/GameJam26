using Godot;

public partial class Coconut : Area2D
{
    [Export] public float Gravity = 900f;
    [Export] public float FallDistance = 200f;   // how far below the spawn it stops

    private float _velocity;
    private float _groundY;

    public override void _Ready()
    {
        _groundY = GlobalPosition.Y + FallDistance;
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("default");
    }

    public override void _Process(double delta)
    {
        if (GlobalPosition.Y >= _groundY) return;   // landed

        _velocity += Gravity * (float)delta;
        GlobalPosition += new Vector2(0, _velocity * (float)delta);
    }
}