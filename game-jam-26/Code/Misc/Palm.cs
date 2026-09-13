using Godot;

public partial class Palm : Node2D
{
    [Export] public PackedScene CoconutScene;
    [Export] public float SpawnDelay = 3f;
    [Export] public int MaxCoconuts = 2;
    [Export] public Node2D SpawnPoint;
    [Export] public Area2D DetectionArea;
    [Export] public float RandomFallMinDelay = 5f;
    [Export] public float RandomFallMaxDelay = 15f;

    private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
    private int _activeCoconuts;
    private bool _playerInside;
    private bool _spawnLoopRunning;

    public override void _Ready()
    {
        DetectionArea.AreaEntered += OnAreaEntered;
        DetectionArea.AreaExited += OnAreaExited;
        RandomFallLoop();
    }

    private async void RandomFallLoop()
    {
        while (IsInstanceValid(this))
        {
            float delay = _rng.RandfRange(RandomFallMinDelay, RandomFallMaxDelay);
            await ToSignal(GetTree().CreateTimer(delay), SceneTreeTimer.SignalName.Timeout);
            if (!IsInstanceValid(this)) return;

            if (_activeCoconuts >= MaxCoconuts) continue;
            if (!GetNode<Player>("/root/Main/Player").OnLand) continue;
            SpawnCoconut();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not PlayerHitbox hitbox) return;
        if (!hitbox.GetParent<Player>().OnLand) return;
        _playerInside = true;
        TrySpawnLoop();
    }

    private void OnAreaExited(Area2D area)
    {
        if (area is PlayerHitbox) _playerInside = false;
    }

    private async void TrySpawnLoop()
    {
        if (_spawnLoopRunning) return;
        _spawnLoopRunning = true;

        while (_playerInside && _activeCoconuts < MaxCoconuts)
        {
            SpawnCoconut();
            await ToSignal(GetTree().CreateTimer(SpawnDelay), SceneTreeTimer.SignalName.Timeout);
            if (!IsInstanceValid(this)) return;
        }

        _spawnLoopRunning = false;
    }

    private void SpawnCoconut()
    {
        var coconut = CoconutScene.Instantiate<Coconut>();
        coconut.GlobalPosition = SpawnPoint.GlobalPosition;
        GetParent().AddChild(coconut);

        _activeCoconuts++;
        coconut.TreeExited += () =>
        {
            _activeCoconuts--;
            if (_playerInside) TrySpawnLoop();
        };
    }
}
