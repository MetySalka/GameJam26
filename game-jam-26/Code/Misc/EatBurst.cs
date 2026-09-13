using Godot;

// Small one-shot particle burst spawned wherever food gets eaten.
// Fully code-built so no scene file is needed; just call EatBurst.SpawnAt(...).
public partial class EatBurst : Node2D
{
	private const float Lifetime = 0.4f;

	public static void SpawnAt(Node world, Vector2 globalPosition, Color color)
	{
		EatBurst burst = new EatBurst();
		world.AddChild(burst);
		burst.GlobalPosition = globalPosition;
		burst.BuildParticles(color);
	}

	private void BuildParticles(Color color)
	{
		CpuParticles2D particles = new CpuParticles2D
		{
			Emitting = true,
			OneShot = true,
			Amount = 10,
			Lifetime = Lifetime,
			Explosiveness = 1f,
			Direction = Vector2.Up,
			Spread = 180f,
			InitialVelocityMin = 60f,
			InitialVelocityMax = 140f,
			Gravity = new Vector2(0f, 260f),
			ScaleAmountMin = 2f,
			ScaleAmountMax = 4f,
			Color = color,
		};
		AddChild(particles);

		GetTree().CreateTimer(Lifetime + 0.1f).Timeout += () => QueueFree();
	}
}
