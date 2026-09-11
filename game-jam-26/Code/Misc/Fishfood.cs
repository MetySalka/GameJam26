using Godot;
using System;

public partial class Fishfood : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.



	public override void _Process(double delta)
    {

    }

    private bool _consumed = false;
    public Vector2 MoveVector {get; set;}

    public void Consume(bool Do = true)
    {
        // Guard against being eaten twice in the same frame
        // (e.g. multiple overlap checks before QueueFree() actually removes it)
        if (_consumed)
            return;

        _consumed = true;

        // Optional: play sound/particles before removal
        // AudioManager.Instance.PlaySfx("eat");

        QueueFree();
    }
}
