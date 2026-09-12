using System;
using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : Node2D
{
	[Export] private TextureProgressBar _textureBar;
	[Export] public global::Background Background { get; set; }
	bool evolving = false;
	int evolveCnt = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{ 
		_textureBar.Value = 0;
		 
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if(evolving)
		{
			if(_textureBar.Scale.X > 0.6f) {
			_textureBar.Scale -= new Vector2(0.0015f, 0.001f);
			}

			if(_textureBar.Scale.X == 0.6f)
			{
				evolving = false;
			}
		}
		if (_textureBar.Value > 99)
		{
			_textureBar.Value = 0;
			_textureBar.Scale = new Vector2(0.75f, 0.7f);
			Background.SetScrollArea(new Vector2(Background.GetScrollArea().X + 4096, Background.GetScrollArea().Y + 4096));
			evolving = true;

		}
	}
}