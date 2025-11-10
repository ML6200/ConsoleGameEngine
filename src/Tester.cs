using System;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

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
        
        ConsoleGraphicsPanel rootGraphicsPanel = new ConsoleGraphicsPanel 
        { 
            AbsolutePosition = new Position2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            HasBorder = false
        };
        ConsoleWindowComponent consoleWindowComponent = new ConsoleWindowComponent(rootGraphicsPanel);
        
        
        var title = new ConsoleGraphicsLabel
        {
            AbsolutePosition = new Position2D(30, 2),
            Text = "SIMPLE DOOM ENGINE",
            ForegroundColor = ConsoleColor.Red
        };
        
        var panel = new ConsoleGraphicsPanel
        {
            AbsolutePosition = new Position2D(10, 5),
            Size = new Dimension2D(60, 15),
            BackgroundColor = ConsoleColor.DarkBlue,
            BorderColor = ConsoleColor.Cyan
        };
        var text = new ConsoleGraphicsLabel
        {
            AbsolutePosition = new Position2D(22, 12),
            Text = "Sprite",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkGray,
            Visible = true
        };
        
        var panel2 = new ConsoleGraphicsPanel
        {
            AbsolutePosition = new Position2D(20, 10),
            Size = new Dimension2D(9, 5),
            BackgroundColor = ConsoleColor.DarkGray,
            BorderColor = ConsoleColor.Black
        };
        
        var info = new ConsoleGraphicsLabel
        {
            AbsolutePosition = new Position2D(12, 6),
            Text = "Press Enter to exit",
            ForegroundColor = ConsoleColor.White
        };
        bool _isRunning = true;

        var label = new ConsoleGraphicsLabel
        {
            AbsolutePosition = new Position2D(0, 0),
            Text = "Hello World!",
            ForegroundColor = ConsoleColor.Green,
            Visible = false
        };
        panel2.AddChild(text);
        panel.AddChild(panel2);
        
        rootGraphicsPanel.AddChild(title);
        rootGraphicsPanel.AddChild(panel);
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
                panel2.Visible = !panel2.Visible;
            } else if (e.Key == ConsoleKey.RightArrow)
            {
                panel2.RelativePosition.X += 1;
                text.RelativePosition.X += 1;
                
            } else if (e.Key == ConsoleKey.LeftArrow)
            {
                panel2.RelativePosition.X -= 1;
                text.RelativePosition.X -= 1;
            }
            else if (e.Key == ConsoleKey.UpArrow)
            {
                panel2.RelativePosition.Y -= 1;
                text.RelativePosition.Y -= 1;
            }
            else if (e.Key == ConsoleKey.DownArrow)
            {
                panel2.RelativePosition.Y += 1;
                text.RelativePosition.Y += 1;
            }
        };
        
        var lastTime = DateTime.Now;
        
        //
    
        while (_isRunning)
        {
            //var currentTime = DateTime.Now;
            //var deltaTime = (currentTime - lastTime).TotalSeconds;
            //lastTime = currentTime;
        
            // Update
            rootGraphicsPanel.Update();
        
            // Render
            _renderer.Clear();
            //rootGraphicsPanel.Render(_renderer);
            renderManager.RenderAll();
            _renderer.Render();
            
            consoleWindowComponent.Render(_renderer);
        
            // Cap framerate (~60 FPS)
            Thread.Sleep(16);
        }
        
        renderManager.Dispose();
        rootGraphicsPanel.Dispose();
        
        //renderManager.Dispose();
    }
}