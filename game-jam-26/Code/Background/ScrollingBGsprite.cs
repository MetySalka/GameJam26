using Godot;
using System;

public partial class ScrollingBGsprite : Sprite2D
{
	[Export] public float ScrollSpeed { get; set; } = 10.0f;
	[Export] public float WindowHeight { get; set; } = 500.0f;

	public const float WobbleSpeed = 0.3f;
	public const float WobbleRange = 0.03f;
	public const float RisingSpeed = 0.05f;

	private float _offsetY;
	private float _textureHeight;
	private float _phase = 0.0f;

	public override void _Ready()
	{
		RegionEnabled = true;
		TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
		_textureHeight = Texture.GetSize().Y;
		UpdateRegion();
	}

	public override void _Process(double delta)
	{
		// Wrap the texture offset to scroll continuously.
		_offsetY += ScrollSpeed * -(float)delta;
		_offsetY %= _textureHeight;
		UpdateRegion();

		// Combine two gentle waves for horizontal movement.
		_phase += WobbleSpeed;
		_phase %= 360;

		double firstWave = Math.Sin((((_phase + 80) % 360) / 360) * 0.9f * Math.PI * 2);
		double secondWave = Math.Sin((_phase / 360) * 1.1f * Math.PI * 2);
		float horizontalOffset = (float)(firstWave * WobbleRange / 2 + secondWave * WobbleRange / 2);
		Position += new Vector2(horizontalOffset, 0);
	}

	private void UpdateRegion()
	{
		RegionRect = new Rect2(0, _offsetY, Texture.GetSize().X, WindowHeight);
	}
}
