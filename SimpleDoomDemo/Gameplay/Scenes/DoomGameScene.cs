using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using NLog;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Systems;
using SimpleDoomDemo.Gameplay.UI;
using SimpleDoomEngine;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class DoomGameScene : IGameScene
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    // ============================= ENGINE ==============================
    private ConsoleEngine _engine;
    private UiPanel _rootPanel;
    private InputManager _input;

    // ============================= UI ==============================
    private GameHud _hud;

    // ============================= ENTITIES ==============================
    public Player Player { get; private set; }
    public List<Demon> Demons { get; private set; }
    public List<GameItem> Items { get; private set; }

    // ============================= SYSTEMS ==============================
    private MovementSystem _movementSystem;
    private CombatSystem _combatSystem;
    private InteractionSystem _interactionSystem;
    private ControlSystem _controlSystem;

    // ============================= GAME STATE ==============================
    private bool Interrupted { get; set; }
    public bool Exited { get; set; }
    
    private bool _gameOverHandled = false;
    public Dimension2D WorldSize { get; private set; }

    // ============================= SYNCHRONIZATION ==============================
    private readonly Lock _visibilityLock = new();
    
    private readonly AudioEngine _audioEngine = new();

    public DoomGameScene()
    {
        // Initialize entities
        Player = new Player(0, 0);
        Demons = new List<Demon>();
        Items = new List<GameItem>();
    }

    public DoomGameScene(Player player, List<Demon> demons, List<GameItem> items)
    {
        Player = player;
        Demons = demons;
        Items = items;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        // Initialize systems
        _movementSystem = new MovementSystem(this);
        _combatSystem = new CombatSystem(this);
        _interactionSystem = new InteractionSystem(this);
        _controlSystem = new ControlSystem(this, _combatSystem);
        
        _engine = consoleEngine;
        _rootPanel = _engine.RootPanel();
        _input = _engine.Input;

        // Setup world size and camera
        int worldWidth = _engine.RootPanel().ScreenSize.Width * 3;
        int worldHeight =_engine.RootPanel().ScreenSize.Height * 3;

        WorldSize = new Dimension2D(worldWidth, worldHeight);
        _engine.Camera.WorldSize = WorldSize;

        _engine.Camera.SetCameraPosition(new Point2D(worldWidth / 2, worldHeight / 2));

        // Subscribe to input events
        _input.OnKeyPressed += OnKeyPressed;

        // Setup cleanup handlers
        Console.CancelKeyPress += (sender, e) => _audioEngine.StopAll();
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => _audioEngine.StopAll();
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => _audioEngine.StopAll();
    }

    public void OnEnter()
    {
        // Add all game entities to root panel (they will use camera transformation)
        if (Player is null)
        {
            _logger.Error("Player is null");
            return;
        }
        _rootPanel.AddChild(Player);

        foreach (var item in Items)
        {
            _rootPanel.AddChild(item);
        }

        foreach (var demon in Demons)
        {
            _rootPanel.AddChild(demon);
            demon.UpdateVisibility(Player.WorldPosition, Player.SightRange);
        }

        // Create and add HUD (positioned at bottom of screen)
        int hudWidth = Console.WindowWidth;
        int hudHeight = 1;
        _hud = new GameHud(_engine, Player, hudWidth, hudHeight)
        {
            RelativePosition = new Point2D(0, Console.WindowHeight - 1)
        };
        _rootPanel.AddChild(_hud);

        // Start music
        //AudioPlayer.PlayMusic(Path.Combine("assets", "sounds", "doom_music.mp3"));
        _audioEngine.Play(Path.Combine("assets", "sounds", "doom_music.mp3"), "main");
    }

    public void OnUpdate(double deltaTime)
    {
        // Check game over conditions (only once)
        if ((Interrupted || !Player.Alive || Exited) && !_gameOverHandled)
        {
            _gameOverHandled = true;
            HandleGameOver();
            return;
        }
        
        int cameraX = Player.WorldPosition.X - _engine.Camera.CameraSize.Width / 2;
        int cameraY = Player.WorldPosition.Y - _engine.Camera.CameraSize.Height / 2;
        _engine.Camera.SetCameraPosition(new Point2D(cameraX, cameraY));

        if (_gameOverHandled)
        {
            return;
        }
        
        UpdateGameLogic(deltaTime);
        
    }

    public void StopAllAudio()
    {
        _audioEngine.StopAll();
    }
    
    public void OnExit()
    {
        StopAllAudio();
        _input.OnKeyPressed -= OnKeyPressed!;

        // Clean up all game entities from the root panel
        _rootPanel.RemoveChild(Player);

        CleanupEntities();

        if (_hud != null)
        {
            _rootPanel.RemoveChild(_hud);
        }
    }

    private void UpdateGameLogic(double deltaTime)
    {
        // Update all systems
        _controlSystem.Update(deltaTime);
        _movementSystem.Update(deltaTime);
        _interactionSystem.Update(deltaTime);

        // Update visibility (fog of war)
        UpdateVisibility();
        // Update HUD
        _hud.UpdateHUD(new Point2D(0, _rootPanel.ScreenSize.Height - 1));

        // Cleanup dead entities
        CleanupEntities();
    }

    private void UpdateVisibility()
    {
        Point2D playerPos = Player.WorldPosition;
        double sightRange = Player.SightRange;

        lock (_visibilityLock)
        {
            // Update item visibility
            foreach (var item in Items)
            {
                item.UpdateVisibility(playerPos, sightRange);
            }

            // Update demon visibility
            foreach (var demon in Demons)
            {
                demon.UpdateVisibility(playerPos, sightRange);
            }
        }
    }

    private void OnKeyPressed(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case ConsoleKey.Escape:
                UiMsgBox msgBox = new UiMsgBox(_rootPanel, _engine.RenderManager, _engine.Input,
                    "You are about to exit", "Are you sure you want to exit the game? (Y/N)");
                msgBox.OnComplete += state =>
                {
                    if (state == MessageOptionState.Ok)
                    {
                        Interrupted = true;
                    }
                };
                break;

            // Movement
            case ConsoleKey.LeftArrow:
                MovePlayerBy(-1, 0);
                break;
            case ConsoleKey.RightArrow:
                MovePlayerBy(1, 0);
                break;
            case ConsoleKey.UpArrow:
                MovePlayerBy(0, -1);
                break;
            case ConsoleKey.DownArrow:
                MovePlayerBy(0, 1);
                break;

            // Combat
            case ConsoleKey.A:
                _combatSystem.PlayerAttack();
                break;
            case ConsoleKey.S:
                _combatSystem.PlayerBFGAttack();
                break;

            // Interaction
            case ConsoleKey.D:
                _interactionSystem.ProcessPlayerDirectInteraction();
                break;
        }
    }

    private void MovePlayerBy(int x, int y)
    {
        Point2D targetPoint = Player.WorldPosition + new Point2D(x, y);
        _movementSystem.MovePlayer(targetPoint);
    }

    private void CleanupEntities()
    {
        // Remove unavailable items from rendering
        foreach (var item in Items)
        {
            if (!item.Available)
            {
                _rootPanel.RemoveChild(item);
            }
        }
        Items.RemoveAll(item => !item.Available);

        // Remove dead demons from rendering
        foreach (var demon in Demons)
        {
            if (!demon.Alive)
            {
                _rootPanel.RemoveChild(demon);
            }
        }
        Demons.RemoveAll(demon => !demon.Alive);
    }

    private void HandleGameOver()
    {
        _audioEngine.StopAll();
        // Create game over scene
        var gameOverScene = new GameOverScene(this, Player, !Player.Alive, Exited, Interrupted);
        _engine.LoadScene(gameOverScene);
    }

    public void PlaySoundEffect(SoundEffectType soundEffectType)
    {
        switch (soundEffectType)
        {
            case SoundEffectType.Door:
                _audioEngine.Play(Path.Combine("assets", "sounds", "door.mp3"), "1");
                break;
            case SoundEffectType.BFG:
                _audioEngine.Play(Path.Combine("assets", "sounds", "bfg.mp3"), "2");
                break;
            case SoundEffectType.ItemPickup:
                _audioEngine.Play(Path.Combine("assets", "sounds", "item_pickup.mp3"), "3");
                break;
            case SoundEffectType.Pain:
                _audioEngine.Play(Path.Combine("assets", "sounds", "pain.mp3"), "4");
                break;
            case SoundEffectType.PlayerDeath:
                _audioEngine.Play(Path.Combine("assets", "sounds", "player_death.mp3"), "5");
                break;
            case SoundEffectType.Shotgun:
                _audioEngine.Play(Path.Combine("assets", "sounds", "shotgun.mp3"), "6");
                break;
            case SoundEffectType.LevelComplete:
                _audioEngine.Play(Path.Combine("assets", "sounds", "level_complete.mp3"), "7");
                break;
        }
    }
}