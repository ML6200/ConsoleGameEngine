using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using NLog;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MainMenuScene : IGameScene
{
    private Logger  _logger = LogManager.GetCurrentClassLogger();
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

        UiMsgBox msgBox = new UiMsgBox(_menuPanel, _engine.RenderManager, _engine.Input,
            "Tip of the day", "You can select buttons with up and down arrows.");
        msgBox.OnOptionSelected += state =>
        {
            
        };
        _menuPanel.AddChild(msgBox);
        
        //if (state == MessageOptionState.Ok)
        //{
            _engine.RenderManager.FocusManager.Register(_playButton);
            _engine.RenderManager.FocusManager.Register(_settingsButton);
            _engine.RenderManager.FocusManager.Register(_quitButton);
        //}

        // Subscribe to input for navigation
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
        _menuPanel.RemoveChild(_settingsButton);

        // Remove menu panel from root
        _engine.RootPanel().RemoveChild(_menuPanel);
    }
    
    private void OnPlayClicked(object sender, EventArgs e)
    {
        // Load game scene
        if (System.IO.File.Exists(_mapPath))
        {
            try
            {
                //Mapper.LoadFromLegacyMap(_mapPath, gameScene.Items, gameScene.Demons, gameScene.Player);
                Mapper mapper = new Mapper(_mapPath);
                DoomGameScene gameScene = new DoomGameScene(mapper.GetPlayer(), 
                    mapper.CollectDemons(), 
                    mapper.CollectItems());
            
                _engine.LoadScene(gameScene);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
                throw;
            }
        }
    }

    private void OnQuitClicked(object sender, EventArgs e)
    {
        _engine.Stop();
        _engine.Dispose();
    }
}
