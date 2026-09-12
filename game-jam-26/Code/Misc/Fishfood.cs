using Godot;


public partial class Fishfood : Area2D
{
	private bool _consumed;
	[Export] private ProgressBar _progressBar;
	[Export] private TextureProgressBar _textureBar;
	// Local movement applied by Main each frame.
	public Vector2 MoveVector { get; set; }

	public void Consume(bool Do = true)
	{
		// QueueFree runs later, so ignore duplicate pickups in the same frame.
		if (_consumed)
			return;
		
		_consumed = true;
		QueueFree();

	}
}
