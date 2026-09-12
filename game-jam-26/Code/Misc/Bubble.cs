using Godot;
using System;

public partial class Bubble : Area2D
{
    private bool _consumed = false;
    private double lifespan = 7;
    private readonly double _maxLifespan = 7;

    private const float StartScale = 0.1f;
    private const float EndScale = 3f;

    private AnimatedSprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
    }

    private bool _popped = false;

    public override void _Process(double delta)
    {
        double t = 1.0 - (lifespan / _maxLifespan); // 0 at start, 1 at end
        t = Mathf.Clamp(t, 0.0, 1.0);
        float scale = Mathf.Lerp(StartScale, EndScale, (float)t);
        GlobalScale = new Vector2(scale, scale);

        if (_popped)
            return;

        lifespan -= delta;
        if (lifespan <= 0)
        {
            _popped = true;
            _sprite.AnimationFinished += OnPopFinished;
            _sprite.Play("Pop");
        }
    }

    private void OnPopFinished()
    {
        _sprite.AnimationFinished -= OnPopFinished;
        Consume(false);
    }

    public void Consume(bool reward = true)
    {
        if (_consumed)
            return;

        _consumed = true;
        QueueFree();
        GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += 5;
    }
}