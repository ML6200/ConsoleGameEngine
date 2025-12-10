using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class SettingsScene : IGameScene
{
    private UiPanel _settingsPanel;
    private UiInputField _inputField;
    private UiButton _backButton;
    private UiButton _saveAndBackButton;
    private UiLabel _titleLabel;
    private UiLabel _mapLabel;
    private ConsoleEngine _engine;
    
    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _settingsPanel = new UiPanel()
        {
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.White,
            Size = _engine.RootPanel().Size,
        };
        
        _engine.RootPanel().AddChild(_settingsPanel);
        
        _titleLabel = new UiLabel("Doom game settings")
        {
            ForegroundColor = ConsoleColor.Yellow,
            BackgroundColor = ConsoleColor.DarkBlue,
        };
        _settingsPanel.AddChild(_titleLabel);
        
        
        _saveAndBackButton = new UiButton("Save")
        {
            RelativePosition = new Point2D(3, 1),
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.Yellow,
            FocusedBgColor = ConsoleColor.Yellow,
            FocusedFgColor = ConsoleColor.Blue
        };
        _settingsPanel.AddChild(_saveAndBackButton);
        
        _backButton = new UiButton("Cancel")
        {
            RelativePosition = new Point2D(_saveAndBackButton.Size.Width + 4, 1),
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.Yellow,
            FocusedBgColor = ConsoleColor.DarkRed,
            FocusedFgColor = ConsoleColor.Yellow
        };
        _settingsPanel.AddChild(_backButton);


        // map setting
        _mapLabel = new UiLabel("Default map path:")
        {
            RelativePosition = new Point2D(3, 3),
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.Yellow,
        };
        _settingsPanel.AddChild(_mapLabel);
        _inputField = new UiInputField(DoomGameManager.DefaultMapPath)
        {
            RelativePosition = new Point2D(3, 4),
            Size = new Dimension2D(40, 1),
            BackgroundColor = ConsoleColor.DarkYellow,
            ForegroundColor = ConsoleColor.Blue,
        };
        _settingsPanel.AddChild(_inputField);
        _settingsPanel.AddChild(_backButton);
    }

    private void Back(object? sender, EventArgs e)
    {
        _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
    }

    private void SaveAndBack(object? sender, EventArgs e)
    {
        if (!_inputField.Text.EndsWith(".dcmf"))
        {
            UiMsgBox msgBox = new UiMsgBox(_settingsPanel, _engine.RenderManager, _engine.Input,
                "Format mismatch", "Only DCMF is acceptable!");
            msgBox.OnOptionSelected += state =>
            { };
        }
        else
        {
            DoomGameManager.DefaultMapPath = _inputField.Text;
            _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));    
        }
    }

    public void OnEnter()
    {
        _engine.RenderManager.FocusManager.Register(_saveAndBackButton);
        _engine.RenderManager.FocusManager.Register(_backButton);
        _engine.RenderManager.FocusManager.Register(_inputField);
        
        _saveAndBackButton.OnClick += SaveAndBack;
        _backButton.OnClick += Back;
        _engine.RenderManager.OnWindowResized += ResizePanel;
    }

    private void ResizePanel(object? sender, EventArgs e)
    {
        _settingsPanel.Size = _engine.RootPanel().Size;
    }

    public void OnUpdate(double deltaTime)
    {
        
    }

    public void OnExit()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.RootPanel().RemoveChild(_settingsPanel);
    }
}