using System;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiMsgBox : UiPanel
{
    private readonly string _title;
    private readonly string _message;
    
    private readonly UiLabel _titleLabel;
    private readonly UiLabel _messageLabel;
    private readonly UiButton _cancelButton;
    private readonly GraphicsComponent _parent;
    private readonly UiButton _okButton;
    
    public event Action<MessageOptionState>? OnComplete;

    public UiMsgBox(GraphicsComponent parent, ConsoleRenderManager renderManager, 
        InputManager inputManager,
        string title, string message)
    {
        ForegroundColor = ConsoleColor.White;
        BackgroundColor = ConsoleColor.Blue;
        parent.AddChild(this);
        _parent = parent;
        _title = title;
        _message = message;
        _titleLabel = new UiLabel(_title)
        {
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = this.BackgroundColor,
        };
        _messageLabel = new UiLabel(_message)
        {
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = this.BackgroundColor,
        };
        _cancelButton = new UiButton("Cancel")
        {
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.White,
            FocusedBgColor = ConsoleColor.Yellow,
            FocusedFgColor = ConsoleColor.DarkBlue
        };
        _okButton = new UiButton("OK")
        {
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.White,
            FocusedBgColor = ConsoleColor.Yellow,
            FocusedFgColor = ConsoleColor.DarkBlue
        };
        
        _okButton.OnClick += (e, s) => Close(MessageOptionState.Ok);
        _cancelButton.OnClick += (e, s) => Close(MessageOptionState.Cancel);
        
        inputManager.OnKeyPressed += (e, s) =>
        {
            if (s.Key == ConsoleKey.Y )
                Close(MessageOptionState.Ok);
            else if (s.Key == ConsoleKey.Escape || s.Key == ConsoleKey.N)
                Close(MessageOptionState.Cancel);
        };
        
        AddChild(_titleLabel);
        AddChild(_messageLabel);
        AddChild(_cancelButton);
        AddChild(_okButton);
        
        renderManager.FocusManager.Register(_okButton);
        renderManager.FocusManager.Register(_cancelButton);
        
        HasBorder = true;
        ComputeSizes();
        ComputePositions();
    }

    private void Close(MessageOptionState option)
    {
        OnComplete?.Invoke(option);
        Visible = false;
        _parent.RemoveChild(this);
    }


    private void ComputeSizes()
    {
        // 2 before and 2 after
        int horizontal = _messageLabel.Size.Width + 4; 
        /* 3 below title, 1 below the message*/
        int vertical = _messageLabel.Size.Height + _titleLabel.Size.Height + 4;
        Size = new Dimension2D(horizontal, vertical);
    }

    private void ComputePositions()
    {
        int midPosPx = _parent.Size.Width / 2;
        int midPosTx = Size.Width / 2;
        int midPosPy = _parent.Size.Height / 2;
        int midPosTy = Size.Height / 2;
        
        RelativePosition = new Point2D(midPosPx - midPosTx, midPosPy - midPosTy);
        
        _okButton.RelativePosition = new Point2D(2, Size.Height - 2);
        _cancelButton.RelativePosition = _okButton.RelativePosition + new Point2D(3, 0);
        _titleLabel.RelativePosition = new Point2D(Size.Width / 2 - _titleLabel.Size.Width / 2, 0);
        _messageLabel.RelativePosition = new Point2D(Size.Width / 2 - _messageLabel.Size.Width / 2,
            Size.Height / 2 - _messageLabel.Size.Height / 2);
    }
}

public enum MessageOptionState
{
    Ok,
    Cancel,
}