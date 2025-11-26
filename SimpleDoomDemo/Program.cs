using System;
using System.IO;
using ConsoleGameEngine.Engine;
using SimpleDoomDemo.Gameplay;

namespace SimpleDoomDemo;

class Program
{
    static void Main(string[] args)
    {
        // Create the ConsoleEngine
        ConsoleEngine engine = new ConsoleEngine();
        engine.TargetUpdatesPerSecond = 40;  // Game logic updates at 60 FPS
        engine.TargetRenderFPS = 60;         // Rendering at 40 FPS
        engine.Initialize();

        // Create and configure the game scene
        var gameScene = new DoomGameScene();

        // Load the map (you can pass a map file path as argument)
        string mapPath = Path.Combine("assets", "maps", "pmp_arena.txt");

        if (File.Exists(mapPath))
        {
            gameScene.LoadMapFromPlainText(mapPath);
            Console.WriteLine($"Loaded map: {mapPath}");
        }
        else
        {
            engine.Stop();
            return;
        }

        // Set the game scene as the initial scene
        engine.SetInitialScene(gameScene);

        // Start the engine (this runs the game loop)
        engine.OnStart();

        // Wait for engine to stop
        while (engine.IsRunning)
        {
            System.Threading.Thread.Sleep(100);
        }

        // Cleanup
        engine.Dispose();
    }
}