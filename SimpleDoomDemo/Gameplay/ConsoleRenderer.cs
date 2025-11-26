using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomEngine.Engine;

public class ConsoleRenderer
{
    private DoomGame _game;
    private ConsoleRenderer2D _renderer2D;

    public ConsoleRenderer(DoomGame game)
    {
        _game = game;
        _renderer2D = new ConsoleRenderer2D(Console.WindowWidth, Console.WindowHeight - 1);
        _renderer2D.InitRenderer();
    }

    public static bool IsPointWithinBounds(Position2D position)
    {
        return position.X < Console.WindowWidth
               && position.X >= 0
               && position.Y < Console.WindowHeight - 1
               && position.Y >= 0;
    }

    public void RenderFullScreenText(string text, ConsoleColor bg = ConsoleColor.Black, ConsoleColor fg = ConsoleColor.White)
    {
        Console.BackgroundColor = bg;
        Console.ForegroundColor = fg;
        
        Console.Clear();
        Console.SetCursorPosition(Console.WindowWidth / 2, Console.WindowHeight / 2);
        Console.WriteLine(text);
        Console.SetCursorPosition(0, 0);
        Console.ReadLine();
    }

    private void RenderHud()
    {
        int hudY = Console.WindowHeight - 1;
        Console.SetCursorPosition(0, hudY);
        
        Console.BackgroundColor = ConsoleColor.DarkGray;
        Console.ForegroundColor = ConsoleColor.White;
        
        string hud = $" HP: {_game.Player.Health}/{_game.Player.MaxHealth} | " +
                     $"Ammo: {_game.Player.Ammo}/{_game.Player.MaxAmmo} | " +
                     $"Cells: {_game.Player.BFGCells}/{_game.Player.MaxBFGCells} | " +
                     $"XP: {_game.Player.CombatPoints} ";
        
        hud = hud.PadRight(Console.WindowWidth);
    
        Console.Write(hud);
        Console.ResetColor();
    }
    
    public void RenderGame()
    {
        _renderer2D.Clear();

        foreach (GameItem item in _game.Items)
        {
            if (item.Available)
            {
                double distance = Position2D.Distance(item.AbsolutePosition, _game.Player.AbsolutePosition);

                if (distance <= _game.Player.SightRange)
                {
                    item.Render(_renderer2D);
                }
            }
        }

        foreach (Demon demon in _game.Demons)
        {
            double distance = Position2D.Distance(demon.AbsolutePosition, _game.Player.AbsolutePosition);

            if (distance <= _game.Player.SightRange)
            {
                demon.Render(_renderer2D);
            }
        }

        _game.Player.Render(_renderer2D);
        _renderer2D.Render();
        RenderHud();
    }
}