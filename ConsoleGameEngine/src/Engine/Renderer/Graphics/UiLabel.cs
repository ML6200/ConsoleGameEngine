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

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (WorldPosition == null) return;

        // UI labels render directly at world position (no camera transformation)
        renderer.DrawText(WorldPosition.X, WorldPosition.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}