using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiInputField : GraphicsComponent, IFocusable, IUiInput
{
    private string _text = "";
    private UiLabel _cursorLabel;
    private Animation _cursorAnim;
    
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
    public AnsiColor FocusedBgColor { get; set; } = AnsiColor.Cyan;
    public AnsiColor FocusedFgColor { get; set; } = AnsiColor.Black;
    
    public UiInputField(string text)
    {
        Text = text;
        InitializeCursor();
    }

    public UiInputField()
    {
        InitializeCursor();
    }

    private void InitializeCursor()
    {
        _cursorLabel = new UiLabel("|")
        {
            Visible = false
        };
        AddChild(_cursorLabel);

        _cursorAnim = AnimationTween.Blink(_cursorLabel, 1);
    }

    public void OnFocusGained()
    {
        HasBorder = true;
        _cursorLabel.Visible = true;
        _cursorLabel.AddAnimation(_cursorAnim);
        _cursorAnim.Resume();
    }

    public void OnFocusLost()
    {
        HasBorder = false;
        _cursorLabel.Visible = false;
        _cursorLabel.ClearAnimations();
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
            _cursorAnim.Freeze();
        }
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        var bgColor = IsFocused ? FocusedBgColor : BackgroundColor;
        var fgColor = IsFocused ? FocusedFgColor : ForegroundColor;

        var style = new RenderStyle(bgColor, fgColor, FontStyle);

        int borderOffset = HasBorder ? 2 : 0;
        int availableWidth = Size.Width - borderOffset;

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
            style
        );

        if (HasBorder)
        {
            renderer.SetCell(WorldPosition.X, WorldPosition.Y, new Cell('[', style));
            renderer.SetCell(WorldPosition.X + Size.Width - 1, WorldPosition.Y, new Cell(']', style));
        }

        int textStartX = WorldPosition.X + (HasBorder ? 1 : 0);
        int textY = WorldPosition.Y + Size.Height / 2;

        if (displayText.Length < availableWidth)
        {
            int padding = (availableWidth - displayText.Length) / 2;
            textStartX += padding;
            CursorPosition = displayText.Length;
        }

        renderer.DrawText(textStartX, textY, displayText, style);

        // Update cursor label position and colors
        if (IsFocused)
        {
            _cursorLabel.WorldPosition = new Point2D(textStartX + CursorPosition, textY);
            _cursorLabel.BackgroundColor = bgColor;
            _cursorLabel.ForegroundColor = fgColor;
        }
    }
}