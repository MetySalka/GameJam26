using Godot;

public partial class Fishfood : Area2D
{
	private bool _consumed;

	// Local movement applied by Main each frame.
	public Vector2 MoveVector { get; set; }

	// Pick once so each piece stays consistently faster or slower.
	public float StarWarsSpeedMultiplier { get; private set; }

	public override void _Ready()
	{
		StarWarsSpeedMultiplier = (float)GD.RandRange(0.1, 1);
	}


	public void Consume(bool Do = true)
	{
		// QueueFree runs later, so ignore duplicate pickups in the same frame.

		if (_consumed)
			return;
		_consumed = true;
		
		if (Do)
		{
			GetNode<TextureProgressBar>("/root/Main/ProgressBar/TextureProgressBar").Value += 1;
		}
		
		QueueFree();
		
	}
}
