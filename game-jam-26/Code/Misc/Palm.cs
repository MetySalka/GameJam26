using Godot;

public partial class Palm : Node2D
{
    [Export] public PackedScene CoconutScene;
    [Export] public Marker2D SpawnPoint;
    [Export] private Player _lizardNode;
    private AnimatedSprite2D _lizard;
    public bool LevelDry = false;
    private bool _spawned = false;

    public override void _Ready()
    {
        _lizard = _lizardNode.GetNode<AnimatedSprite2D>("LizardSprite");
        _lizard.AnimationChanged += OnLizardSpriteChange;
    }

    // Connect TriggerArea's body_entered signal to this in the editor.
    private void OnBodyEntered(Node2D body)
    {
        if (!LevelDry || _spawned) return;
        _spawned = true;
        CallDeferred(MethodName.SpawnCoconut);
    }

    public void OnLizardSpriteChange()
    {
        GD.Print("Sucho");
        LevelDry = true;
    }

    private void SpawnCoconut()
    {
        var coconut = CoconutScene.Instantiate<Area2D>();
        GetTree().CurrentScene.AddChild(coconut);
        coconut.GlobalPosition = SpawnPoint.GlobalPosition;
        coconut.ZIndex = 100;
    }
}