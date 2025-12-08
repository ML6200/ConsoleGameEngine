using System;
using System.IO;
using System.Text;
using System.Threading;
using ConsoleGameEngine.Engine;
using SimpleDoomDemo.Gameplay.Scenes;

namespace SimpleDoomDemo;

class Program
{
    static void Main(string[] args)
    {
        string DEFAULT_MAP_PATH = Path.Combine("assets", "maps", "pmp_arena.txt");
        
        // Create the ConsoleEngine
        Console.OutputEncoding = Encoding.UTF8;
        ConsoleEngine engine = new ConsoleEngine();
        engine.TargetUpdatesPerSecond = 40;  // Game logic updates at 40 FPS
        engine.TargetRenderFPS = 100;       // Rendering at 60 FPS
        engine.Initialize();

        // Determine map path
        string mapPath = args.Length > 0
            ? args[0]
            : DEFAULT_MAP_PATH;

        // Start with main menu
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