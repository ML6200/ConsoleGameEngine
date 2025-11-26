using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Scenes;

/// <summary>
/// Game over scene displaying results and stats.
/// </summary>
public class GameOverScene : IGameScene
{
    private ConsoleEngine _engine;
    private ConsoleGraphicsPanel _rootPanel;
    private readonly Player _player;
    private readonly bool _playerDied;
    private readonly bool _levelComplete;
    private readonly bool _interrupted;
    private GameOverPanel _gameOverPanel;

    public GameOverScene(Player player, bool playerDied, bool levelComplete, bool interrupted)
    {
        _player = player;
        _playerDied = playerDied;
        _levelComplete = levelComplete;
        _interrupted = interrupted;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _rootPanel = _engine.GetRootPanel();
    }

    public void OnEnter()
    {
        // Play appropriate sound effect
        if (_playerDied)
        {
            AudioPlayer.PlaySound(System.IO.Path.Combine("assets", "sounds", "player_death.mp3"));
        }
        else if (_levelComplete)
        {
            AudioPlayer.PlayMusic(System.IO.Path.Combine("assets", "sounds", "level_complete.mp3"));
        }

        // Create game over panel
        _gameOverPanel = new GameOverPanel(_player, _playerDied, _levelComplete, _interrupted)
        {
            RelativePosition = new Position2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            Visible = true
        };
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

        // Clean up panel
        if (_gameOverPanel != null)
        {
            _rootPanel.RemoveChild(_gameOverPanel);
        }
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        // Any key exits
        _engine.Stop();
    }
}

/// <summary>
/// Custom panel for rendering game over screen.
/// </summary>
public class GameOverPanel : ConsoleGraphicsPanel
{
    private readonly Player _player;
    private readonly bool _playerDied;
    private readonly bool _levelComplete;
    private readonly bool _interrupted;

    public GameOverPanel(Player player, bool playerDied, bool levelComplete, bool interrupted)
    {
        _player = player;
        _playerDied = playerDied;
        _levelComplete = levelComplete;
        _interrupted = interrupted;

        BackgroundColor = ConsoleColor.Black;
        ForegroundColor = ConsoleColor.White;
        HasBorder = false;
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        base.Render(renderer);

        int centerX = Console.WindowWidth / 2;
        int centerY = Console.WindowHeight / 2;

        // Draw title message
        if (_playerDied)
        {
            renderer.DrawText(centerX - 5, centerY - 5, "YOU DIED!", ConsoleColor.Red, ConsoleColor.Black);
        }
        else if (_interrupted)
        {
            renderer.DrawText(centerX - 3, centerY - 5, "EXITED", ConsoleColor.Yellow, ConsoleColor.Black);
        }
        else if (_levelComplete)
        {
            renderer.DrawText(centerX - 8, centerY - 5, "LEVEL COMPLETE!", ConsoleColor.Green, ConsoleColor.Black);
        }

        // Draw separator
        renderer.DrawText(centerX - 10, centerY - 2, "═══════════════════", ConsoleColor.White, ConsoleColor.Black);

        // Draw stats
        renderer.DrawText(centerX - 10, centerY, $"Final XP: {_player.CombatPoints}", ConsoleColor.Cyan, ConsoleColor.Black);
        renderer.DrawText(centerX - 10, centerY + 1, $"Demons Killed: {_player.CombatPoints / 2}", ConsoleColor.Green, ConsoleColor.Black);

        // Draw bottom separator
        renderer.DrawText(centerX - 10, centerY + 3, "═══════════════════", ConsoleColor.White, ConsoleColor.Black);

        // Draw exit prompt
        renderer.DrawText(centerX - 12, centerY + 5, "Press any key to exit...", ConsoleColor.DarkGray, ConsoleColor.Black);
    }
}
