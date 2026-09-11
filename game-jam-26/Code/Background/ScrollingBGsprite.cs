using Godot;

public partial class ScrollingBGsprite : Sprite2D
{
    [Export] public float ScrollSpeed { get; set; } = 10.0f;
    [Export] public float WindowHeight { get; set; } = 500.0f;

    private float _offsetY;
    private float _textureHeight;

    public override void _Ready()
    {
        RegionEnabled = true;
        TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
        _textureHeight = Texture.GetSize().Y;
        UpdateRegion();
    }

    public override void _Process(double delta)
    {


        _offsetY += ScrollSpeed * -(float)delta;
        _offsetY %= _textureHeight;
        UpdateRegion();
    }

    private void UpdateRegion()
    {
        RegionRect = new Rect2(0, _offsetY, Texture.GetSize().X, WindowHeight);
    }
}