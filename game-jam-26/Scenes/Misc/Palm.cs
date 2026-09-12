using Godot;
using System;

public partial class Palm : Node2D
{
	[Export] public PackedScene CoconutScene;
	[Export] public Marker2D SpawnPoint;

	// Connect TriggerArea's body_entered signal to this in the editor.
	private void OnBodyEntered(Node2D body)
	{
		var coconut = CoconutScene.Instantiate<RigidBody2D>();
		GetTree().CurrentScene.AddChild(coconut);
		coconut.GlobalPosition = SpawnPoint.GlobalPosition;
	}
}
