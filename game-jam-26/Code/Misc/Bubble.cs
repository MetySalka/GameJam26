using Godot;

public partial class Bubble : Area2D
{
    private bool _consumed = false;
    private double lifespan = 7;
    private readonly double _maxLifespan = 7;

    private const float StartScale = 0.9f;
    private const float EndScale = 3f;
    private const float SimulationMargin = 0.1f;
    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
    private Node2D _world;

    private AnimatedSprite2D _sprite;

    public override void _Ready()
    {
        _sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        _world = GetParent<Node2D>();
    }

    private bool _popped = false;

    public override void _Process(double delta)
    {
        if (_consumed || IsQueuedForDeletion())
            return;

        Rect2 visibleBounds = Helpers.GetLocalViewport(_world);
        Rect2 simulationBounds = Helpers.GetSimulationBounds(visibleBounds, SimulationMargin);
        if (!simulationBounds.HasPoint(Position))
        {
            // Recycle escaped bubbles without awarding points or changing their count.
            Position = Helpers.RandomPointInMargin(visibleBounds, SimulationMargin, _rng);
            if (_popped)
                _sprite.AnimationFinished -= OnPopFinished;
            _popped = false;
            lifespan = _maxLifespan;
            Scale = Vector2.One * StartScale;
            _sprite.Stop();
            _sprite.Play("Idle");
            return;
        }

        double t = 1.0 - (lifespan / _maxLifespan); // 0 at start, 1 at end
        t = Mathf.Clamp(t, 0.0, 1.0);
        float scale = Mathf.Lerp(StartScale, EndScale, (float)t);
        Scale = new Vector2(scale, scale);

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
        if (reward)
        {
            GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += 5;
            EatBurst.SpawnAt(_world, GlobalPosition, new Color(0.6f, 0.85f, 1f));
        }
        QueueFree();
    }
}
