using System.Linq;
using Godot;

public partial class Main : Node
{
	private const float FoodSpeed = 0.1f;

	[Export] public Node2D Background { get; set; }


	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private readonly PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	private readonly PackedScene tadpoleFoodScene = GD.Load<PackedScene>("res://Scenes/Misc/tadpolefood.tscn");
	private Player _Player;

	private PackedScene stikaScene = GD.Load<PackedScene>("res://Scenes/Creatures/stika.tscn");
	private double _secondsUntilStikaSpawn;
	private bool _levelStarted;
	private Rect2 Viewport;
	private int Wait;
	private int _framesSinceSpawn;

	public override void _Ready()
	{
		Viewport = GetViewport().GetVisibleRect();
		_Player = GetNode<Player>("Player");
	}

	public void prepareLevel()
	{
		_levelStarted = false;
		global::Background world = (global::Background)Background;
		Helpers.ClearScene(world);
		world.ResetForNewRun();
		_Player.ResetForNewRun();
		GetNode<GameJam26.Code.UI.ProgressBar>("ProgressBar").ResetForNewRun();
		GetNode<Control>("GameOver").Hide();
		_framesSinceSpawn = 0;
		for (int i = 0; i < 500; i++)
		{
			generateFood(GetFoodScene());
		}
		Wait = _rng.RandiRange(300, 1200);
		_secondsUntilStikaSpawn = 10;
		_levelStarted = true;
	}

	public override void _Process(double delta)
	{
		if (_levelStarted)
		{	
			_secondsUntilStikaSpawn -= delta;
			if (_secondsUntilStikaSpawn <= 0 && Background.GetChildren().OfType<Stika>().Count() < 10)
			{
				SpawnStika(Helpers.RandomPointInMargin(
					Helpers.GetLocalViewport(Background), Stika.SimulationMargin, _rng));
					

				_secondsUntilStikaSpawn = _rng.RandfRange(1, 7);
			}
		}
		_framesSinceSpawn++;

		// Keep the existing frame-based spawn timing.
		if (_framesSinceSpawn >= Wait)
		{
			_framesSinceSpawn = 0;
			Wait = _rng.RandiRange(300, 1200);
			generateFood(GetFoodScene());
		}
	}



  public Stika SpawnStika(Vector2 position)
  {
	GD.Print("StikaSpawned");
      Stika stika = stikaScene.Instantiate<Stika>();
      stika.Position = position;
	  stika.Target = GetNode<Player>("Player");
      Background.AddChild(stika);
      return stika;
  }



	private PackedScene GetFoodScene()
	{
		return _Player.Level >= 1 ? tadpoleFoodScene : foodScene;
	}

	public void SpawnLevelUpFood()
	{
		PackedScene scene = GetFoodScene();
		Rect2 visibleBounds = Helpers.GetLocalViewport(Background);
		for (int i = 0; i < 50; i++)
			ExactFood(scene, Helpers.RandomPointInMargin(visibleBounds, Fishfood.SimulationMargin, _rng));
	}

	public void generateFood(PackedScene scene)
	{
		Rect2 visibleBounds = Helpers.GetLocalViewport(Background);
		ExactFood(scene, Helpers.RandomPointInRect(visibleBounds, _rng));
	}


	public void ExactFood(PackedScene scene, Vector2 position)
	{
		Fishfood food = scene.Instantiate<Fishfood>();
		food.Spawner = this;
		food.SourceScene = scene;
		food.Position = position;
		food.MoveVector = new Vector2(
			_rng.RandfRange(-FoodSpeed, FoodSpeed),
			_rng.RandfRange(-FoodSpeed, FoodSpeed));
		Background.AddChild(food);
	}
}
