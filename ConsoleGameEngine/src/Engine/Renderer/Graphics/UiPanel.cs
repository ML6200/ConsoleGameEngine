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

        // Transform world coordinates to screen coordinates using camera
        Point2D? screenPos = camera.TransformPoint(WorldPosition);

        // Culling: if off-screen, don't render
        if (screenPos == null) return;

        renderer.FillRect(screenPos.X, screenPos.Y, Size.Width, Size.Height,
            ' ', BackgroundColor, ForegroundColor);

        if (HasBorder)
        {
            renderer.DrawBox(screenPos.X, screenPos.Y, Size.Width, Size.Height,
                BackgroundColor, BorderColor);
        }

        // Children are rendered automatically by GraphicsComponent.Compute()
    }
}