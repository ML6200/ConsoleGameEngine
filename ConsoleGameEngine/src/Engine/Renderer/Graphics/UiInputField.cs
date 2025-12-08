using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiInputField : GraphicsComponent, IFocusable
{
    private string _text = "";
    
    private int CursorPosition
    {
        get
        {
            return _text.Length;
        }
    } 
    
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            UpdateSize(); //for growth later
        }
    }
    
    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;
    
    public bool HasBorder { get; set; } = false;

    private event EventHandler KeyPressed;

    public ConsoleColor NormalBgColor { get; set; } = ConsoleColor.DarkGray;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
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
            _text += param.KeyChar;
        }
        else if (param.Key == ConsoleKey.Backspace)
        {
            if (_text.Length > 0)
                _text = _text.Substring(0, _text.Length - 1);
        }
    }
    
    private void UpdateSize()
    {
        int minWidth = HasBorder ? _text.Length + 2 : _text.Length;
        int minHeight = HasBorder ? 2 : 1;
        
        int newWidth = Math.Max(minWidth, Size.Width);
        int newHeight = Math.Max(minHeight, Size.Height);
        
        Size = new Dimension2D(newWidth, newHeight);
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (WorldPosition == null) return;

        // UI buttons render directly at world position (no camera transformation)
        var bgColor = IsFocused ? FocusedBgColor : NormalBgColor;

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
                ForegroundColor
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
                ForegroundColor
            );
        }
        
        int textXIndex = Size.Width - Text.Length - 1;
        //Text.Substring(textXIndex, Text.Length - 1)
        renderer.DrawText(textX, textY, Text, bgColor, ForegroundColor);
        renderer.SetCell(textX + CursorPosition, textY, 
            new ('_', bgColor, ForegroundColor));
    }
}