using System;
using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : Node2D
{
	[Export] private TextureProgressBar _textureBar;
	[Export] public global::Background Background { get; set; }
int testVar;
	int animationState;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{ 
		_textureBar.Value = 0;
		 
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if(animationState >= 1 && animationState < 100)
		{
			testVar++;
			_textureBar.Value = Mathf.MoveToward(_textureBar.Value, 0, 1);;
			_textureBar.Scale += new Vector2(0.0025f, 0.001f);
			Background.SetScrollArea(Background.GetScrollArea() + new Vector2(16,16));


			animationState++;
		} else if (animationState >= 100)
		{
			animationState = -100;
			GD.Print("TestVar in middle of animation: " + testVar);
		}
		if(animationState < -1)
		{
			_textureBar.Value = Mathf.MoveToward(0, _textureBar.Value, 1);;
			_textureBar.Scale -= new Vector2(0.0025f, 0.001f);
			animationState++;
			testVar--;
			
		} 
		GD.Print("TestVar at the end: " + testVar);
		if (_textureBar.Value > 99)
		{
			animationState = 1;
		}
	}
}