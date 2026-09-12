using Godot;

public partial class Swordfih : Area2D
{
	public Vector2 Velocity { get; set; }
	private double _secondsRemaining = 15;

	public override void _Process(double delta)
	{
		Position += Velocity * (float)delta;
		// Allow time to fly in from outside the screen before removing the fish.
		_secondsRemaining -= delta;
		if (_secondsRemaining <= 0)
			QueueFree();
	}
}
