using System;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleGraphicsPanel : ConsoleGraphicsComponent
{
    public bool HasBorder { get; set; } = true;

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
            ' ', BackgroundColor, ConsoleColor.White);
        
        if (HasBorder)
        {
            renderer.DrawBox(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height,
                BackgroundColor, BorderColor);
        }

        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
            child.Render(renderer);
    }
}