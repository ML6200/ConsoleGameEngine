using System;
using System.Dynamic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiLabel : GraphicsComponent
{
    private string _text = "";

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            SetSize();
        }
    }
    public UiLabel()
    {
        SetSize();
    }

    public UiLabel(string text)
    {
        Text = text;
        SetSize();
    }
    
    private void SetSize()
    {
        string[] lines = Text.Split('\n');
        Height = lines.Length;
        Width = 0;
        foreach (var line in lines)
        {
            // handle \r\n (Windows) endings
            int lineWidth = line.TrimEnd('\r').Length;
            if (lineWidth > Width)
                Width = lineWidth;
        }
    }

    protected override void Draw(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (_text.Contains("\n"))
        {
            string[] lines = _text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                renderer.DrawText(WorldPosition.X, WorldPosition.Y + i, lines[i].TrimEnd('\r'),
                    BackgroundColor, ForegroundColor);
            }
        }
        else
        {
            renderer.DrawText(WorldPosition.X, WorldPosition.Y, Text,
                BackgroundColor, ForegroundColor);
        }
    }
}