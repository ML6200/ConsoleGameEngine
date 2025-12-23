using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiPanel : GraphicsComponent
{
    public UiPanel(int width, int height, Point2D? relativePosition) : base(width, height, relativePosition)
    {
    }

    public UiPanel()
    {
    }

    public bool HasBorder { get; set; } = true;

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // UI panels render directly at world position (no camera transformation)
        // This makes them fixed on screen, perfect for menus, HUDs, etc.

        var fillStyle = new RenderStyle(BackgroundColor, ForegroundColor, FontStyle);
        renderer.FillRect(WorldPosition.X, WorldPosition.Y, Width, Height, ' ', fillStyle);

        if (HasBorder)
        {
            var borderStyle = new RenderStyle(BackgroundColor, BorderColor, FontStyle);
            renderer.DrawBox(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height, borderStyle);
        }
    }
}