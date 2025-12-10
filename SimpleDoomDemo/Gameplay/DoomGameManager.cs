using System;
using System.Text;
using System.Threading;
using ConsoleGameEngine.Engine;
using SimpleDoomDemo.Gameplay.Scenes;

namespace SimpleDoomDemo.Gameplay;

public class DoomGameManager
{
    public static string DefaultMapPath { get; set; }
    public static bool IsTipShown { get; set; }
    private ConsoleEngine _engine;
    private MainMenuScene _menuScene;

    public DoomGameManager(string defaultMapPath)
    {
        IsTipShown = false;
        DefaultMapPath = defaultMapPath;
        Console.OutputEncoding = Encoding.UTF8;
        _engine= new ConsoleEngine();
        _engine.TargetUpdatesPerSecond = 40;  // Game logic updates at 40 FPS
        _engine.TargetRenderFPS = 100;       // Rendering at 100 FPS
        _engine.Initialize();
        _menuScene = new MainMenuScene(DefaultMapPath);
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
}