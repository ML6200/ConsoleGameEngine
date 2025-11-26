using System;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleGraphicsButton : ConsoleGraphicsComponent, IFocusable
{
    private int _width;
    private int _height;
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
    
    public bool HasBorder { get; set; } = true;

    public event EventHandler OnClick;

    public ConsoleColor NormalBgColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleGraphicsButton(string text)
    {
        Text = text;
    }

    public ConsoleGraphicsButton()
    {
    }


    public void OnFocusGained()
    {
    }

    public void OnFocusLost()
    {
    }

    public void OnFocusActivate()
    {
        AddAnimation(AnimationTween.Blink(this, 0.3, false));
        
        OnClick?.Invoke(this, EventArgs.Empty);
    }
    
    private void UpdateSize()
    {
        if (_text == null) return;

        int minWidth = HasBorder ? _text.Length + 2 : _text.Length;
        int minHeight = HasBorder ? 3 : 1;
        
        int newWidth = Math.Max(minWidth, Size.Width);
        int newHeight = Math.Max(minHeight, Size.Height);
        Size = new Dimension2D(newWidth, newHeight);
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        var bgColor = IsFocused ? FocusedBgColor : NormalBgColor;
        
        if (HasBorder)
        {
            renderer.FillRect(
                AbsolutePosition.X,
                AbsolutePosition.Y,
                Size.Width,
                Size.Height,
                ' ',
                bgColor,
                ForegroundColor
            );

            // Szegely
            renderer.DrawBox(
                AbsolutePosition.X,
                AbsolutePosition.Y,
                Size.Width,
                Size.Height,
                bgColor,
                BorderColor
            );
        }
        else
        {
            renderer.FillRect(
                AbsolutePosition.X,
                AbsolutePosition.Y,
                Size.Width,
                Size.Height,
                ' ',
                bgColor,
                ForegroundColor
            );
        }

        // Szoveg
        int padding = HasBorder ? (Size.Width-Text.Length) / 2 : 1;
        int textX = AbsolutePosition.X + padding;
        int textY = AbsolutePosition.Y + Size.Height / 2;

        renderer.DrawText(textX, textY, Text, bgColor, ForegroundColor);

        base.Render(renderer);
    }
}