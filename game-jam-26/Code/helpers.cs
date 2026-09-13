using System;
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

	public static Rect2 GetLocalRect(Node2D node, Rect2 screenRect)
	{
		// Convert an arbitrary screen-space rect (e.g. the player's reachable area)
		// into the node's local coordinates, including the camera.
		Transform2D viewportToLocal = node.GetGlobalTransformWithCanvas().AffineInverse();
		return viewportToLocal * screenRect;
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

	public static Rect2 GetSimulationBounds(Rect2 visibleBounds, float marginFraction)
	{
		Vector2 margin = visibleBounds.Size * marginFraction;
		return new Rect2(visibleBounds.Position - margin, visibleBounds.Size + margin * 2);
	}

	public static Vector2 RandomPointInMargin(Rect2 visibleBounds, float marginFraction,
		RandomNumberGenerator rng)
	{
		Rect2 bounds = GetSimulationBounds(visibleBounds, marginFraction);
		Vector2 point;
		do
		{
			point = RandomPointInRect(bounds, rng);
		} while (visibleBounds.HasPoint(point));
		return point;
	}

	// Camera-like movement applies equally to food and enemies.
	public static void MoveWorldObjects(Vector2 offset, Background background)
	{
		foreach (Node child in background.GetChildren())
		{
			if (!child.IsQueuedForDeletion() && child is not ScrollingBGsprite)
				((Node2D)child).Position += offset;
		}
	}

	public static void MoveFood(Vector2 offset, Background background)
	{

		foreach (Node child in background.GetChildren())
		{
			if (child is Fishfood food && !food.IsQueuedForDeletion())
				food.Position += offset;
		}

	}

	public static void MoveFoodStarWars(Vector2 offset, Background background)
	{
		foreach (Node child in background.GetChildren())
		{
			if (child is Fishfood food && !food.IsQueuedForDeletion())
				food.Position += offset * food.StarWarsSpeedMultiplier;
		}

	}


	// Pushes same-type siblings apart so they don't chase the player stacked on
	// top of each other, which would let a whole cluster be dodged as if it
	// were a single enemy. Returns a displacement to add straight onto
	// GlobalPosition; the caller scales it by delta as needed.
	public static Vector2 GetSeparationFromSiblings<T>(Node2D self, Node parent, float minDistance)
		where T : Node2D
	{
		Vector2 push = Vector2.Zero;
		foreach (Node child in parent.GetChildren())
		{
			if (child == self || child is not T other || other.IsQueuedForDeletion())
				continue;
			Vector2 diff = self.GlobalPosition - other.GlobalPosition;
			float distance = diff.Length();
			if (distance < minDistance)
			{
				// Nudge in a stable arbitrary direction instead of leaving two
				// exactly-overlapping enemies stuck with a zero-length diff.
				Vector2 direction = distance > 0.001f ? diff / distance : Vector2.Right;
				push += direction * (minDistance - distance);
			}
		}
		return push;
	}

	public static float GetAngleToObject(Vector2 Coords1, Vector2 Coords2)
	{

		Vector2 diff = Coords2-Coords1;
      	return (float)(Math.Atan2(diff.Y, diff.X) * 180.0 / Math.PI);

	}

	public static void ClearScene(Background background)
	{
		foreach (Node child in background.GetChildren())
		{
			if ((child is Fishfood || child is Stika || child is Swordfih || child is Bubble || child is Puffer || child is Heal) && !child.IsQueuedForDeletion())
				child.QueueFree();
		}
	}

	public static Sprite2D AttachShadow(Node2D owner, Vector2 offset, Vector2 size)
	{
		_shadowTexture ??= CreateShadowTexture();
		Sprite2D shadow = new()
		{
			Texture = _shadowTexture,
			Position = offset,
			Scale = new Vector2(size.X / ShadowTextureSize.X, size.Y / ShadowTextureSize.Y),
		};
		owner.AddChild(shadow);
		owner.MoveChild(shadow, 0);
		return shadow;
	}

	private static readonly Vector2 ShadowTextureSize = new(128f, 64f);
	private static GradientTexture2D _shadowTexture;

	private static GradientTexture2D CreateShadowTexture()
	{
		return new GradientTexture2D
		{
			Width = (int)ShadowTextureSize.X,
			Height = (int)ShadowTextureSize.Y,
			Fill = GradientTexture2D.FillEnum.Radial,
			FillFrom = new Vector2(0.5f, 0.5f),
			FillTo = new Vector2(1f, 0.5f),
			Gradient = new Gradient
			{
				Offsets = new[] { 0f, 0.65f, 1f },
				Colors = new[]
				{
					new Color(0f, 0f, 0f, 0.34f),
					new Color(0f, 0f, 0f, 0.26f),
					new Color(0f, 0f, 0f, 0f),
				},
			},
		};
	}
}
