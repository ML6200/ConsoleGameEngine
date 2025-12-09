using System;
using System.Text;
using System.Threading;
using ConsoleGameEngine.Engine;
using SimpleDoomDemo.Gameplay;
using SimpleDoomDemo.Gameplay.Scenes;
using NLog;

namespace SimpleDoomDemo;

class Program
{
    static void ConvertMap(string path)
    {
        Mapper mapper  = new Mapper();
        mapper.LoadFromLegacy(path);
        mapper.SaveMap("arena.dcmf");
    }
    static void Main(string[] args)
    {
        string DEFAULT_MAP_PATH = "arena.dcmf";//Path.Combine("assets", "maps", "pmp_arena.txt");

        // Initialize NLog configuration
        LogManager.Setup().LoadConfigurationFromFile("nlog.xml");

        Console.OutputEncoding = Encoding.UTF8;
        ConsoleEngine engine = new ConsoleEngine();
        engine.TargetUpdatesPerSecond = 40;  // Game logic updates at 40 FPS
        engine.TargetRenderFPS = 100;       // Rendering at 100 FPS
        engine.Initialize();

        // Determine map path
        string mapPath = args.Length > 0
            ? args[0]
            : DEFAULT_MAP_PATH;

        // Start with main menuX
        var menuScene = new MainMenuScene(mapPath);
        engine.SetInitialScene(menuScene);

        // Start the engine (this runs the game loop)
        engine.OnStart();

        // Wait for engine to stop
        while (engine.IsRunning)
        {
            Thread.Sleep(100);
        }

        // Cleanup
        engine.Dispose();
    }
}