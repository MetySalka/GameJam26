using System.Linq;
using System.Collections.Generic;
using Godot;

public partial class Main : Node
{
	private const float FoodSpeed = 0.2f;

	[Export] public Node2D Background { get; set; }


	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private readonly PackedScene foodScene = GD.Load<PackedScene>("res://Scenes/Misc/fishfood.tscn");
	private readonly PackedScene shrimpFoodScene = GD.Load<PackedScene>("res://Scenes/Misc/shrimpfood.tscn");
	private readonly PackedScene bublinaScene = GD.Load<PackedScene>("res://Scenes/Misc/bubble.tscn");
	private readonly PackedScene swordfihScene = GD.Load<PackedScene>("res://Scenes/Creatures/Enemy/swordfih.tscn");
	private readonly PackedScene pufferScene = GD.Load<PackedScene>("res://Scenes/Creatures/Enemy/puffer.tscn");
	private Player _Player;
	private PackedScene stikaScene = GD.Load<PackedScene>("res://Scenes/Creatures/stika.tscn");
	private readonly Dictionary<EnemyKind, double> _spawnTimers = new();
	private int _spawnSettingsLevel = -1;
	private bool _levelStarted;
	private Rect2 Viewport;
	private int Wait;
	private int _framesSinceSpawn;
	private ColorRect _fihWarn;

	private Camera2D camera;
	private sealed class SwordfishWarning
	{
		public ColorRect Rect { get; init; }
		public List<Swordfih> Fish { get; } = new();
	}
	private readonly List<SwordfishWarning> _swordfishWarnings = new();
	private readonly Stack<ColorRect> _availableSwordfishWarnings = new();

	public override void _Ready()
	{
		Viewport = GetViewport().GetVisibleRect();
		_Player = GetNode<Player>("Player");
		_fihWarn = GetNode<ColorRect>("FihWarn");
		_fihWarn.MouseFilter = Control.MouseFilterEnum.Ignore;
		_fihWarn.Hide();
		_availableSwordfishWarnings.Push(_fihWarn);
		camera = GetNode<Camera2D>("Camera2D");

	}
	// Debug helper: skips straight to the given evolution level after a fresh
	// prepareLevel(), reusing the normal level-up logic (sprite swap, land phase, etc.).
	public void DebugSetLevel(int level)
	{
		for (int i = 0; i < level; i++)
			_Player.LevelUp();
	}

	public void prepareLevel()
	{
		_levelStarted = false;
		HideSwordfishWarning();
		global::Background world = (global::Background)Background;
		Helpers.ClearScene(world);
		world.ResetForNewRun();
		_Player.ResetForNewRun();
		GetNode<GameJam26.Code.UI.ProgressBar>("ProgressBar").ResetForNewRun();
		GetNode<Control>("ScreenUI/GameOver").Hide();
		_framesSinceSpawn = 0;
		for (int i = 0; i < 20; i++)
		{
			generateFood(GetFoodScene());
		}
		Wait = 100;
		ResetEnemySpawnTimers();
		_levelStarted = true;
	}

	public override void _Process(double delta)
	{
		UpdateSwordfishWarning();
		if (_levelStarted && GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer > 0)
			UpdateEnemySpawns(delta);

		_framesSinceSpawn++;
		if (_framesSinceSpawn >= Wait)
		{
			_framesSinceSpawn = 0;
			Wait = _rng.RandiRange(30, 240);
			generateFood(GetFoodScene());
		}
	}

	private void ResetEnemySpawnTimers()
	{
		_spawnSettingsLevel = _Player.Level;
		foreach (EnemyKind enemy in System.Enum.GetValues<EnemyKind>())
			_spawnTimers[enemy] = EnemySpawnTable.Select(enemy, _Player.Level).InitialDelay;
	}

