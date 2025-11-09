using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;


namespace ConsoleGameEngine.Engine.Renderer;

public class ConsolePanel : ConsoleComponent
{
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor BorderColor { get; set; } = ConsoleColor.White;
    public bool HasBorder { get; set; } = true;

    public override void Update()
    {
        if (!Visible) return;
        
        foreach (var child in Children)
            child.Update();
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        renderer.FillRect(Position.X, Position.Y, Size.Width, Size.Height,
            ' ', BackgroundColor, ConsoleColor.White);
        
        if (HasBorder)
        {
            renderer.DrawBox(Position.X, Position.Y, Size.Width, Size.Height,
                BackgroundColor, BorderColor);
        }
        
        base.Render(renderer);
    }
}