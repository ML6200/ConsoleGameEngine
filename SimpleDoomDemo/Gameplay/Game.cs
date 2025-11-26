using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomEngine;

public class Game
{
    // =============================FIELDS_PRIVATE==============================
    public Player _player;
    private bool _interrupted;
    private bool _exited;
    private double _fillingRatio;
    private ConsoleRenderer _consoleRenderer;
    private GameLogic _gameLogic;
    private Stopwatch _stopwatchLogic = new Stopwatch();
    private Stopwatch _stopwatchRenderer = new Stopwatch();

    
    // =============================FIELDS_PUBLIC==============================
    // ==========================PUBLIC_SETTERS&GETTERS=========================
    public Player Player => _player;
    public bool Interrupted
    {
        get => _interrupted;
        set => _interrupted = value;
    }
    public bool Exited
    {
        get => _exited;
        set => _exited = value;
    }
    
    public List<GameItem> _items;
    public List<Demon> _demons;
    
    public List<Demon> Demons
    {
        get {return _demons;}
    }

    public double FillingRatio
    {
        get { return _fillingRatio;}
    }

    public List<GameItem> Items
    {
        get { return _items; }
    }

    // =============================METHODS==============================
    public Game()
    {
        _fillingRatio = 0.4;
        _items = new List<GameItem>();
        _demons = new List<Demon>();
        _consoleRenderer = new ConsoleRenderer(this);
        _gameLogic = new GameLogic(this);
        _player = new Player(0, 0);
    }

    private void UserMoveBy(int x, int y)
    {
        Position comp = _player.Position.Add(new Position(x, y));
        if (ConsoleRenderer.IsPointWithinBounds(comp))
            _gameLogic.Move(_player, comp);
    }

    private void UserAction()
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo pressedKey = Console.ReadKey(true);

            switch (pressedKey.Key)
            {
                case ConsoleKey.E: _interrupted = true; break;

                case ConsoleKey.LeftArrow: UserMoveBy(-1, 0);break;
                case ConsoleKey.RightArrow: UserMoveBy(1, 0); break;
                
                case ConsoleKey.UpArrow: UserMoveBy(0, -1); break;
                case ConsoleKey.DownArrow: UserMoveBy(0, 1); break;
                //case ConsoleKey.B: _gameLogic.PlayerBFGAttackLogic(); break;
                case ConsoleKey.S: _gameLogic.PlayerBFGAttackLogic();break;

                case ConsoleKey.D:
                {
                    for (int i = 0; i < _items.Count; i++)
                    {
                        _gameLogic.PlayerDirectInteractionLogic();
                    }
                } break;
                
                case ConsoleKey.A: _gameLogic.PlayerAttackLogic(); break;
            }
        }
        _gameLogic.PlayerIndirectInteractionLogic();
    }
    
    public void Run()
    {
        Console.CursorVisible = false;
        Console.SetCursorPosition(0, 0);
        
        _stopwatchLogic.Start();
        _stopwatchRenderer.Start();
        
        Console.CancelKeyPress += (sender, e) =>
        {
            AudioPlayer.StopMusic();
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            AudioPlayer.StopMusic();
        };

        AudioPlayer.PlayMusic(Path.Combine("assets","sounds","doom_music.mp3"));
        
        while (!_interrupted && _player.Alive && !_exited)
        {
            UserAction();

            if (_stopwatchLogic.ElapsedMilliseconds > 500)
            {
                long deltaTime = _stopwatchLogic.ElapsedMilliseconds;
                _gameLogic.UpdateGameState(deltaTime);
                _stopwatchLogic.Restart();
            }

            if (_stopwatchRenderer.ElapsedMilliseconds > 25)
            {
                _consoleRenderer.RenderGame();
                _stopwatchRenderer.Restart();
            }
            Thread.Sleep(25); // approx. 40fps
        }

        if (!_player.Alive)
        {
            AudioPlayer.StopMusic();
            PlaySoundEffect(SoundEffectType.PlayerDeath);
            
            _consoleRenderer.RenderFullScreenText("YOU DIED!", ConsoleColor.Black, ConsoleColor.Red);
        }
        if (_interrupted)
        {
            AudioPlayer.StopMusic();
            _consoleRenderer.RenderFullScreenText("EXITED", ConsoleColor.Black, ConsoleColor.Green);
        }
        if (_exited)
        {
            AudioPlayer.StopMusic();
            PlaySoundEffect(SoundEffectType.LevelExit);
            
            _consoleRenderer.RenderFullScreenText("LEVEL COMPLETE!", ConsoleColor.Black, ConsoleColor.Green);
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
                        //ITEMS
                        case 'A':
                            _items.Add(new GameItem(j, i - 1, ItemType.AMMO));
                            break;
                        case 'B':
                            _items.Add(new GameItem(j, i - 1, ItemType.BFGCELL));
                            break;
                        case 'D':
                            _items.Add(new GameItem(j, i - 1, ItemType.DOOR));
                            break;
                        case 'E':
                            _items.Add(new GameItem(j, i - 1, ItemType.LEVELEXIT));
                            break;
                        case 'M':
                            _items.Add(new GameItem(j, i - 1, ItemType.MEDKIT));
                            break;
                        case 'T':
                            _items.Add(new GameItem(j, i - 1, ItemType.TOXICWASTE));
                            break;
                        case 'W':
                            _items.Add(new GameItem(j, i - 1, ItemType.WALL));
                            break;
                        //DEMONS
                        case 'z':
                            _demons.Add(new Demon(j, i - 1, DemonType.Zombieman));
                            break;
                        case 'i':
                            _demons.Add(new Demon(j, i - 1, DemonType.Imp));
                            break;
                        case 'm':
                            _demons.Add(new Demon(j, i - 1, DemonType.Mancubus));
                            break;
                        //PLAYER
                        case 'p':
                            _player.Position = new Position(j, i - 1);
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