	private void UpdateEnemySpawns(double delta)
	{
		if (_spawnSettingsLevel != _Player.Level)
			ResetEnemySpawnTimers();

		foreach (EnemyKind enemy in System.Enum.GetValues<EnemyKind>())
		{
			EnemySpawnRule rule = EnemySpawnTable.Select(enemy, _Player.Level);
			if (rule.MaxCount == 0)
				continue;
			_spawnTimers[enemy] -= delta;
			if (_spawnTimers[enemy] > 0)
				continue;

			int alive = Background.GetChildren().Count(child => !child.IsQueuedForDeletion() && (enemy switch
			{
				EnemyKind.Pike => child is Stika,
				EnemyKind.Swordfish => child is Swordfih,
				EnemyKind.Bubble => child is Bubble,
				EnemyKind.Pufferfish => child is Puffer,
				_ => false
			}));
			int available = rule.MaxCount - alive;
			if (available <= 0)
				continue;

			// Trim the last batch to the available capacity so streams cannot exceed the cap.
			int count = System.Math.Min(available, _rng.RandiRange(rule.MinBatch, rule.MaxBatch));
			if (enemy == EnemyKind.Swordfish)
			{
				Rect2 viewport = GetViewport().GetVisibleRect();
				Rect2 targetArea = new Rect2(viewport.GetCenter() - viewport.Size * 0.35f, viewport.Size * 0.7f);
				SwordFishStream(_rng.RandfRange(0, 360), count, Helpers.RandomPointInRect(targetArea, _rng));
			}
			else
			{
				Rect2 bounds = Helpers.GetLocalViewport(Background);
				for (int i = 0; i < count; i++)
				{
					if (enemy == EnemyKind.Pike)
						SpawnStika(Helpers.RandomPointInMargin(bounds, Stika.SimulationMargin, _rng));
					else if (enemy == EnemyKind.Bubble)
						SpawnBubble(Helpers.RandomPointInRect(bounds, _rng));
					else if (enemy == EnemyKind.Pufferfish)
						SpawnPuffer(Helpers.RandomPointInMargin(bounds, Puffer.SimulationMargin, _rng));
				}
			}
			_spawnTimers[enemy] = _rng.RandfRange(rule.MinSeconds, rule.MaxSeconds);
		}
	}

	// Position is the point the stream passes through in viewport pixels.
	// Angle is clockwise degrees from right; the row starts 2,000 pixels before position.
	public void SwordFishStream(float Angle, int width, Vector2 position)
	{
		if (width <= 0 || _Player.OnLand)
			return;
		ColorRect rect;
		if (_availableSwordfishWarnings.Count > 0)
			rect = _availableSwordfishWarnings.Pop();
		else
		{
			rect = (ColorRect)_fihWarn.Duplicate();
			rect.Hide();
			_fihWarn.GetParent().AddChild(rect);
		}
		var warning = new SwordfishWarning { Rect = rect };
		_swordfishWarnings.Add(warning);
		float spacing = 0f;

		for (int i = 0; i < width; i++)
		{
			Swordfih swordfish = swordfihScene.Instantiate<Swordfih>();
			Background.AddChild(swordfish);
			swordfish.SpawnInStream(Angle, position, 0f);
			if (i == 0)
				spacing = 50f * Background.GetGlobalTransformWithCanvas().X.Length();
			float offset = (i - (width - 1) * 0.5f) * spacing;
			swordfish.SpawnInStream(Angle, position, offset);
			warning.Fish.Add(swordfish);
		}

		// Center the rectangle on the crossing point, with its long axis along the stream.
		Transform2D screenToCanvas = rect.GetCanvasTransform().AffineInverse();
		Vector2 heading = Vector2.Right.Rotated(Mathf.DegToRad(Angle));
		Vector2 sideways = new Vector2(-heading.Y, heading.X);
		Vector2 localHeading = screenToCanvas.X * heading.X + screenToCanvas.Y * heading.Y;
		Vector2 localSideways = screenToCanvas.X * sideways.X + screenToCanvas.Y * sideways.Y;
		rect.Size = new Vector2(2300f, spacing * width);
		rect.PivotOffset = rect.Size * 0.5f;
		rect.Scale = new Vector2(localHeading.Length(), localSideways.Length());
		rect.Rotation = localHeading.Angle();
		rect.Position = screenToCanvas * position - rect.PivotOffset;
		rect.Show();
		UpdateSwordfishWarning();
	}

