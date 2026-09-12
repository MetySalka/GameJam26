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

          foreach (Node child in GetChildren())
          {
              if (child is Fishfood food && !food.IsQueuedForDeletion())
                  food.Position += new Vector2(0, distance);
          }
      }
  }
