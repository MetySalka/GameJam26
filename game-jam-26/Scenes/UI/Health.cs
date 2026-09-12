using Godot;
using System;

public partial class Health : TextureProgressBar
{
	[Export] private Player _player;
	[Export] private TextureProgressBar _healthStat;
	public int HealthPlayer = 5;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_healthStat.Value = HealthPlayer;
	}
}
