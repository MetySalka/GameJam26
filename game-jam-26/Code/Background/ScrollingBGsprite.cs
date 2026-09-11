using Godot;
using System;

public partial class ScrollingBGsprite : Sprite2D
{
    [Export] public float ScrollSpeed { get; set; } = 10.0f;
    [Export] public float WindowHeight { get; set; } = 500.0f;

    public const float WobbleSpeed = 0.3f;
    public const float WobbleRange = 0.03f;
    public const float RisingSpeed = 0.05f;

    private float _offsetY;
    private float _textureHeight;
    private float _phase = 0.0f;

    public override void _Ready()
    {
        RegionEnabled = true;
        TextureRepeat = CanvasItem.TextureRepeatEnum.Enabled;
        _textureHeight = Texture.GetSize().Y;
        UpdateRegion();
    }

    public override void _Process(double delta)
    {
        // Scrolling
        _offsetY += ScrollSpeed * -(float)delta;
        _offsetY %= _textureHeight;
        UpdateRegion();

        // Wobble
        _phase += WobbleSpeed;
        _phase %= 360;

        Position += new Vector2(
            (float)(
                Math.Sin((((_phase + 80) % 360) / 360) * 0.9f * Math.PI * 2) * WobbleRange / 2 +
                Math.Sin((_phase / 360) * 1.1f * Math.PI * 2) * WobbleRange / 2
            ),
            0
        );
    }

    private void UpdateRegion()
    {
        RegionRect = new Rect2(0, _offsetY, Texture.GetSize().X, WindowHeight);
    }
}