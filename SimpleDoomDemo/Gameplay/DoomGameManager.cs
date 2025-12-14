using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using ConsoleGameEngine.Engine;
using NLog;
using SimpleDoomDemo.Gameplay.Scenes;

namespace SimpleDoomDemo.Gameplay;

public class DoomGameManager
{
    public static GameSettings GameSettings { get; private set; } = SettingsManager.Load();
    public static bool IsTipShown { get; set; }
    private ConsoleEngine _engine;
    private MainMenuScene _menuScene;
    
    private static Logger _logger = LogManager.GetCurrentClassLogger();

    public DoomGameManager()
    {
        IsTipShown = false;
        Console.OutputEncoding = Encoding.UTF8;
        _engine= new ConsoleEngine();
        _engine.TargetUpdatesPerSecond = 40;  // Game logic updates at 40 FPS
        _engine.TargetRenderFps = 100;       // Rendering at 100 FPS
        _engine.Initialize();
        _menuScene = new MainMenuScene();
    }

    public void StartGame()
    {
        _engine.SetInitialScene(_menuScene);
        _engine.OnStart();
        
        while (_engine.IsRunning)
        {
            Thread.Sleep(100);
        }
        
        _engine.Dispose();
    }

    public static void SaveSettings()
    {
        SettingsManager.Save(GameSettings);
    }

    public static void ReloadSettings()
    {
        GameSettings = SettingsManager.Load();
    }

    private static class SettingsManager
    {
        private static readonly string FilePath = "settings.json";
        private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

        public static GameSettings Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    File.WriteAllText(FilePath, JsonSerializer.Serialize(DefaultGameSettings, Options));
                    return DefaultGameSettings;
                }

                string json = File.ReadAllText(FilePath);
                return JsonSerializer.Deserialize<GameSettings>(json) ?? DefaultGameSettings;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                throw;
            }
        }

        public static void Save(GameSettings settings)
        {
            File.WriteAllText(FilePath, JsonSerializer.Serialize(settings, Options));
        }

        private static readonly GameSettings DefaultGameSettings = new()
        {
            DefaultMap = "map.dcmf",
            AssetsPath = "assets",
        };
    }
}