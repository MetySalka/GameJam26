using Godot;
using System;

public partial class PlayerHitbox : Area2D
{
	[Export] private Player _player;
	private CollisionShape2D _HitUp;
	private CollisionShape2D _HitDown;
	private CollisionShape2D _HitLeft;
	private CollisionShape2D _HitRight;
	public override void _Ready()
	{
		_HitUp = GetNode<CollisionShape2D>("HitUp");
		_HitDown = GetNode<CollisionShape2D>("HitDown");
		_HitLeft = GetNode<CollisionShape2D>("HitLeft");
		_HitRight = GetNode<CollisionShape2D>("HitRight");

		_player = GetNode<Player>("../Player");
	}
	
	private void OnHitboxEntered(Area2D area)
	{
		if (area is Fishfood food)
		{
			_player.OnPlayerDeath();
		}
	}
	
	public override void _Process(double delta)
	{
		
	}
}
