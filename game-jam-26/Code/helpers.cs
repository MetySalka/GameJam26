using Godot;

// Shared geometry helpers for gameplay scripts.
public static class Helpers
{
	public static Rect2 GetLocalViewport(Node2D node)
	{
		// Convert screen bounds into the node's coordinates, including the camera.
		Transform2D viewportToLocal = node.GetGlobalTransformWithCanvas().AffineInverse();
		return viewportToLocal * node.GetViewport().GetVisibleRect();
	}

	public static Vector2 RandomPointInRect(Rect2 rect, RandomNumberGenerator rng)
	{
		return new Vector2(
			rng.RandfRange(rect.Position.X, rect.End.X),
			rng.RandfRange(rect.Position.Y, rect.End.Y));
	}

	public static Vector2 ClampToRect(Vector2 position, Rect2 rect)
	{
		return new Vector2(
			Mathf.Clamp(position.X, rect.Position.X, rect.End.X),
			Mathf.Clamp(position.Y, rect.Position.Y, rect.End.Y));
	}

	public static void MoveFood(Vector2 offset, Background background)
	{

		foreach (Node child in background.GetChildren())
		{
			if (child is Fishfood food && !food.IsQueuedForDeletion())
				food.Position += offset;
		}

	}
}
