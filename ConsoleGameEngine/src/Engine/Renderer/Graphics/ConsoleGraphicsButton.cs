using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleGraphicsButton : ConsoleGraphicsComponent, IFocusable
{
    private int _width;
    private int _height;
    public string Text
    {
        get; 
        set;
    }
    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;

    public event EventHandler OnClick;

    public ConsoleColor NormalBgColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    

    public int MinWidth
    {
        get => Text?.Length ?? 0;
    }
    
    public int MinHeight
    {
        get => 3;
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
        OnClick?.Invoke(this, EventArgs.Empty);
    }
    

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        var bgColor = IsFocused ? FocusedBgColor : NormalBgColor;

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

        // Szoveg
        int textX = AbsolutePosition.X + (Size.Width - Text.Length) / 2;
        int textY = AbsolutePosition.Y + Size.Height / 2;

        renderer.DrawText(textX, textY, Text, bgColor, ForegroundColor);

        base.Render(renderer);
    }

    public override void Update()
    {
    }
}