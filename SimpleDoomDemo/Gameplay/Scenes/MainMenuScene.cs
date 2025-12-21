using System;
using System.Text;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using NLog;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MainMenuScene : IGameScene
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private ConsoleEngine _engine;
    private UiPanel _menuPanel;
    private UiButton _playButton;
    private UiButton _quitButton;
    private UiButton _settingsButton;
    private UiButton _mapEditButton;

    public MainMenuScene()
    {
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
    }

    public void OnEnter()
    {
        int centerX = _engine.ScreenSize.Width / 2;
        int centerY = _engine.ScreenSize.Height / 2;

        // Create menu panel with blue background
        _menuPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(_engine.ScreenSize.Width, _engine.ScreenSize.Height),
            HasBorder = false
        };
        _engine.UiViewport.AddChild(_menuPanel);
        _engine.Input.PushMode(InputMode.KeyInput);

        // Create Play button
        _playButton = new UiButton("▶ PLAY GAME")
        {
            RelativePosition = new Point2D(centerX - 10, centerY - 8),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ForegroundColor = ConsoleColor.White,
        };
        _playButton.OnClick += OnPlayClicked;
        _menuPanel.AddChild(_playButton);
        
        _settingsButton = new UiButton("⚙ Settings")
        {
            RelativePosition = new Point2D(centerX - 10, centerY - 4),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.DarkBlue,
            BackgroundColor = ConsoleColor.Blue,
            ForegroundColor = ConsoleColor.White,
        };
        _settingsButton.OnClick += Settings;
        _menuPanel.AddChild(_settingsButton);
        
        _mapEditButton = new UiButton("✎ Map editor")
        {
            RelativePosition = new Point2D(centerX - 10, centerY),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.DarkGray,
            BackgroundColor = ConsoleColor.Gray,
            ForegroundColor = ConsoleColor.Black,
        };
        _mapEditButton.OnClick += EditMap;
        _menuPanel.AddChild(_mapEditButton);

        // Create Quit button
        _quitButton = new UiButton("⏼ QUIT")
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 6),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.DarkRed,
            ForegroundColor = ConsoleColor.White,
        };
        _quitButton.OnClick += OnQuitClicked;
        _menuPanel.AddChild(_quitButton);

        if (!DoomGameManager.IsTipShown)
        {
            UiMsgBox msgBox = new UiMsgBox(_menuPanel, _engine.UiManager, _engine.Input,
                "💡 Tip of the day", "You can select buttons with up and down arrows.");
            msgBox.OnComplete += state =>
            {
                DoomGameManager.IsTipShown = true;
                EnableButtons();
            };
        }

        if (DoomGameManager.IsTipShown) EnableButtons();

        _engine.RenderManager.OnWindowResized += WindowResized;
        
        _menuPanel.RelativePosition = new Point2D(0, -_engine.RootPanel().ScreenSize.Height);
        _menuPanel.AddAnimation(AnimationTween.MoveTo(_menuPanel, new Point2D(0, 0), 0.5));
    }

    private void EnableButtons()
    {
        // _engine.UiManager.Register(_playButton);
        // _engine.UiManager.Register(_settingsButton);
        // _engine.UiManager.Register(_mapEditButton);
        // _engine.UiManager.Register(_quitButton);
    }

    private void DisableButtons()
    {
        //_engine.UiManager.UnregisterAll();
    }

    private void WindowResized(object? sender, EventArgs e)
    {
        int x = _engine.RootPanel().ScreenSize.Width / 2 - _menuPanel.Size.Width / 2;
        int y = _engine.RootPanel().ScreenSize.Height / 2 - _menuPanel.Size.Height / 2;
        
        foreach (var child in _menuPanel.Children)
        {
            child.RelativePosition = new Point2D(x, y) + child.RelativePosition;
        }
        _menuPanel.Size = _engine.RootPanel().Size;
    }

    private void Settings(object? sender, EventArgs e)
    {
        _engine.LoadScene(new SettingsScene());
    }

    private void EditMap(object? sender, EventArgs e)
    {
        _engine.LoadScene(new MapEditorScene());
    }

    public void OnUpdate(double deltaTime)
    {
        // Menu doesn't need logic updates
    }

    public void OnExit()
    {
        // Remove menu panel from root
        _engine.UiViewport.RemoveChild(_menuPanel);
        _engine.Input.PopMode();
    }
    
    private void OnPlayClicked(object sender, EventArgs e)
    {
        // Load game scene
        try
        {
            MapParser mapParser = new MapParser(DoomGameManager.GameSettings.DefaultMap);
            DoomGameScene gameScene = new DoomGameScene(mapParser.GetPlayer(), 
                mapParser.CollectDemons(), 
                mapParser.CollectItems());
        
            _engine.LoadScene(gameScene);
        }
        catch (Exception exception)
        {
            DisableButtons();
            
            string message = exception.Message;
            message = WrapText(message);
            
            UiMsgBox msgBox = new UiMsgBox(_engine.RootPanel(),
                _engine.UiManager, _engine.Input,
                "Failed to load", message);

            msgBox.OnComplete += result =>
            {
                EnableButtons();
            };
            _logger.Error(exception);
        }
    }

    // very dumb solution but works
    private static string WrapText(string message)
    {
        if (message.Length > 100)
        {
            StringBuilder msg = new StringBuilder();
            for (int i = 0; i < message.Length; i++)
            {
                if (i > 0 && i % 50 == 0)
                    msg.AppendLine();
                    
                msg.Append(message[i]);
            }
            message = msg.ToString();
        }

        return message;
    }

    private void OnQuitClicked(object sender, EventArgs e)
    {
        UiMsgBox msgBox = new UiMsgBox(_menuPanel, _engine.UiManager, _engine.Input, 
            "Quit", "Are you sure you want to quit?");

        msgBox.OnComplete += state =>
        {
            if (state == MessageOptionState.Ok)
            {
                _engine.Stop();
                _engine.Dispose();
            }
        };
    }
}
