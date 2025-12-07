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

    public override void Compute(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
            ' ', BackgroundColor, ForegroundColor);
        
        if (HasBorder)
        {
            renderer.DrawBox(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
                BackgroundColor, BorderColor);
        }

        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
            child.Compute(renderer);
    }
}