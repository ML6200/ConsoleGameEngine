using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using NLog;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;



public class UiInputBox : UiPanel
{
    private readonly string _title;
    private readonly string _message;
    
    private readonly UiLabel _titleLabel;
    private readonly UiInputField _inputField;
    private readonly UiButton _cancelButton;
    private readonly GraphicsComponent _parent;
    private readonly UiButton _okButton;
    private readonly ConsoleRenderManager _renderManager;
    private Logger _logger = LogManager.GetCurrentClassLogger();
    
    public event Action<string>? OnOk;
    public event EventHandler OnCancelled;

    public UiInputBox(GraphicsComponent parent, ConsoleRenderManager renderManager, 
        InputManager inputManager,
        string title, string message)
    {
        _renderManager = renderManager;
        
        ForegroundColor = ConsoleColor.White;
        BackgroundColor = ConsoleColor.Blue;
        
        Size = new Dimension2D(20, 6);
        
        parent.AddChild(this);
        _parent = parent;
        _title = title;
        _message = message;
        _titleLabel = new UiLabel(_title)
        {
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = this.BackgroundColor,
        };
        _inputField = new UiInputField("")
        {
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = this.BackgroundColor,
            Size = new Dimension2D(20, 1),
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
        
        _okButton.OnClick += (e, s) =>
        {
            OnOk?.Invoke(_inputField.Text);
            Close();
        };
        _cancelButton.OnClick += (e, s) =>
        {
            OnCancelled?.Invoke(this, EventArgs.Empty);
            Close();
        };
        
        AddChild(_titleLabel);
        AddChild(_inputField);
        AddChild(_cancelButton);
        AddChild(_okButton);
        
        renderManager.FocusManager.Register(_inputField);
        renderManager.FocusManager.Register(_okButton);
        renderManager.FocusManager.Register(_cancelButton);
        
        HasBorder = true;
        ComputeSizes();
        ComputePositions();
    }

    private void Close()
    {
        Visible = false;
        _parent.RemoveChild(this);
        _renderManager.FocusManager.Unregister(_inputField);
        _renderManager.FocusManager.Unregister(_okButton);
        _renderManager.FocusManager.Unregister(_cancelButton);
    }

    
    private void ComputeSizes()
    {
        // 2 before and 2 after
        int horizontal = _inputField.Size.Width > _titleLabel.Size.Width ? 
            _inputField.Size.Width + 2 : _titleLabel.Size.Width + 2; 
        
        /* 3 below title, 1 below the message*/
        int vertical = _titleLabel.Size.Height + _titleLabel.Size.Height + 4;
        Width = horizontal;
        Height = vertical;
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
        _inputField.RelativePosition = new Point2D(Size.Width / 2 - _inputField.Size.Width / 2,
            Size.Height / 2 - _inputField.Size.Height / 2);
    }
}