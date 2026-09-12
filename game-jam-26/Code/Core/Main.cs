using System.Linq;
using Godot;

public partial class Main : Node
{
	private const float FoodSpeed = 0.2f;

	[Export] public Node2D Background { get; set; }


	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private readonly PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	private readonly PackedScene tadpoleFoodScene = GD.Load<PackedScene>("res://Scenes/Misc/tadpolefood.tscn");
	private readonly PackedScene bublinaScene = GD.Load<PackedScene>("res://Scenes/Misc/bubble.tscn");
	private readonly PackedScene swordfihScene = GD.Load<PackedScene>("res://Scenes/Creatures/Enemy/swordfih.tscn");
	private Player _Player;
	private PackedScene stikaScene = GD.Load<PackedScene>("res://Scenes/Creatures/stika.tscn");
	private double _secondsUntilStikaSpawn;
	private bool _levelStarted;
	private Rect2 Viewport;
	private int Wait;
	private int _framesSinceSpawn;
	private double _secondsUntilBubbleSpawn;
	private double _secondsUntilSwordfishStream;

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
		for (int i = 0; i < 20; i++)
		{
			generateFood(GetFoodScene());
		}
		Wait = _rng.RandiRange(300, 1200);
		_secondsUntilStikaSpawn = 5;
		_levelStarted = true;
	}

	public override void _Process(double delta)
	{
		if (_levelStarted && GetNode<Health>("/root/Main/Health").HealthPlayer > 0 && _Player.Level == 0)
		{	
			_secondsUntilStikaSpawn -= delta;
			if (_secondsUntilStikaSpawn <= 0 && Background.GetChildren().OfType<Stika>().Count() < 10)
			{
				SpawnStika(Helpers.RandomPointInMargin(
					Helpers.GetLocalViewport(Background), Stika.SimulationMargin, _rng));
					

				_secondsUntilStikaSpawn = _rng.RandfRange(3, 8);
			}
		}
		_framesSinceSpawn++;



		if (_framesSinceSpawn >= Wait)
		{
			_framesSinceSpawn = 0;
			Wait = _rng.RandiRange(50, 250);
			generateFood(GetFoodScene());
		}



		if (_levelStarted && GetNode<Health>("/root/Main/Health").HealthPlayer > 0)
		{	
			_secondsUntilBubbleSpawn -= delta;
			if (_secondsUntilBubbleSpawn <= 0 && Background.GetChildren().OfType<Bubble>().Count() < 6 + _Player.Level * 2)
			{
				SpawnBubble(Helpers.RandomPointInRect(Helpers.GetLocalViewport(Background), _rng));
					

				_secondsUntilBubbleSpawn = _rng.RandfRange(1, 7);
			}
		}

		
		
		if (_levelStarted && GetNode<Health>("/root/Main/Health").HealthPlayer > 0)
		{	
			_secondsUntilSwordfishStream -= delta;
			if (_secondsUntilSwordfishStream <= 0)
			{
				
				SwordFishStream(_rng.RandfRange(0, 360), _rng.RandiRange(1, 3), Helpers.RandomPointInRect(Helpers.GetLocalViewport(Background), _rng));

				_secondsUntilSwordfishStream = _rng.RandfRange(4, 12);
			}
		}

		




	}

	// Position is the point the stream passes through in viewport pixels.
	// Angle is clockwise degrees from right; the row starts 2,000 pixels before position.
	public void SwordFishStream(float Angle, int width, Vector2 position)
	{
		const float spacing = 40f;
		const float spawnDistance = 2000f;
		const float speed = 600f;
		Vector2 heading = Vector2.Right.Rotated(Mathf.DegToRad(Angle));
		Vector2 spawnCenter = position - heading * spawnDistance;
		Vector2 sideways = new Vector2(-heading.Y, heading.X);
		Transform2D viewportToLocal = Background.GetGlobalTransformWithCanvas().AffineInverse();
		Vector2 localHeading = viewportToLocal * (position + heading) - viewportToLocal * position;

		for (int i = 0; i < width; i++)
		{
			float offset = (i - (width - 1) * 0.5f) * spacing;
			Swordfih swordfish = swordfihScene.Instantiate<Swordfih>();
			swordfish.Position = viewportToLocal * (spawnCenter + sideways * offset);
			swordfish.Rotation = localHeading.Angle();
			swordfish.Velocity = localHeading * speed;
			Background.AddChild(swordfish);
			swordfish.GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play("Right");
		}
	}

public void SpawnBubble(Vector2 position)
  {
	  Bubble bubble = bublinaScene.Instantiate<Bubble>();
	  bubble.Position = position;
	  Background.AddChild(bubble);
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
		for (int i = 0; i < 10; i++)
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