	private void UpdateSwordfishWarning()
	{
		if (_swordfishWarnings.Count == 0)
			return;
		if (GetNode<Health>("/root/Main/ScreenUI/Health").HealthPlayer <= 0)
		{
			HideSwordfishWarning();
			return;
		}
		for (int i = _swordfishWarnings.Count - 1; i >= 0; i--)
		{
			SwordfishWarning warning = _swordfishWarnings[i];
			warning.Fish.RemoveAll(fish => !GodotObject.IsInstanceValid(fish) || fish.IsQueuedForDeletion());
			if (warning.Fish.Count == 0 || warning.Fish.Any(fish => fish.IsAboutToEnterViewport()))
			{
				warning.Fish.ForEach(fish => fish.GetNode<AudioStreamPlayer>("WhateverThatIs").Play());
				warning.Rect.Hide();
				_availableSwordfishWarnings.Push(warning.Rect);
				_swordfishWarnings.RemoveAt(i);
			}
		}
	}

	public void HideSwordfishWarning()
	{
		foreach (SwordfishWarning warning in _swordfishWarnings)
		{
			warning.Rect.Hide();
			_availableSwordfishWarnings.Push(warning.Rect);
		}
		_swordfishWarnings.Clear();
	}

	public void BeginLandPhase()
	{	
		HideSwordfishWarning();
		foreach (Node child in Background.GetChildren())
		{
			if (child is Fishfood || child is Swordfih || child is Stika || child is Bubble || child is Puffer)
			{
				((Node2D)child).Hide();
				child.QueueFree();
			}



		}

		camera.Zoom = new Vector2(1.1f, 1.1f);
		camera.ForceUpdateScroll();
		Background.Hide();

	}

	public Puffer SpawnPuffer(Vector2 position)
	{
		Puffer puffer = pufferScene.Instantiate<Puffer>();
		puffer.Position = position;
		puffer.Target = _Player;
		Background.AddChild(puffer);
		return puffer;
	}

public void SpawnBubble(Vector2 position)
  {
	  Bubble bubble = bublinaScene.Instantiate<Bubble>();
	  if (!bubble.TryPlaceAwayFromPlayer(Background, _Player, position))
	  {
		  bubble.Free();
		  return;
	  }
	  Background.AddChild(bubble);
  }

  public Stika SpawnStika(Vector2 position)
  {
	if (_Player.OnLand)
		return null;
	GD.Print("StikaSpawned");
      Stika stika = stikaScene.Instantiate<Stika>();
      stika.Position = position;
	  stika.Target = GetNode<Player>("Player");
      Background.AddChild(stika);
      return stika;
  }



	private PackedScene GetFoodScene()
	{
		if (_Player.Level == 1)
			return _rng.Randf() < 0.5f ? foodScene : shrimpFoodScene;
		return _Player.Level >= 2 ? shrimpFoodScene : foodScene;
	}

	public void SpawnLevelUpFood()
	{
		if (_Player.OnLand)
			return;
		Rect2 visibleBounds = Helpers.GetLocalViewport(Background);
		for (int i = 0; i < 8; i++)
			ExactFood(GetFoodScene(), Helpers.RandomPointInMargin(visibleBounds, Fishfood.SimulationMargin, _rng));
	}

	public void generateFood(PackedScene scene)
	{
		// Spawn within the area the player can actually reach, not the full screen,
		// so food never lands permanently stranded past the swim/walk bounds.
		Rect2 reachableBounds = GetFoodSpawnBounds();
		ExactFood(scene, Helpers.RandomPointInRect(reachableBounds, _rng));
	}

	private Rect2 GetFoodSpawnBounds()
	{
		Rect2 reachable = _Player.GetMovementBounds();
		Vector2 shrink = reachable.Size * 0.08f;
		Rect2 inset = new Rect2(reachable.Position + shrink * 0.5f, reachable.Size - shrink);
		return Helpers.GetLocalRect(Background, inset);
	}


	public void ExactFood(PackedScene scene, Vector2 position)
	{
		if (_Player.OnLand)
			return;
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
