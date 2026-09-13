using Godot;

public partial class CoconutSpawnersa : Area2D
{
    [Export] public PackedScene CoconutScene;
    [Export] public float SpawnDelay = 2f;
    [Export] public int MaxCoconuts = 2;
    [Export] public Node2D SpawnPoint;      // where coconuts appear (e.g. top of palm)

    private int _alive;
    private bool _playerInside;
    private bool _cooldownRunning;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited  += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Player) return;
        _playerInside = true;
        TryStartCooldown();
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player) _playerInside = false;
    }

    private async void TryStartCooldown()
    {
        if (_cooldownRunning || _alive >= MaxCoconuts) return;
        _cooldownRunning = true;

        await ToSignal(GetTree().CreateTimer(SpawnDelay), SceneTreeTimer.SignalName.Timeout);
        if (!IsInstanceValid(this)) return;
        _cooldownRunning = false;

        if (_alive >= MaxCoconuts) return;
        Spawn();

        // Player still standing under the tree? Keep dropping.
        if (_playerInside) TryStartCooldown();
    }

    private void Spawn()
    {
        var coconut = CoconutScene.Instantiate<Coconut>();
        coconut.GlobalPosition = SpawnPoint.GlobalPosition;
        GetParent().AddChild(coconut);

        _alive++;
        coconut.TreeExited += () => _alive--;   // freed → count drops
    }
}