using System;
using ConsoleGameEngine.Engine;

namespace SimpleDoomDemo;

class Program
{
    static void Main(string[] args)
    {
        ConsoleEngine engine = new ConsoleEngine();
        engine.TargetUpdatesPerSecond = 24;
        engine.TargetRenderFPS = 60;
        engine.Initialize();
        engine.OnStart();

        while (engine.IsRunning)
        {
            
        }
        
        engine.Dispose();
    }
}