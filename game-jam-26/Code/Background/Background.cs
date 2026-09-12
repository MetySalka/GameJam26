  using Godot;

  public partial class Background : Node2D
  {
      private ScrollingBGsprite _sprite;

      public override void _Ready()
      {
          _sprite = GetNode<ScrollingBGsprite>("ScrollingBGsprite");
      }

      public void ScrollVertically(float distance)
      {
          _sprite.ScrollVertically(distance);

          Helpers.MoveWorldObjects(new Vector2(0, distance), this);
      }


      public void SetScrollArea(Vector2 FromTo)
    {
        _sprite.ScrollAreaFrom = FromTo.X;
        _sprite.ScrollAreaTo = FromTo.Y;
    }


    public Vector2 GetScrollArea()
    {
        return new Vector2(_sprite.ScrollAreaFrom, _sprite.ScrollAreaTo);
    }
  }
