using System;
using System.Drawing;
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
        _game.StopAllAudio();
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.Input.OnKeyPressed -= OnKeyPressed;

        _rootPanel.BackgroundColor = ConsoleColor.Black;
        _rootPanel.RemoveChild(_gameOverPanel);
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath)); // later we fix this by adding settings
    }
}

/// <summary>
/// Custom panel for rendering game over screen.
/// </summary>
public class GameOverPanel : UiPanel
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

        BackgroundColor = ConsoleColor.Red;
        ForegroundColor = ConsoleColor.White;
        
        HasBorder = false;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // Full screen UI overlay - don't use camera transformation
        int centerX = ScreenSize.Width / 2;
        int centerY = ScreenSize.Height / 2;

        ConsoleColor color;

        // Fill entire screen based on game over reason
        if (_playerDied)
        {
            color = ConsoleColor.DarkRed;
            renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height, ' ', color);
            renderer.DrawText(centerX - 5, centerY - 5, "YOU DIED!", color);
        }
        else if (_interrupted)
        {
            color = ConsoleColor.DarkBlue;
            renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height, ' ', color);
            renderer.DrawText(centerX - 3, centerY - 5, "EXITED", color, ConsoleColor.Yellow);
        }
        else if (_levelComplete)
        {
            color = ConsoleColor.DarkGreen;
            renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height, ' ', color);
            renderer.DrawText(centerX - 8, centerY - 5, "LEVEL COMPLETE!", color);
        }
        else
        {
            color = ConsoleColor.Red;
            renderer.FillRect(WorldPosition.X, WorldPosition.Y, Size.Width, Size.Height, ' ', color);
        }

        // Draw separator
        renderer.DrawText(centerX - 10, centerY - 2, "═══════════════════", color);

        // Draw stats
        renderer.DrawText(centerX - 10, centerY, $"Final XP: {_player.CombatPoints}", color);
        renderer.DrawText(centerX - 10, centerY + 1, $"Demons Killed: {_player.CombatPoints / 2}", color);

        // Draw bottom separator
        renderer.DrawText(centerX - 10, centerY + 3, "═══════════════════", color);

        // Draw exit prompt
        renderer.DrawText(centerX - 12, centerY + 5, "Press any key to exit...", color);
    }
}
