using System;
using System.Collections.Generic;
using System.IO;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Scenes;
using SimpleDoomDemo.Gameplay.Systems;
using SimpleDoomDemo.Gameplay.UI;
using SimpleDoomEngine;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;
using GameOverScene = SimpleDoomDemo.Gameplay.Scenes.GameOverScene;

namespace SimpleDoomDemo.Gameplay;

/// <summary>
/// Main game scene using Entity-Component-System architecture.
/// Integrates with ConsoleEngine for automatic update/render loop.
/// </summary>
public class DoomGameScene : IGameScene
{
    // ============================= ENGINE ==============================
    private ConsoleEngine _engine;
    private ConsoleGraphicsPanel _rootPanel;
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
    private AISystem _aiSystem;

    // ============================= GAME STATE ==============================
    public bool Interrupted { get; set; }
    public bool Exited { get; set; }
    public double PlayerFillingRatio { get; private set; } = 0.4;
    private bool _gameOverHandled = false;

    // ============================= TIMING ==============================
    private const double LOGIC_UPDATE_INTERVAL = 0.5; // 500ms in seconds
    private double _logicAccumulator = 0;

    // ============================= SYNCHRONIZATION ==============================
    private readonly object _visibilityLock = new object();
    public object VisibilityLock => _visibilityLock;

    public DoomGameScene()
    {
        // Initialize entities
        Player = new Player(0, 0);
        Demons = new List<Demon>();
        Items = new List<GameItem>();

        // Initialize systems
        _movementSystem = new MovementSystem(this);
        _combatSystem = new CombatSystem(this);
        _interactionSystem = new InteractionSystem(this);
        _aiSystem = new AISystem(this, _combatSystem);
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        _rootPanel = _engine.GetRootPanel();
        _input = _engine.Input;

        // Subscribe to input events
        _input.OnKeyPressed += OnKeyPressed;

        // Setup cleanup handlers
        Console.CancelKeyPress += (sender, e) => AudioPlayer.StopMusic();
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => AudioPlayer.StopMusic();
    }

    public void OnEnter()
    {
        // Add all entities to root panel as children
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
            RelativePoint = new Point2D(0, Console.WindowHeight - 1)
        };
        _rootPanel.AddChild(_hud);

        // Start music
        AudioPlayer.PlayMusic(Path.Combine("assets", "sounds", "doom_music.mp3"));
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

        // Don't update game logic if game over was triggered
        if (_gameOverHandled)
        {
            return;
        }

        // Accumulate time for logic updates (run at 500ms intervals)
        //_logicAccumulator += deltaTime;

        /*
        if (_logicAccumulator >= LOGIC_UPDATE_INTERVAL)
        {
            long deltaTimeMs = (long)(_logicAccumulator * 1000);
            UpdateGameLogic(deltaTimeMs);
            _logicAccumulator = 0;
        }
        */
        UpdateGameLogic(deltaTime);

        // Component animations update automatically via engine
        // No need to manually call Update() on components
    }

    public void OnExit()
    {
        AudioPlayer.StopMusic();
        _input.OnKeyPressed -= OnKeyPressed!;
    }

    private void UpdateGameLogic(double deltaTime)
    {
        // Update all systems
        _aiSystem.Update(deltaTime);
        _movementSystem.Update(deltaTime);
        _interactionSystem.Update(deltaTime);

        // Update visibility (fog of war)
        UpdateVisibility();

        // Update HUD
        _hud.UpdateHUD(new Point2D(0, _rootPanel.WorldSize.Height-1));

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
            case ConsoleKey.E:
                Interrupted = true;
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
        AudioPlayer.StopMusic();

        // Create game over scene
        var gameOverScene = new GameOverScene(Player, !Player.Alive, Exited, Interrupted);
        _engine.LoadScene(gameOverScene);
    }

    public void LoadMapFromPlainText(string path)
    {
        try
        {
            string[] lines = File.ReadAllLines(path);
            string[] firstLine = lines[0].Split(",");
            int y = int.Parse(firstLine[0]);
            int x = int.Parse(firstLine[1]);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                for (int j = 0; j < x; j++)
                {
                    switch (line[j])
                    {
                        // ITEMS
                        case 'A':
                            Items.Add(new GameItem(j, i - 1, ItemType.AMMO));
                            break;
                        case 'B':
                            Items.Add(new GameItem(j, i - 1, ItemType.BFGCELL));
                            break;
                        case 'D':
                            Items.Add(new GameItem(j, i - 1, ItemType.DOOR));
                            break;
                        case 'E':
                            Items.Add(new GameItem(j, i - 1, ItemType.LEVELEXIT));
                            break;
                        case 'M':
                            Items.Add(new GameItem(j, i - 1, ItemType.MEDKIT));
                            break;
                        case 'T':
                            Items.Add(new GameItem(j, i - 1, ItemType.TOXICWASTE));
                            break;
                        case 'W':
                            Items.Add(new GameItem(j, i - 1, ItemType.WALL));
                            break;

                        // DEMONS
                        case 'z':
                            Demons.Add(new Zombieman(j, i - 1));
                            break;
                        case 'i':
                            Demons.Add(new Imp(j, i - 1));
                            break;
                        case 'm':
                            Demons.Add(new Mancubus(j, i - 1));
                            break;

                        // PLAYER
                        case 'p':
                            Player.RelativePoint = new Point2D(j, i - 1);
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    public void PlaySoundEffect(SoundEffectType soundEffectType)
    {
        switch (soundEffectType)
        {
            case SoundEffectType.Door:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "door.mp3"));
                break;
            case SoundEffectType.BFG:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "bfg.mp3"));
                break;
            case SoundEffectType.ItemPickup:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "item_pickup.mp3"));
                break;
            case SoundEffectType.Pain:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "pain.mp3"));
                break;
            case SoundEffectType.PlayerDeath:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "player_death.mp3"));
                break;
            case SoundEffectType.Shotgun:
                AudioPlayer.PlaySound(Path.Combine("assets", "sounds", "shotgun.mp3"));
                break;
            case SoundEffectType.LevelExit:
                AudioPlayer.PlayMusic(Path.Combine("assets", "sounds", "level_complete.mp3"));
                break;
        }
    }
}