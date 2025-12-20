using System;
using System.Drawing;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Scenes;

/// <summary>
/// Game over scene displaying results and stats.
/// </summary>
public class GameOverScene : IGameScene
{
    private ConsoleEngine _engine;
    private UiPanel _rootPanel;
    private readonly Player _player;
    private readonly bool _playerDied;
    private readonly bool _levelComplete;
    private readonly bool _interrupted;
    private GameOverPanel _gameOverPanel;
    private DoomGameScene _game;

    public GameOverScene(DoomGameScene game, Player player, bool playerDied, bool levelComplete, bool interrupted)
    {
        _game = game;
        _player = player;
        _playerDied = playerDied;
        _levelComplete = levelComplete;
        _interrupted = interrupted;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _rootPanel = _engine.RootPanel();
    }

    public void OnEnter()
    {
        // Play appropriate sound effect
        if (_playerDied)
        {
            _game.PlaySoundEffect(SoundEffectType.PlayerDeath);
        }
        else if (_levelComplete)
        {
            _game.PlaySoundEffect(SoundEffectType.LevelComplete);
        }

        // Create game over panel
        _gameOverPanel = new GameOverPanel(_player, _playerDied, _levelComplete, _interrupted)
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            Visible = true
        };
        if (_interrupted) _rootPanel.BackgroundColor = ConsoleColor.DarkBlue;
        if (_playerDied) _rootPanel.BackgroundColor = ConsoleColor.DarkRed;
        if (_levelComplete) _rootPanel.BackgroundColor = ConsoleColor.DarkGreen;
        _rootPanel.AddChild(_gameOverPanel);

        // Subscribe to input
        _engine.Input.OnKeyPressed += OnKeyPressed;
    }

    public void OnUpdate(double deltaTime)
    {
        // No updates needed
    }

    public void OnExit()
    {
        _engine.Input.OnKeyPressed -= OnKeyPressed;
        _game.StopAllAudio();
        _engine.FocusManager.UnregisterAll();
        _engine.Input.OnKeyPressed -= OnKeyPressed;

        _rootPanel.BackgroundColor = ConsoleColor.Black;
        _rootPanel.RemoveChild(_gameOverPanel);
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        _engine.LoadScene(new MainMenuScene()); // later we fix this by adding settings
    }
}

/// <summary>
/// Custom panel for rendering game over screen using UI component building blocks.
/// </summary>
public class GameOverPanel : UiPanel
{
    private readonly Player _player;
    private readonly bool _playerDied;
    private readonly bool _levelComplete;
    private readonly bool _interrupted;

    private UiLabel _titleLabel;
    private UiLabel _separatorLabel;
    private UiLabel _statsLabel;
    private UiLabel _bottomSeparatorLabel;
    private UiLabel _promptLabel;

    public GameOverPanel(Player player, bool playerDied, bool levelComplete, bool interrupted)
    {
        _player = player;
        _playerDied = playerDied;
        _levelComplete = levelComplete;
        _interrupted = interrupted;

        HasBorder = false;

        // Determine title text and colors based on game over reason
        string titleText;
        ConsoleColor bgColor;
        ConsoleColor fgColor = ConsoleColor.White;

        if (_playerDied)
        {
            titleText = "YOU DIED!";
            bgColor = ConsoleColor.DarkRed;
        }
        else if (_interrupted)
        {
            titleText = "EXITED";
            bgColor = ConsoleColor.DarkBlue;
            fgColor = ConsoleColor.Yellow;
        }
        else if (_levelComplete)
        {
            titleText = "LEVEL COMPLETE!";
            bgColor = ConsoleColor.DarkGreen;
        }
        else
        {
            titleText = "";
            bgColor = ConsoleColor.Red;
        }

        // Create UI components
        int centerX = Console.WindowWidth / 2;
        int centerY = Console.WindowHeight / 2;

        // Title label
        _titleLabel = new UiLabel(titleText)
        {
            RelativePosition = new Point2D(centerX - titleText.Length / 2, centerY - 5),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor
        };

        // Top separator
        _separatorLabel = new UiLabel("═══════════════════")
        {
            RelativePosition = new Point2D(centerX - 10, centerY - 2),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor
        };

        // Stats label
        string statsText = $"Final XP: {_player.CombatPoints}\nDemons Killed: {_player.CombatPoints / 2}";
        _statsLabel = new UiLabel(statsText)
        {
            RelativePosition = new Point2D(centerX - 10, centerY),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor
        };

        // Bottom separator
        _bottomSeparatorLabel = new UiLabel("═══════════════════")
        {
            RelativePosition = new Point2D(centerX - 10, centerY + 3),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor
        };

        // Exit prompt
        _promptLabel = new UiLabel("Press any key to exit...")
        {
            RelativePosition = new Point2D(centerX - 12, centerY + 5),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor
        };
        
        UiPanel mainPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            BackgroundColor = bgColor,
            ForegroundColor = fgColor,
            HasBorder = false
        };

        // Add all children
        mainPanel.AddChild(_titleLabel);
        mainPanel.AddChild(_separatorLabel);
        mainPanel.AddChild(_statsLabel);
        mainPanel.AddChild(_bottomSeparatorLabel);
        mainPanel.AddChild(_promptLabel);
        AddChild(mainPanel);
        
        mainPanel.RelativePosition = new Point2D(0, -Console.WindowHeight);
        mainPanel.AddAnimation(AnimationTween.MoveTo(mainPanel, new Point2D(0, 0), 0.5));
        BackgroundColor = bgColor;
        ForegroundColor = fgColor;
    }
}
