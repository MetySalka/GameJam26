using Godot;
using System;

public partial class ScrollingBGsprite : Sprite2D
{
	[Export] public float ScrollSpeed { get; set; } = 10.0f;
	[Export] public float WindowHeight { get; set; } = 500.0f;



	public float ScrollAreaFrom = 0.0f;
	public float ScrollAreaTo = 1024.0f;
	public const float WobbleSpeed = 0.3f;
	public const float WobbleRange = 0.03f;
	public const float RisingSpeed = 0.05f;

	private float _offsetY;
	private Texture2D _sourceTexture;
	private float _phase = 0.0f;
	private Vector2 _startingPosition;
	private Vector2 _startingScrollArea;

	public override void _Ready()
	{
		_startingPosition = Position;
		_startingScrollArea = new Vector2(ScrollAreaFrom, ScrollAreaTo);
		// Keep the scene's texture for custom drawing, but disable Sprite2D's
		// automatic drawing so the full image isn't drawn behind the strips.
		_sourceTexture = Texture;
		Texture = null;
		TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled;
		_offsetY = ScrollAreaFrom;
		UpdateRegion();
	}

	public override void _Process(double delta)
	{


		// Advance through source rows from ScrollAreaFrom toward ScrollAreaTo.
		_offsetY += ScrollSpeed * (float)delta;
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
		if (TryGetScrollRange(out float from, out float to))
		{
			// Store an absolute source row. Expanding either boundary leaves
			// the current row unchanged, instead of resetting the scroll phase.
			if (_offsetY < from || _offsetY >= to)
				_offsetY = from + Mathf.PosMod(_offsetY - from, to - from);
		}
		QueueRedraw();
	}

	private bool TryGetScrollRange(out float from, out float to)
	{
		float height = _sourceTexture?.GetHeight() ?? 0;
		from = Mathf.Clamp(ScrollAreaFrom, 0, height);
		to = Mathf.Clamp(ScrollAreaTo, 0, height);
		return _sourceTexture != null && to - from >= 1.0f;
	}

	public override void _Draw()
	{
		if (!TryGetScrollRange(out float from, out float to) || WindowHeight <= 0)
			return;

		float tileWidth = _sourceTexture.GetWidth();
		Vector2 size = new Vector2(tileWidth * 12, WindowHeight);
		Vector2 origin = Offset - (Centered ? size / 2 : Vector2.Zero);
		float sourceY = from + Mathf.PosMod(_offsetY - from, to - from);
		float drawnHeight = 0;

		while (drawnHeight < WindowHeight)
		{
			float sectionHeight = Mathf.Min(to - sourceY, WindowHeight - drawnHeight);
			if (sectionHeight <= 0 || drawnHeight + sectionHeight == drawnHeight)
				break;

			Rect2 source = new Rect2(0, sourceY, tileWidth, sectionHeight);
			for (int column = 0; column < 12; column++)
			{
				Vector2 position = origin + new Vector2(column * tileWidth, drawnHeight);
				DrawTextureRectRegion(_sourceTexture,
					new Rect2(position, new Vector2(tileWidth, sectionHeight)), source);
			}

			drawnHeight += sectionHeight;
			sourceY = from;
		}
	}

	public void ResetForNewRun()
	{
		Position = _startingPosition;
		ScrollAreaFrom = _startingScrollArea.X;
		ScrollAreaTo = _startingScrollArea.Y;
		_offsetY = ScrollAreaFrom;
		_phase = 0;
		UpdateRegion();
	}

	public void ScrollVertically(float distance)
	{
		if (Mathf.IsZeroApprox(Scale.Y))
			return;

		_offsetY -= distance / Scale.Y;
		UpdateRegion();
	}


}
