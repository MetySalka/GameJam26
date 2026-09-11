using Godot;
using System;
using System.Diagnostics.Metrics;

public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	Rect2 Viewport;
	int Wait;

	[Export] public Node2D Background { get; set; }
	int ProcessCounter;
	Rect2 localViewport;

	public void generateFood(PackedScene foodScene)
	{
		Fishfood food = foodScene.Instantiate<Fishfood>();
		food.Position = RandomPointInRect(GetLocalViewport());
		food.MoveVector = new Vector2(_rng.RandfRange(-0.1f, 0.1f), _rng.RandfRange(-0.1f, 0.1f));
		Background.AddChild(food);
	}

	public void ExactFood(PackedScene foodScene, Vector2 FoodPos)
	{
		Fishfood food = foodScene.Instantiate<Fishfood>();
		food.Position = FoodPos;
		food.MoveVector = new Vector2(_rng.RandfRange(-0.1f, 0.1f), _rng.RandfRange(-0.1f, 0.1f));
		Background.AddChild(food);
	}

	public override void _Ready()
	{
		Viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);

	}
	private Rect2 GetLocalViewport()
	{
		// Food positions are local to Background; include the camera's canvas transform.
		Viewport = GetViewport().GetVisibleRect();
		Transform2D viewportToLocal = Background.GetGlobalTransformWithCanvas().AffineInverse();
		return viewportToLocal * Viewport;
	}


	private Vector2 RandomPointInRect(Rect2 rect)
{
    return new Vector2(
        rect.Position.X + _rng.RandfRange(0, rect.Size.X),
        rect.Position.Y + _rng.RandfRange(0, rect.Size.Y)
    );
}
	public void prepareLevel()
	{
		GD.Print("Initial viewport is:");
		GD.Print(Viewport);
		for (int i = 0; i < 50; i++)
		{
			generateFood(foodScene);
		}
		Wait = _rng.RandiRange(300, 1200);
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FoodUpdate();
		Viewport = new Rect2(new Vector2(0, 0), GetViewport().GetVisibleRect().Size);
		ProcessCounter++;

		if (ProcessCounter >= Wait)
		{
			ProcessCounter = 0;
			Wait = _rng.RandiRange(300, 1200);
			generateFood(foodScene);
		}


	}


	public void FoodUpdate()
	{	
		localViewport = GetLocalViewport();
		// Extend each side by 10%, making the simulation 120% of the visible size.
		Vector2 margin = localViewport.Size * 0.1f;
		Rect2 simulationBounds = new Rect2(localViewport.Position - margin, localViewport.Size + margin * 2f);
		foreach (Node child in Background.GetChildren())
		{
			if (child is Fishfood food && !food.IsQueuedForDeletion())
			{
				Vector2 newFoodPosition = new Vector2(food.Position.X, food.Position.Y);
				newFoodPosition.X += food.MoveVector.X;
				newFoodPosition.Y += food.MoveVector.Y;
				food.Position = newFoodPosition;

				if (!simulationBounds.HasPoint(food.Position))
				{
					// Sample the outer rectangle until the position lies in the offscreen margin.
					Vector2 respawnPosition;
					do
					{
						respawnPosition = RandomPointInRect(simulationBounds);
					} while (localViewport.HasPoint(respawnPosition));

					ExactFood(foodScene, respawnPosition);
					food.Consume(false);
				}



			}



		}
	}
}
