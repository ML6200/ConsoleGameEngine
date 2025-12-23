using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiButton : GraphicsComponent, IFocusable
{
    private string _text;
    
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateSize();
        }
    }
    
    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;
    
    public bool HasBorder { get; set; } = false;

    public event EventHandler OnClick;

    public AnsiColor FocusedBgColor { get; set; } = AnsiColor.Cyan;
    public AnsiColor FocusedFgColor { get; set; } = AnsiColor.Cyan;
    
    public UiButton(string text)
    {
        Text = text;
    }

    public UiButton()
    {
    }


    public void OnFocusGained()
    {
        HasBorder = true;
    }

    public void OnFocusLost()
    {
        HasBorder = false;
    }

    public void OnFocusActivate()
    {
        AddAnimation(AnimationTween.Blink(this, 0.3, false));
        
        OnClick(this, EventArgs.Empty);
    }
    
    private void UpdateSize()
    {
        if (_text == null) return;

        int minWidth = HasBorder ? _text.Length + 2 : _text.Length;
        int minHeight = HasBorder ? 3 : 1;
        
        int newWidth = Math.Max(minWidth, Size.Width);
        int newHeight = Math.Max(minHeight, Size.Height);
        Width = newWidth;
        Height = newHeight;
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // UI buttons render directly at world position (no camera transformation)
        AnsiColor bgColor = IsFocused ? FocusedBgColor : BackgroundColor;
        AnsiColor fgColor = IsFocused ? FocusedFgColor : ForegroundColor;

        var style = new RenderStyle(bgColor, fgColor, FontStyle);

        if (HasBorder)
        {
            renderer.FillRect(
                WorldPosition.X,
                WorldPosition.Y,
                Size.Width,
                Size.Height,
                ' ',
                style
            );

            // Szegely
            renderer.DrawBox(
                WorldPosition.X,
                WorldPosition.Y,
                Size.Width,
                Size.Height,
                style
            );
        }
        else
        {
            renderer.FillRect(
                WorldPosition.X,
                WorldPosition.Y,
                Size.Width,
                Size.Height,
                ' ',
                style
            );
        }

        // Szoveg
        int padding = (Size.Width-Text.Length) / 2;
        int textX = WorldPosition.X + padding;
        int textY = WorldPosition.Y + Size.Height / 2;

        renderer.DrawText(textX, textY, Text, style);
    }
}