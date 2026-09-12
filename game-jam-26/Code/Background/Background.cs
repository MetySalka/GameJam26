  using Godot;

  public partial class Background : Node2D
  {
      private ScrollingBGsprite _sprite;
      private Vector2 _startingPosition;

      public override void _Ready()
      {
          _sprite = GetNode<ScrollingBGsprite>("ScrollingBGsprite");
          _startingPosition = Position;
      }

      public void ResetForNewRun()
      {
          Position = _startingPosition;
          _sprite.ResetForNewRun();
      }

      public void ScrollVertically(float distance)
      {
          Helpers.MoveWorldObjects(new Vector2(0, distance), this);
      }


      public void SetLevelTransition(int fromLevel, int toLevel, float progress)
      {
          _sprite.SetLevelTransition(fromLevel, toLevel, progress);
      }

      public Vector2 GetLevelSourceRange(int level)
      {
          return _sprite.GetLevelSourceRange(level);
      }
  }
