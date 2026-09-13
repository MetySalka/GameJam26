using Godot;

namespace GameJam26.Code.UI;

// Purely decorative: fills the menu with a blue sea backdrop and sends
// random fish swimming across it every few seconds, reusing each species'
// actual in-game swim animation.
public partial class StartMenuBackground : Control
{
	// Mirrors how each creature is set up in-game: separate art per facing
	// (Swordfih, Stika) or a single flip-able animation (Puffer, Shrimp, Tadpole).
	private sealed class FishSpecies
	{
		public string[] RightFrames;
		public string[] LeftFrames;
		public bool FlipInsteadOfLeftFrames;
		public bool Spin;
		public Vector2 BaseSize;
	}

	private static readonly FishSpecies[] Species =
	{
		new FishSpecies
		{
			RightFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Swordfih/swordfish_scaled_0001.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfish_scaled_0002.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfish_scaled_0003.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfish_scaled_0004.png",
			},
			LeftFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Swordfih/swordfih_0001.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfih_0002.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfih_0003.png",
				"res://Assets/Sprites/Creatures/Swordfih/swordfih_0004.png",
			},
			BaseSize = new Vector2(110f, 40f),
		},
		new FishSpecies
		{
			// Fully inflated frame only, spinning like it does after hitting the player.
			RightFrames = new[] { "res://Assets/Sprites/Creatures/puffer/puffer.png_0006.png" },
			FlipInsteadOfLeftFrames = true,
			Spin = true,
			BaseSize = new Vector2(60f, 60f),
		},
		new FishSpecies
		{
			RightFrames = new[] { "res://Assets/Sprites/Creatures/Shrimp/shrimp right.png" },
			LeftFrames = new[] { "res://Assets/Sprites/Creatures/Shrimp/shrimp left.png" },
			Spin = true,
			BaseSize = new Vector2(40f, 30f),
		},
		new FishSpecies
		{
			RightFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Tadpole/tadpole right_0001.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole right_0002.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole right_0003.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole right_0004.png",
			},
			LeftFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Tadpole/tadpole left_0001.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole left_0002.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole left_0003.png",
				"res://Assets/Sprites/Creatures/Tadpole/tadpole left_0004.png",
			},
			BaseSize = new Vector2(50f, 40f),
		},
		new FishSpecies
		{
			RightFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Stika/right/stika_sprite_0001.png",
				"res://Assets/Sprites/Creatures/Stika/right/stika_sprite_0002.png",
				"res://Assets/Sprites/Creatures/Stika/right/stika_sprite_0003.png",
				"res://Assets/Sprites/Creatures/Stika/right/stika_sprite_0004.png",
			},
			LeftFrames = new[]
			{
				"res://Assets/Sprites/Creatures/Stika/left/stika_sprite_left_0001.png",
				"res://Assets/Sprites/Creatures/Stika/left/stika_sprite_left_0002.png",
				"res://Assets/Sprites/Creatures/Stika/left/stika_sprite_left_0003.png",
				"res://Assets/Sprites/Creatures/Stika/left/stika_sprite_left_0004.png",
			},
			BaseSize = new Vector2(90f, 60f),
		},
	};

	private readonly RandomNumberGenerator _rng = new();
	private double _timeUntilNextSpawn;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.WhenPaused;
		_rng.Randomize();
		ScheduleNextSpawn();
		VisibilityChanged += OnVisibilityChanged;
	}

	private void OnVisibilityChanged()
	{
		if (Visible)
			return;
		foreach (Node child in GetChildren())
		{
			if (child is AnimatedSprite2D fish)
				fish.QueueFree();
		}
	}

	public override void _Process(double delta)
	{
		if (!Visible)
			return;

		_timeUntilNextSpawn -= delta;
		if (_timeUntilNextSpawn <= 0)
		{
			SpawnFish();
			ScheduleNextSpawn();
		}
	}

	private void ScheduleNextSpawn()
	{
		_timeUntilNextSpawn = _rng.RandfRange(1.5f, 4.0f);
	}

	private void SpawnFish()
	{
		FishSpecies species = Species[_rng.RandiRange(0, Species.Length - 1)];
		bool goingRight = _rng.Randf() > 0.5f;
		float scale = _rng.RandfRange(0.6f, 1.2f);
		Vector2 fishSize = species.BaseSize * scale;
		float y = _rng.RandfRange(0.1f, 0.85f) * Size.Y;
		float startX = goingRight ? -fishSize.X : Size.X + fishSize.X;
		float endX = goingRight ? Size.X + fishSize.X : -fishSize.X;

		AnimatedSprite2D fish = new AnimatedSprite2D
		{
			SpriteFrames = BuildSpriteFrames(species),
			Animation = goingRight || species.FlipInsteadOfLeftFrames ? "Right" : "Left",
			FlipH = species.FlipInsteadOfLeftFrames && !goingRight,
			Modulate = new Color(1f, 1f, 1f, _rng.RandfRange(0.7f, 1f)),
			Position = new Vector2(startX, y),
			Scale = Vector2.One * scale,
		};
		AddChild(fish);
		fish.Play();

		if (species.Spin)
			fish.RotationDegrees = _rng.RandfRange(0f, 360f);

		float speed = _rng.RandfRange(120f, 220f);
		float duration = Mathf.Abs(endX - startX) / speed;

		Tween tween = fish.CreateTween();
		tween.SetParallel();
		tween.TweenProperty(fish, "position:x", endX, duration);
		if (species.Spin)
		{
			float spinDirection = goingRight ? 1f : -1f;
			tween.TweenProperty(fish, "rotation_degrees", fish.RotationDegrees + spinDirection * 720f, duration);
		}
		tween.Chain().TweenCallback(Callable.From(fish.QueueFree));
	}

	private static SpriteFrames BuildSpriteFrames(FishSpecies species)
	{
		SpriteFrames frames = new SpriteFrames();
		frames.RemoveAnimation("default");

		frames.AddAnimation("Right");
#pragma warning disable CS0618
		frames.SetAnimationLoop("Right", true);
#pragma warning restore CS0618
		frames.SetAnimationSpeed("Right", 5.0);
		foreach (string path in species.RightFrames)
			frames.AddFrame("Right", GD.Load<Texture2D>(path));

		if (!species.FlipInsteadOfLeftFrames)
		{
			frames.AddAnimation("Left");
#pragma warning disable CS0618
			frames.SetAnimationLoop("Left", true);
#pragma warning restore CS0618
			frames.SetAnimationSpeed("Left", 5.0);
			foreach (string path in species.LeftFrames)
				frames.AddFrame("Left", GD.Load<Texture2D>(path));
		}

		return frames;
	}
}
