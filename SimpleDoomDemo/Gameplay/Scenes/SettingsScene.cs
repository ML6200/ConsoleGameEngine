using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class SettingsScene : IGameScene
{
    private UiPanel _settingsPanel;
    private UiButton _backButton;
    private UiButton _saveAndBackButton;
    
    private UiLabel _mapLabel;
    private UiInputField _mapField;
    
    private UiLabel _assetsLabel;
    private UiInputField _assetsField;
    
    private UiLabel _titleLabel;
    private UiButton _browseButton;
    private ConsoleEngine _engine;
    
    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _settingsPanel = new UiPanel()
        {
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.White,
            Size = _engine.RootPanel().Size,
            BorderColor = ConsoleColor.Yellow
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
    }

    private void Back(object? sender, EventArgs e)
    {
        _engine.LoadScene(new MainMenuScene());
    }

    private void SaveAndBack(object? sender, EventArgs e)
    {
        if (!MapParser.HasCorrectExtension(_mapField.Text))
        {
            DisableButtons();
            UiMsgBox msgBox = new UiMsgBox(_settingsPanel, _engine.RenderManager, _engine.Input,
                "Format mismatch", "Only '.dcmf' or '.map' is acceptable!");
            msgBox.OnComplete += state =>
            {
                EnableButtons();
            };
        }
        else
        {
            DoomGameManager.GameSettings.DefaultMap = _mapField.Text;
            DoomGameManager.GameSettings.AudioAssetsPath = _assetsField.Text;
            DoomGameManager.SaveSettings();
            _engine.LoadScene(new MainMenuScene());    
        }
    }

    public void OnEnter()
    {
        // map setting
        _mapLabel = new UiLabel("Default map path:")
        {
            RelativePosition = new Point2D(3, 3),
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.Yellow,
        };
        _settingsPanel.AddChild(_mapLabel);
        
        _mapField = new UiInputField(DoomGameManager.GameSettings.DefaultMap, _engine.Input)
        {
            RelativePosition = new Point2D(3, 4),
            Size = new Dimension2D(40, 1),
            BackgroundColor = ConsoleColor.DarkYellow,
            ForegroundColor = ConsoleColor.Blue,
        };
        _settingsPanel.AddChild(_mapField);
        
        // music assets
        _assetsLabel = new UiLabel("Music assets path(folder):")
        {
            RelativePosition = new Point2D(3, 6),
            BackgroundColor = ConsoleColor.DarkBlue,
            ForegroundColor = ConsoleColor.Yellow,
        };
        _settingsPanel.AddChild(_assetsLabel);
        
        _assetsField = new UiInputField(DoomGameManager.GameSettings.AudioAssetsPath, _engine.Input)
        {
            RelativePosition = new Point2D(3, 7),
            Size = new Dimension2D(40, 1),
            BackgroundColor = ConsoleColor.DarkYellow,
            ForegroundColor = ConsoleColor.Blue,
        };
        _settingsPanel.AddChild(_assetsField);
        

        _browseButton = new UiButton("Browse")
        {
            RelativePosition = new Point2D(_mapField.Size.Width + 4, 4),
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.Yellow,
            FocusedBgColor = ConsoleColor.Yellow,
            FocusedFgColor = ConsoleColor.Blue
        };
        
        EnableButtons();

        _saveAndBackButton.OnClick += SaveAndBack;
        _backButton.OnClick += Back;
        _engine.RenderManager.OnWindowResized += ResizePanel;
        _browseButton.OnClick += Browse;
    }

    private void EnableButtons()
    {
        _engine.RenderManager.FocusManager.Register(_saveAndBackButton);
        _engine.RenderManager.FocusManager.Register(_backButton);
        _engine.RenderManager.FocusManager.Register(_mapField);
        _engine.RenderManager.FocusManager.Register(_assetsField);
        //_engine.RenderManager.FocusManager.Register(_browseButton);
    }

    private void DisableButtons()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
    }

    private void Browse(object? sender, EventArgs e)
    {
        
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