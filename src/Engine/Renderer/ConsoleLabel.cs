using System;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleLabel : ConsoleComponent
{
    public string Text { get; set; } = "";

    public ConsoleLabel()
    {
    }

    public ConsoleLabel(string text)
    {
        Text = text;
    }

    public override void Update()
    { }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        renderer.DrawText(Position.X, Position.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}