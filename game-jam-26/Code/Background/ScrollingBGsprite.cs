using Godot;

public partial class ScrollingBGsprite : Sprite2D
{
	// Two viewport bands plus two equal gaps occupy the lower 4,096 source rows.
	public const float BottomRow = 8192f;
	public const float MiddleRow = 4096f;
	public const float LevelStep = (BottomRow - MiddleRow) / 2f;

	public float SourceTop { get; private set; }
	public float WindowHeight => Mathf.Min(GetViewport().GetVisibleRect().Size.Y, _sourceTexture.GetHeight());
	public float GapHeight => LevelStep - WindowHeight;
	// Cancel inherited node/camera scaling: one source pixel occupies one viewport pixel.
	public float TileWidthLocal => _sourceTexture.GetWidth() / GetGlobalTransformWithCanvas().X.Length();

	private Texture2D _sourceTexture;
	private Vector2 _startingPosition;
	private float _phase;
	private int _fromLevel;
	private int _toLevel;
	private float _transitionProgress;

	public override void _Ready()
	{
		_startingPosition = Position;
		_sourceTexture = Texture;
		// Draw only the selected source band, without the full Sprite2D texture behind it.
		Texture = null;
		TextureRepeat = CanvasItem.TextureRepeatEnum.Disabled;
		ResetForNewRun();
	}

	// X is the top source row; Y is the bottom. A band contains one viewport's source rows.
	public Vector2 GetLevelSourceRange(int level)
	{
		float bottom = Mathf.Min(BottomRow, _sourceTexture.GetHeight());
		float top = Mathf.Clamp(bottom - Mathf.Max(0, level) * LevelStep - WindowHeight,
			0f, _sourceTexture.GetHeight() - WindowHeight);
		return new Vector2(top, top + WindowHeight);
	}

	public void SetLevelTransition(int fromLevel, int toLevel, float progress)
	{
		_fromLevel = fromLevel;
		_toLevel = toLevel;
		_transitionProgress = Mathf.Clamp(progress, 0f, 1f);
		UpdateSourceBand();
	}

	private void UpdateSourceBand()
	{
		SourceTop = Mathf.Lerp(GetLevelSourceRange(_fromLevel).X,
			GetLevelSourceRange(_toLevel).X, _transitionProgress);
		QueueRedraw();
	}

	public override void _Process(double delta)
	{
		// Keep horizontal movement, but never drift vertically out of the level's band.
		_phase += (float)delta * 0.3f;
		Position = _startingPosition + new Vector2(Mathf.Sin(_phase) * 3f, 0f);
		UpdateSourceBand();
	}

	public override void _Draw()
	{
		if (_sourceTexture == null || WindowHeight <= 0f)
			return;

		// Map one viewport-height source band onto the whole visible screen, including
		// the scene's sprite scale and camera transform. Tile only horizontally.
		Rect2 visible = Helpers.GetLocalViewport(this);
		float tileWidth = TileWidthLocal;
		float originX = Offset.X - (Centered ? tileWidth * 0.5f : 0f);
		float startX = originX + Mathf.Floor((visible.Position.X - originX) / tileWidth) * tileWidth;
		Rect2 source = new Rect2(0f, SourceTop, _sourceTexture.GetWidth(), WindowHeight);
		for (float x = startX; x < visible.End.X; x += tileWidth)
			DrawTextureRectRegion(_sourceTexture,
				new Rect2(x, visible.Position.Y, tileWidth, visible.Size.Y), source);
	}

	public void ResetForNewRun()
	{
		Position = _startingPosition;
		_phase = 0f;
		SetLevelTransition(0, 0, 0f);
	}
}
