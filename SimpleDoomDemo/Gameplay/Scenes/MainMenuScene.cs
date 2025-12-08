using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MainMenuScene : IGameScene
{
    private ConsoleEngine _engine;
    private UiPanel _menuPanel;
    private UiButton _playButton;
    private UiButton _quitButton;
    private UiButton _settingsButton;
    private string _mapPath;

    public MainMenuScene(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
    }

    public void OnEnter()
    {
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;

        // Create menu panel with blue background
        _menuPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(_engine.RootPanel().ScreenSize.Width, _engine.RootPanel().ScreenSize.Height),
            HasBorder = false
        };
        _engine.RootPanel().AddChild(_menuPanel);

        // Create Play button
        _playButton = new UiButton("▶ PLAY GAME")
        {
            RelativePosition = new Point2D(centerX - 10, centerY - 3),
            Size = new Dimension2D(20, 3),
            NormalBgColor = ConsoleColor.DarkGreen,
            FocusedBgColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ForegroundColor = ConsoleColor.White,
        };
        _playButton.OnClick += OnPlayClicked;
        _menuPanel.AddChild(_playButton);
        
        _settingsButton = new UiButton("⚙ Map Editor")
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 1),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.DarkBlue,
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.White,
        };
        _settingsButton.OnClick += EditMap;
        _menuPanel.AddChild(_settingsButton);

        // Create Quit button
        _quitButton = new UiButton("⏼ QUIT")
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 5),
            Size = new Dimension2D(20, 3),
            NormalBgColor = ConsoleColor.DarkRed,
            FocusedBgColor = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.DarkRed,
            ForegroundColor = ConsoleColor.White,
        };
        _quitButton.OnClick += OnQuitClicked;
        _menuPanel.AddChild(_quitButton);
        
        UiInputField inputField = new UiInputField(_engine.Input)
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 10),
            Size = new Dimension2D(20, 1),
            FocusedBgColor = ConsoleColor.DarkGray,
            ForegroundColor = ConsoleColor.White,
        };

        inputField.Visible = false;
        
        _menuPanel.AddChild(inputField);

        // Subscribe to input for navigation
        _engine.RenderManager.FocusManager.Register(_playButton);
        _engine.RenderManager.FocusManager.Register(_settingsButton);
        _engine.RenderManager.FocusManager.Register(_quitButton);
        _engine.RenderManager.FocusManager.Register(inputField);
    }

    private void EditMap(object? sender, EventArgs e)
    {
        _engine.LoadScene(new MapEditor(_mapPath));
    }

    public void OnUpdate(double deltaTime)
    {
        // Menu doesn't need logic updates
    }

    public void OnExit()
    {
        // Clean up buttons from menu panel
        _menuPanel.RemoveChild(_playButton);
        _menuPanel.RemoveChild(_quitButton);

        // Remove menu panel from root
        _engine.RootPanel().RemoveChild(_menuPanel);
    }
    
    private void OnPlayClicked(object sender, EventArgs e)
    {
        // Load game scene
        if (System.IO.File.Exists(_mapPath))
        {
            var gameScene = new DoomGameScene();
            Mapper mapper = new Mapper(_mapPath);
            gameScene.Items = mapper.CollectItems();
            gameScene.Player = mapper.GetPlayer();
            gameScene.Demons = mapper.CollectDemons();
            
            _engine.LoadScene(gameScene);
        }
    }

    private void OnQuitClicked(object sender, EventArgs e)
    {
        _engine.Stop();
        _engine.Dispose();
    }
}
