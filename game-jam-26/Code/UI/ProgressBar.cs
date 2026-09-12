using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : CanvasLayer
{
	[Export] private TextureProgressBar _textureBar;
	[Export] public global::Background Background { get; set; }
	int animationState;
	private Vector2 _originalScale;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_textureBar.Value = 0;
		_originalScale = _textureBar.Scale;
		_textureBar.Resized += UpdatePivot;
		UpdatePivot();
	}

	private void UpdatePivot()
	{
		_textureBar.PivotOffset = _textureBar.Size / 2.0f;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if (animationState >= 1 && animationState < 100)
		{
			_textureBar.Value = Mathf.MoveToward(_textureBar.Value, 0, 1);
			_textureBar.Scale += new Vector2(0.0025f, 0.001f);
			Background.SetScrollArea(Background.GetScrollArea() + new Vector2(16, 16));
			Helpers.MoveWorldObjects(new Vector2(0, 5), Background);
			Helpers.MoveFood(new Vector2(0, -5), Background);
			Helpers.MoveFoodStarWars(new Vector2(0, 6), Background);


			animationState++;
		}
		else if (animationState >= 100)
		{
			animationState = -100;

			foreach (Node child in Background.GetChildren())
			{
				if ((child is Stika) && !child.IsQueuedForDeletion())
				{
					((Stika)child).KYS();
				}
			}
		}
		if (animationState < -1)
		{
			_textureBar.Scale -= new Vector2(0.0025f, 0.001f);
			animationState++;
			if (animationState == -1)
			{
				_textureBar.Scale = _originalScale;
				animationState = 0;
			}
		}
		if (animationState == 0 && _textureBar.Value >= _textureBar.MaxValue)
		{
			animationState = 1;
		}
	}
}
