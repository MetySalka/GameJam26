using System;
using Godot;

namespace GameJam26.Code.UI;

public partial class ProgressBar : CanvasLayer
{
	[Export] private TextureProgressBar _textureBar;
	[Export] public global::Background Background { get; set; }
	[Export] private Health _health;

	Player player;
	int animationState;
	private Vector2 _originalScale;
	private int _displayedLevel = -1;
	private double _drainPerFrame;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_textureBar.Value = 0;
		_originalScale = _textureBar.Scale;
		_textureBar.Resized += UpdatePivot;
		UpdatePivot();
		player = GetNode<Player>("/root/Main/Player");
		_textureBar.Step = 0;
		UpdateLevelRequirement();
	}

	public void ResetForNewRun()
	{
		animationState = 0;
		_drainPerFrame = 0;
		_textureBar.Value = 100;
		_textureBar.Scale = _originalScale;
		_displayedLevel = -1;
		UpdateLevelRequirement();
	}

	private void UpdateLevelRequirement()
	{
		if (_displayedLevel == player.Level)
			return;
		_displayedLevel = player.Level;
		_textureBar.MaxValue = Math.Pow(10.0, player.Level + 1);
	}

	private void UpdatePivot()
	{
		_textureBar.PivotOffset = _textureBar.Size / 2.0f;
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		if(! (animationState == 0))
		{
			player.Invincible = true;
		} else
		{
			player.Invincible = false;
		}

		UpdateLevelRequirement();

		if (animationState >= 1 && animationState < 100)
		{
			_textureBar.Value = Mathf.MoveToward(_textureBar.Value, 0, _drainPerFrame);
			_textureBar.Scale += new Vector2(0.0025f, 0.001f);
			Background.SetLevelTransition(player.Level, player.Level + 1, animationState / 99f);
			Helpers.MoveWorldObjects(new Vector2(0, 5), Background);
			Helpers.MoveFood(new Vector2(0, -5), Background);
			Helpers.MoveFoodStarWars(new Vector2(0, 6), Background);


			animationState++;
		}
		else if (animationState >= 100)
		{
			animationState = -100;
			_textureBar.Value = 0;
			player.LevelUp();
			UpdateLevelRequirement();
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
			// Empty the bar over the same 99 animation frames at every level.
			_drainPerFrame = _textureBar.Value / 99.0;
			animationState = 1;
			player.PlayWhoosh();
		}
	}
}
