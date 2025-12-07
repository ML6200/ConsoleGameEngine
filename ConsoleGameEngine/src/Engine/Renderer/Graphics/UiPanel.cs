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

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (WorldPosition == null) return;

        // UI panels render directly at world position (no camera transformation)
        // This makes them fixed on screen, perfect for menus, HUDs, etc.
        renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
            ' ', BackgroundColor, ForegroundColor);

        if (HasBorder)
        {
            renderer.DrawBox(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
                BackgroundColor, BorderColor);
        }

        // Children are rendered automatically by GraphicsComponent.Compute()
    }
}