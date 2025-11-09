using System;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine;

class Tester
{
    static void RenderTester()
    {
        var renderer = new ConsoleRenderer2D(Console.WindowWidth, Console.WindowHeight);
        while (true)
        {
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawBox(2, 1, 30, 6);
            renderer.DrawText(4, 3, "Hello world.");
            renderer.Render();
            Thread.Sleep(500);
            
            renderer.DrawBox(2, 1, 30, 6);
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawText(4, 3, "Hello world..");
            renderer.Render();
            Thread.Sleep(500);
            
            renderer.DrawBox(2, 1, 30, 6);
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawText(4, 3, "Hello world...");
            renderer.Render();
            Thread.Sleep(500);
        }
    }
    
    static void Main(string[] args)
    {
        
        InputManager _inputManager = new InputManager();
        ConsoleRenderer2D _renderer = new ConsoleRenderer2D(Console.WindowWidth, Console.WindowHeight);
        ConsoleRenderManager renderManager = new ConsoleRenderManager(_renderer);
        
        ConsolePanel _rootPanel = new ConsolePanel 
        { 
            Position = new Position2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            HasBorder = false
        };
        var title = new ConsoleLabel
        {
            Position = new Position2D(30, 2),
            Text = "SIMPLE DOOM ENGINE",
            ForegroundColor = ConsoleColor.Red
        };
        
        var panel = new ConsolePanel
        {
            Position = new Position2D(10, 5),
            Size = new Dimension2D(60, 15),
            BackgroundColor = ConsoleColor.DarkBlue,
            BorderColor = ConsoleColor.Cyan
        };
        
        var info = new ConsoleLabel
        {
            Position = new Position2D(12, 6),
            Text = "Press Enter to exit",
            ForegroundColor = ConsoleColor.White
        };
        bool _isRunning = true;

        var label = new ConsoleLabel
        {
            Position = new Position2D(0, 0),
            Text = "Hello World!",
            ForegroundColor = ConsoleColor.Green,
            Visible = false
        };
        
        _rootPanel.AddChild(title);
        _rootPanel.AddChild(panel);
        panel.AddChild(info);
        panel.AddChild(label);
        
        _inputManager.OnEnterPressed += (s, e) => _isRunning = false;
        _inputManager.OnKeyPressed += (s, e) =>
        {
            if (e.Key == ConsoleKey.E)
            {
                label.Visible = !label.Visible;
            } else if (e.Key == ConsoleKey.A)
            {
                title.Visible = !title.Visible;
            }
        };
        
        var lastTime = DateTime.Now;
    
        while (_isRunning)
        {
            //var currentTime = DateTime.Now;
            //var deltaTime = (currentTime - lastTime).TotalSeconds;
            //lastTime = currentTime;
        
            // Update
            _rootPanel.Update();
        
            // Render
            _renderer.Clear();
            _rootPanel.Render(_renderer);
            renderManager.Start();
        
            // Cap framerate (~60 FPS)
            Thread.Sleep(16);
        }
    }
}