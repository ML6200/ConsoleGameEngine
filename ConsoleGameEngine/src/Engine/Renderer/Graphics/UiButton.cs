using System;
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
    
    public bool HasBorder { get; set; } = true;

    public event EventHandler OnClick;

    public ConsoleColor NormalBgColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    public UiButton(string text)
    {
        Text = text;
    }

    public UiButton()
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
        
        OnClick(this, EventArgs.Empty);
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

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (WorldPosition == null) return;

        // Transform world coordinates to screen coordinates
        Point2D? screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == null) return; // Off-screen culling

        var bgColor = IsFocused ? FocusedBgColor : NormalBgColor;

        if (HasBorder)
        {
            renderer.FillRect(
                screenPos.X,
                screenPos.Y,
                Size.Width,
                Size.Height,
                ' ',
                bgColor,
                ForegroundColor
            );

            // Szegely
            renderer.DrawBox(
                screenPos.X,
                screenPos.Y,
                Size.Width,
                Size.Height,
                bgColor,
                BorderColor
            );
        }
        else
        {
            renderer.FillRect(
                screenPos.X,
                screenPos.Y,
                Size.Width,
                Size.Height,
                ' ',
                bgColor,
                ForegroundColor
            );
        }

        // Szoveg
        int padding = HasBorder ? (Size.Width-Text.Length) / 2 : 1;
        int textX = screenPos.X + padding;
        int textY = screenPos.Y + Size.Height / 2;

        renderer.DrawText(textX, textY, Text, bgColor, ForegroundColor);
    }
}