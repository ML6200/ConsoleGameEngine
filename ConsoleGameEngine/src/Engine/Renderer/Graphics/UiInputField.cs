using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiInputField : GraphicsComponent, IFocusable, IUiInput
{
    private string _text = "";
    
    private int CursorPosition {get; set;} = 0;

    public string Text
    {
        get => _text;
        private set
        {
            OnTextChanged?.Invoke(this, EventArgs.Empty);
            _text = value;
        }
    }

    public bool IsFocused { get; set; }
    public bool CanFocus { get; set; } = true;
    public bool HasBorder { get; set; } = false;

    private event EventHandler OnTextChanged;
    public ConsoleColor FocusedBgColor { get; set; } = ConsoleColor.Cyan;
    public ConsoleColor FocusedFgColor { get; set; } = ConsoleColor.Black;
    
    public UiInputField(string text)
    {
        Text = text;
    }

    public UiInputField()
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
    }
    
    public void HandleInput(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == ConsoleKey.Backspace)
        {
            if (Text.Length > 0)
            {
                Text = Text.Substring(0, Text.Length - 1);
            }
            return;
        }
        if (keyEventArgs is { Key: ConsoleKey.X, Control: true })
        {
            Text = "";
            return;
        }
        if (keyEventArgs.IsPrintable && !char.IsControl(keyEventArgs.KeyChar))
        {
            Text += keyEventArgs.KeyChar;
        }
    }
    
    private void ComputeSize()
    {
        if (Size != Dimension2D.NullSize) return;
        
        int minWidth = HasBorder ? _text.Length + 2 : _text.Length;
        int minHeight = 1;

        int newWidth = Math.Max(minWidth, Size.Width);
        int newHeight = Math.Max(minHeight, Size.Height);

        Width = newWidth;
        Height = newHeight;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        var bgColor = IsFocused ? FocusedBgColor : BackgroundColor;
        var fgColor = IsFocused ? FocusedFgColor : ForegroundColor;

        int borderOffset = HasBorder ? 2 : 0;
        int availableWidth = Size.Width - borderOffset;

        /*
         * [Base size]
         * [ This is a long text| ]
         * | |                 |  |
         * 1 3                -3  1
         */
        string displayText = Text;
        
        if (Text.Length > availableWidth)
        {
            int startIndex = Text.Length - availableWidth;
            displayText = displayText.Substring(startIndex);
            CursorPosition = availableWidth;
        }
        
        renderer.FillRect(
            WorldPosition.X,
            WorldPosition.Y,
            Size.Width,
            Size.Height,
            ' ',
            bgColor,
            fgColor
        );
        
        if (HasBorder)
        {
            renderer.SetCell(WorldPosition.X, WorldPosition.Y, new Cell('['));
            renderer.SetCell(WorldPosition.X + Size.Width - 1, WorldPosition.Y, new Cell(']'));
        }
        
        int textStartX = WorldPosition.X + (HasBorder ? 1 : 0);
        int textY = WorldPosition.Y + Size.Height / 2;
        
        if (displayText.Length < availableWidth)
        {
            int padding = (availableWidth - displayText.Length) / 2;
            textStartX += padding;
            CursorPosition = displayText.Length;
        }
        
        renderer.DrawText(textStartX, textY, displayText, bgColor, fgColor);
        
        if (IsFocused)
            renderer.SetCell(textStartX + CursorPosition, textY, new ('|', bgColor, fgColor));
    }
}