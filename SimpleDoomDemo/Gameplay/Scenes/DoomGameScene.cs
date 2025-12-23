using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Audio;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using NLog;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Systems;
using SimpleDoomDemo.Gameplay.UI;
using SimpleDoomEngine;
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

    // ============================= VIEWPORTS ==============================
    private UiViewport _uiViewport;
    private GameViewport _gameViewport;

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

        _gameViewport = _engine.GameViewport;
        _uiViewport = _engine.UiViewport;

        WorldSize = _engine.WorldSize;
        
        _input.PushMode(InputMode.KeyInput);
        HandleInput();

        // Setup cleanup handlers
        Console.CancelKeyPress += (sender, e) => _audioEngine.StopAll();
        AppDomain.CurrentDomain.UnhandledException += (sender, e) => _audioEngine.StopAll();
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => _audioEngine.StopAll();
    }

    public void OnEnter()
    {
        if (LoadEntities()) return;

        AddHud();

        _gameViewport.Camera?.FollowObject(Player);

        // Start music
        //AudioPlayer.PlayMusic(Path.Combine("assets", "sounds", "doom_music.mp3"));
        _audioEngine.Play(
            Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, 
                "mark_lor-war_of_sirens.mp3"), 
            "main",
            true,
            true
        );
        _audioEngine.SetVolume("main", 0);
    }

    private void AddHud()
    {
        // Create and add HUD to UI viewport (screen space - no camera transformation)
        int hudWidth = Console.WindowWidth;
        int hudHeight = 1;
        _hud = new GameHud(_engine, Player, hudWidth, hudHeight)
        {
            RelativePosition = new Point2D(0, Console.WindowHeight - 1)
        };
        _uiViewport.AddChild(_hud);  // Add to UI viewport, not root panel
    }

    private bool LoadEntities()
    {
        // Add all game entities to game viewport (they will use camera transformation)
        _gameViewport.AddChild(Player);

        foreach (var item in Items)
        {
            _gameViewport.AddChild(item);
        }

        foreach (var demon in Demons)
        {
            _gameViewport.AddChild(demon);
            demon.UpdateVisibility(Player.WorldPosition, Player.SightRange);
        }

        return false;
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
        _input.PopMode();
        StopAllAudio();

        // Clean up all game entities from game viewport
        _gameViewport.RemoveChild(Player);

        CleanupAllEntities();

        // Clean up HUD from UI viewport
        _uiViewport.RemoveChild(_hud);
        
    }

    private void CleanupAllEntities()
    {
        foreach (var demon in Demons)
        {
            _gameViewport.RemoveChild(demon);
        }

        foreach (var item in Items)
        {
            _gameViewport.RemoveChild(item);
        }
    }
    
    private void CleanupDeadEntities()
    {
        // Remove unavailable items from game viewport
        foreach (var item in Items)
        {
            if (!item.Available)
            {
                _gameViewport.RemoveChild(item);
            }
        }
        Items.RemoveAll(item => !item.Available);

        // Remove dead demons from game viewport
        foreach (var demon in Demons)
        {
            if (!demon.Alive)
            {
                _gameViewport.RemoveChild(demon);
            }
        }
        Demons.RemoveAll(demon => !demon.Alive);
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
        _hud.UpdateHud(new Point2D(0, _rootPanel.ScreenSize.Height - 1));

        // Cleanup dead entities
        CleanupDeadEntities();
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

    private void HandleInput()
    {
        KeyBinding escapeKeyBinding = KeyBinding.Commons.Escape;
        KeyBinding leftArrowKeyBinding = KeyBinding.Commons.LeftArrow;
        KeyBinding rightArrowKeyBinding = KeyBinding.Commons.RightArrow;
        KeyBinding upArrowKeyBinding = KeyBinding.Commons.UpArrow;
        KeyBinding downArrowKeyBinding = KeyBinding.Commons.DownArrow;
        KeyBinding attackKeyBinding = KeyBinding.Parse("A");
        KeyBinding attackBfgKeyBinding = KeyBinding.Parse("S");
        KeyBinding interactKeyBinding = KeyBinding.Parse("D");
        
        _input.RegisterToScene(this, escapeKeyBinding);
        _input.RegisterToScene(this,leftArrowKeyBinding);
        _input.RegisterToScene(this,rightArrowKeyBinding);
        _input.RegisterToScene(this,upArrowKeyBinding);
        _input.RegisterToScene(this,downArrowKeyBinding);
        _input.RegisterToScene(this,attackKeyBinding);
        _input.RegisterToScene(this,attackBfgKeyBinding);
        _input.RegisterToScene(this,interactKeyBinding);
        
        _input.Subscribe(escapeKeyBinding, () =>
        {
            UiMsgBox msgBox = new UiMsgBox(_rootPanel, _engine.UiManager, _engine.Input,
                "You are about to exit", "Are you sure you want to exit the game? (Y/N)");
            msgBox.OnComplete += state =>
            {
                if (state == MessageOptionState.Ok)
                {
                    Interrupted = true;
                }
            };
        });
        _input.Subscribe(leftArrowKeyBinding, () => { MovePlayerBy(-1, 0); });
        _input.Subscribe(rightArrowKeyBinding, () => { MovePlayerBy(1, 0); });
        _input.Subscribe(upArrowKeyBinding, () => { MovePlayerBy(0, -1); });
        _input.Subscribe(downArrowKeyBinding, () => { MovePlayerBy(0, 1); });
        _input.Subscribe(attackKeyBinding, () => { _combatSystem.PlayerAttack(); });
        _input.Subscribe(attackBfgKeyBinding, () => { _combatSystem.PlayerBFGAttack(); });
        _input.Subscribe(interactKeyBinding, () =>
        {
            _interactionSystem.ProcessPlayerDirectInteraction();
        });
    }

    private void MovePlayerBy(int x, int y)
    {
        Point2D targetPoint = Player.RelativePosition + new Point2D(x, y);
        _movementSystem.MovePlayer(targetPoint);
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
        var soundId = soundEffectType.ToString();
    
        switch (soundEffectType)
        {
            case SoundEffectType.Door:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_door.mp3"), 
                    soundId, 
                    cooldownMs: 500);  // 500ms cooldown
                break;
            case SoundEffectType.BFG:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_bfg.mp3"), 
                    soundId, 
                    cooldownMs: 300);
                break;
            case SoundEffectType.ItemPickup:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_pickup.mp3"), 
                    soundId, 
                    cooldownMs: 200);
                break;
            case SoundEffectType.Pain:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_pain.mp3"), 
                    soundId, 
                    cooldownMs: 400);
                break;
            case SoundEffectType.PlayerDeath:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_death.mp3"), 
                    soundId, 
                    cooldownMs: 1000);
                break;
            case SoundEffectType.Shotgun:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "gs_shotgun.mp3"), 
                    soundId, 
                    cooldownMs: 50);
                break;
            case SoundEffectType.LevelComplete:
                _audioEngine.Play(
                    Path.Combine(DoomGameManager.GameSettings.AudioAssetsPath, "mark_lor-war_of_sirens.mp3"), 
                    soundId, 
                    stopIfPlaying: true);
                break;
        }
    }
}