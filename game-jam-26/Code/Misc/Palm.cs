using Godot;

public partial class Palm : Area2D
{
    [Export] public PackedScene CoconutScene;
    [Export] public float SpawnDelay = 2f;
    [Export] public int MaxCoconuts = 2;
    [Export] public Node2D SpawnPoint;      // where coconuts appear (e.g. top of palm)
    // Distance from SpawnPoint down to the sand at the trunk base, measured in
    // unscaled palm pixels; multiplied by this palm's scale when a coconut drops.
    [Export] public float GroundFallDistance = 100f;

    private int _alive;
    private bool _playerInside;
    private bool _cooldownRunning;

    public override void _Ready()
    {
        // The player body has no collision shape of its own, so the palm
        // watches the player's hitbox area entering this zone instead.
        AreaEntered += OnAreaEntered;
        AreaExited  += OnAreaExited;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is not PlayerHitbox hitbox) return;
        // Only the dodo walks the beach; while the player is still in the water
        // the palms are hidden and must not drop anything.
        if (!hitbox.GetParent<Player>().OnLand) return;
        _playerInside = true;
        TryStartCooldown();
    }

    private void OnAreaExited(Area2D area)
    {
        if (area is PlayerHitbox) _playerInside = false;
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
        // Stop the fall at the sand under this palm instead of a fixed drop.
        coconut.FallDistance = GroundFallDistance * GlobalScale.Y;
        GetParent().AddChild(coconut);
        // Position it after it is in the tree: on a parentless node the value
        // would be stored as a local offset and this palm's offset would stack.
        coconut.GlobalPosition = SpawnPoint.GlobalPosition;

        _alive++;
        coconut.TreeExited += () => _alive--;   // freed → count drops
    }
}