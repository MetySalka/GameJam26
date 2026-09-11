using Godot;
using System;

public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	Rect2 Viewport;




	public void generateFood(PackedScene foodScene)
	{
		Fishfood food = foodScene.Instantiate<Fishfood>();
        food.Position = new Vector2(Viewport.Size.X * _rng.RandfRange(0,1), Viewport.Size.Y * _rng.RandfRange(0,1));
		food.MoveVector = new Vector2(_rng.RandfRange(-0.1f,0.1f),_rng.RandfRange(-0.1f,0.1f));
        AddChild(food);
	}
 
	public void ExactFood(PackedScene foodScene, Vector2 FoodPos)
	{
		Fishfood food = foodScene.Instantiate<Fishfood>();
		food.Position = FoodPos;
		food.MoveVector = new Vector2(_rng.RandfRange(-0.05f,0.05f),_rng.RandfRange(-0.05f,0.05f));
		AddChild(food);
	}

	public override void _Ready()
	{
		Viewport = new Rect2(new Vector2(0,0), GetViewport().GetVisibleRect().Size);

	}

	public void prepareLevel()
	{
		GD.Print(Viewport);
		for(int i = 0; i < 1200; i++) {
			generateFood(foodScene);
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Viewport = new Rect2(new Vector2(0,0), GetViewport().GetVisibleRect().Size);
		FoodUpdate();

	}


	public void FoodUpdate()
	{
		foreach(Node child in GetChildren())
		{ if (child is Fishfood food)
			{
				Vector2 newFoodPosition = new Vector2(food.Position.X, food.Position.Y);
				newFoodPosition.X += food.MoveVector.X;
				newFoodPosition.Y += food.MoveVector.Y;
				food.Position = newFoodPosition;

				if(!Viewport.HasPoint(food.GlobalPosition))
				{
					ExactFood(foodScene, food.Position);
					food.Consume(false);
				}
				


			}
		


}
}
}
