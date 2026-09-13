using Godot;
using System;

public partial class Health : TextureProgressBar
{
	[Export] private Player _player;
	[Export] private TextureProgressBar _healthStat;
	// Single source of truth for how many hits the player survives: the number of
	// hearts drawn is _healthStat.MaxValue, so HealthPlayer always starts there
	// instead of a separately hardcoded number that could drift out of sync.
	public int MaxHealth { get; private set; }
	public int HealthPlayer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MaxHealth = (int)_healthStat.MaxValue;
		HealthPlayer = MaxHealth;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_healthStat.Value = HealthPlayer;
	}
}
