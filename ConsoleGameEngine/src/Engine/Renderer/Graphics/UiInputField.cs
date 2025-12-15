using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiInputField : GraphicsComponent, IFocusable
{
    private string _text = "";
    private volatile bool _isDeleting = false;
    
    private int CursorPosition => _text.Length;

    public string Text
    {
        get => _text;
        set
        {
            OnTextChanged?.Invoke(this, EventArgs.Empty);
            _text = value;
            UpdateSize(); //for growth later
        }
    }

    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;
    
    public bool HasBorder { get; set; } = false;

    private event EventHandler OnTextChanged;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor FocusedFgColor { get; set; } = ConsoleColor.Black;
    
    private InputManager _inputManager;
    
    public UiInputField(string text)
    {
        Text = text;
    }

    public UiInputField(InputManager inputManager)
    {
        _inputManager = inputManager;
    }


    public void OnFocusGained()
    {
        HasBorder = true;
    }

    public void OnFocusLost()
    {
        HasBorder = false;
    }

    public void OnFocusActivate(KeyEventArgs? param)
    {
        if (param != null
            && param.Key != ConsoleKey.Backspace
            && param.Key != ConsoleKey.Enter
            && param.Key != ConsoleKey.Escape
            && param.Key != ConsoleKey.DownArrow
            && param.Key != ConsoleKey.UpArrow)
        {
            _isDeleting = false;
            Text += param.KeyChar;
        }
        else if (param.Key == ConsoleKey.Backspace)
        {
            if (Text.Length > 0)
            {
                _isDeleting = true;
                Text = _text.Substring(0, Text.Length - 1);
            }
        }
    }
    
    private void UpdateSize()
    {
        int minWidth = HasBorder ? _text.Length + 2 : _text.Length;
        int minHeight = 1;
        
        int newWidth = Math.Max(minWidth, Size.Width);
        int newHeight = Math.Max(minHeight, Size.Height);
        
        Width   = newWidth;
        Height = newHeight;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // UI buttons render directly at world position (no camera transformation)
        var bgColor = IsFocused ? FocusedBgColor : BackgroundColor;
        var fgColor = IsFocused ? FocusedFgColor : ForegroundColor;

        // Szoveg
        int padding = (Size.Width-Text.Length) / 2;
        int textX = WorldPosition.X + padding;
        int textY = WorldPosition.Y + Size.Height / 2;
        
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
            renderer.SetCell(WorldPosition.X, WorldPosition.Y, new Cell('['));
            renderer.SetCell(WorldPosition.X + Size.Width - 1, WorldPosition.Y, new Cell(']'));
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
        
        renderer.DrawText(textX, textY, Text, bgColor, fgColor);
        if (IsFocused)
            renderer.SetCell(textX + CursorPosition, textY, new ('█', bgColor, fgColor));
    }
}