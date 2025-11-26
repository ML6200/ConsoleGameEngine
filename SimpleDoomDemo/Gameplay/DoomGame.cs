using System;
using System.Collections.Generic;
using System.IO;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Systems;
using SimpleDoomEngine;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay;

/// <summary>
/// Main game class using Entity-Component-System architecture.
/// Manages entities (Player, Demons, Items) and delegates logic to Systems.
/// </summary>
public class DoomGame
{
    // ============================= ENTITIES ==============================
    public Player Player { get; private set; }
    public List<Demon> Demons { get; private set; }
    public List<GameItem> Items { get; private set; }

    // ============================= SYSTEMS ==============================
    private MovementSystem _movementSystem;
    private CombatSystem _combatSystem;
    private InteractionSystem _interactionSystem;
    private AISystem _aiSystem;
    private ConsoleEngine _gameEngine;

    // ============================= RENDERING & AUDIO ==============================
    private ConsoleRenderer _renderer;

    // ============================= GAME STATE ==============================
    public bool Interrupted { get; set; }
    public bool Exited { get; set; }
    public double PlayerFillingRatio { get; private set; } = 0.4;
    
    public DoomGame()
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

        // Initialize renderer
        _renderer = new ConsoleRenderer(this);
    }

    public void Run()
    {
        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);

        // Setup cleanup handlers
        Console.CancelKeyPress += (sender, e) => AudioPlayer.StopMusic();
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => AudioPlayer.StopMusic();

        AudioPlayer.PlayMusic(Path.Combine("assets", "sounds", "doom_music.mp3"));

        // Main game loop
        

        // Game over handling
        HandleGameOver();
    }

    private void UpdateGameLogic(long deltaTime)
    {
        // Update all systems
        _aiSystem.Update(deltaTime);
        _movementSystem.Update(deltaTime);
        _interactionSystem.Update(deltaTime);

        // Cleanup dead entities
        CleanupEntities();
    }

    private void UpdateAnimations(double deltaTime)
    {
        // Update player animations
        Player.Update(deltaTime);

        // Update demon animations
        foreach (Demon demon in Demons)
        {
            demon.Update(deltaTime);
        }

        // Update item animations
        foreach (GameItem item in Items)
        {
            item.Update(deltaTime);
        }
    }

    private void ProcessUserInput()
    {
        if (!Console.KeyAvailable)
            return;
        

        switch (pressedKey.Key)
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
        Position2D targetPosition = Player.AbsolutePosition + new Position2D(x, y);
        _movementSystem.MovePlayer(targetPosition);
    }

    private void CleanupEntities()
    {
        // Remove unavailable items
        Items.RemoveAll(item => !item.Available);

        // Remove dead demons
        Demons.RemoveAll(demon => !demon.Alive);
    }

    private void HandleGameOver()
    {
        AudioPlayer.StopMusic();

        if (!Player.Alive)
        {
            PlaySoundEffect(SoundEffectType.PlayerDeath);
            _renderer.RenderFullScreenText("YOU DIED!", ConsoleColor.Black, ConsoleColor.Red);
        }
        else if (Interrupted)
        {
            _renderer.RenderFullScreenText("EXITED", ConsoleColor.Black, ConsoleColor.Green);
        }
        else if (Exited)
        {
            PlaySoundEffect(SoundEffectType.LevelExit);
            _renderer.RenderFullScreenText("LEVEL COMPLETE!", ConsoleColor.Black, ConsoleColor.Green);
        }
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
                            Player.RelativePosition = new Position2D(j, i - 1);
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