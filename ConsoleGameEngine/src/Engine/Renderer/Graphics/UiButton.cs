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
    
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor FocusedFgColor { get; set; } = ConsoleColor.Cyan;
    
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

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (WorldPosition == null) return;

        // UI buttons render directly at world position (no camera transformation)
        ConsoleColor bgColor = IsFocused ? FocusedBgColor : BackgroundColor;
        ConsoleColor fgColor = IsFocused ? FocusedFgColor : ForegroundColor;

        if (HasBorder)
        {
            renderer.FillRect(
                WorldPosition.X,
                WorldPosition.Y,
                Size.Width,
                Size.Height,
                ' ',
                bgColor,
                fgColor
            );

            // Szegely
            renderer.DrawBox(
                WorldPosition.X,
                WorldPosition.Y,
                Size.Width,
                Size.Height,
                bgColor,
                fgColor
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
                bgColor,
                fgColor
            );
        }

        // Szoveg
        int padding = (Size.Width-Text.Length) / 2;
        int textX = WorldPosition.X + padding;
        int textY = WorldPosition.Y + Size.Height / 2;

        renderer.DrawText(textX, textY, Text, bgColor, fgColor);
    }
}