using System;
using System.Dynamic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiLabel : GraphicsComponent
{
    public string Text { get; set; } = "";

    public UiLabel()
    {
    }

    public UiLabel(string text)
    {
        Text = text;
        Width = text.Length;
        Height = 1;
    }

    public override void Compute(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        renderer.DrawText(WorldPosition.X, WorldPosition.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}