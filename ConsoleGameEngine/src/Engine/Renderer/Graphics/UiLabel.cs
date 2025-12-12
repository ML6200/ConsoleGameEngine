using System;
using System.Dynamic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiLabel : GraphicsComponent
{
    private string _text = "";

    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            _text = value;
            UpdateSize();
        }
    }

    private void UpdateSize()
    {
        SetSize();
    }

    public UiLabel()
    {
        SetSize();
    }

    public UiLabel(string text)
    {
        Text = text;
        UpdateSize();
    }
    
    private void SetSize()
    {
        Height = 1;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // UI labels render directly at world position (no camera transformation)
        renderer.DrawText(WorldPosition.X, WorldPosition.Y, Text,
            BackgroundColor, ForegroundColor);
    }
}