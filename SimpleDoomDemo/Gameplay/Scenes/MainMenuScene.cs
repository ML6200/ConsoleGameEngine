using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MainMenuScene : IGameScene
{
    private ConsoleEngine _engine;
    private ConsoleGraphicsPanel _rootPanel;
    private ConsoleGraphicsButton _playButton;
    private ConsoleGraphicsButton _quitButton;
    private string _mapPath;

    public MainMenuScene(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _rootPanel = _engine.RootPanel();
    }

    public void OnEnter()
    {
        int centerX = Console.WindowWidth / 2;
        int centerY = Console.WindowHeight / 2;

        // Title is not needed - buttons are self-explanatory

        // Create Play button
        _playButton = new ConsoleGraphicsButton("PLAY GAME")
        {
            RelativePosition = new Point2D(centerX - 10, centerY - 3),
            Size = new Dimension2D(20, 3),
            NormalBgColor = ConsoleColor.DarkGreen,
            FocusedBgColor = ConsoleColor.Green,
            BackgroundColor = ConsoleColor.DarkGreen,
            ForegroundColor = ConsoleColor.White,
            HasBorder = true
        };
        _playButton.OnClick += OnPlayClicked;
        _rootPanel.AddChild(_playButton);

        // Create Quit button
        _quitButton = new ConsoleGraphicsButton("QUIT")
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 2),
            Size = new Dimension2D(20, 3),
            NormalBgColor = ConsoleColor.DarkRed,
            FocusedBgColor = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.DarkRed,
            ForegroundColor = ConsoleColor.White,
            HasBorder = true
        };
        _quitButton.OnClick += OnQuitClicked;
        _rootPanel.AddChild(_quitButton);

        // Subscribe to input for navigation
        _engine.Input.OnKeyPressed += OnKeyPressed;
    }

    public void OnUpdate(double deltaTime)
    {
        // Menu doesn't need logic updates
    }

    public void OnExit()
    {
        _engine.Input.OnKeyPressed -= OnKeyPressed;

        // Clean up buttons
        _rootPanel.RemoveChild(_playButton);
        _rootPanel.RemoveChild(_quitButton);
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case ConsoleKey.UpArrow:
                _playButton.IsFocused = true;
                _quitButton.IsFocused = false;
                break;

            case ConsoleKey.DownArrow:
                _playButton.IsFocused = false;
                _quitButton.IsFocused = true;
                break;

            case ConsoleKey.Enter:
            case ConsoleKey.Spacebar:
                if (_playButton.IsFocused)
                {
                    _playButton.OnFocusActivate();
                }
                else if (_quitButton.IsFocused)
                {
                    _quitButton.OnFocusActivate();
                }
                break;

            case ConsoleKey.Escape:
                _engine.Stop();
                break;
        }
    }

    private void OnPlayClicked(object sender, EventArgs e)
    {
        // Load game scene
        var gameScene = new DoomGameScene();

        if (System.IO.File.Exists(_mapPath))
        {
            gameScene.LoadMapFromPlainText(_mapPath);
        }

        _engine.LoadScene(gameScene);
    }

    private void OnQuitClicked(object sender, EventArgs e)
    {
        _engine.Stop();
    }
}
