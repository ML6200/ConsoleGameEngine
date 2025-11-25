using System;
using System.Dynamic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleGraphicsLabel : ConsoleGraphicsComponent
{
    public string Text { get; set; } = "";

    public ConsoleGraphicsLabel()
    {
    }

    public ConsoleGraphicsLabel(string text)
    {
        Text = text;
        Width = text.Length;
        Height = 1;
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        renderer.DrawText(AbsolutePosition.X, AbsolutePosition.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}