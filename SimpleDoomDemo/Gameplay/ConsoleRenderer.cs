using System;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomEngine.Engine;

public class ConsoleRenderer
{
    private Game _game;

    public ConsoleRenderer(Game game)
    {
        _game = game;
    }
    
    public static bool IsPointWithinBounds(Position position)
    {
        return position.X < Console.WindowWidth
               && position.X >= 0
               && position.Y < Console.WindowHeight
               && position.Y >= 0;
    }
    
    private void RenderSingleSprite(Position position, ConsoleSprite sprite)
    {
        if (IsPointWithinBounds(position))
        {
            Console.BackgroundColor = sprite.backgroundColor;
            Console.ForegroundColor = sprite.foregroundColor;
            Console.SetCursorPosition(position.X, position.Y);
            Console.Write(sprite.glypth);
        }
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
        Console.ResetColor();
        Console.Clear();

        foreach (GameItem item in _game.Items)
        {
            if (item.Available)
            {
                double distance = Position.Distance(item.Position, _game.Player.Position);
                
                if (distance <= _game.Player.SightRange)
                {
                    RenderSingleSprite(item.Position, item.Sprite);
                }
            }
        }
        
        foreach (Demon demon in _game.Demons)
        {
            double distance = Position.Distance(demon.Position, _game.Player.Position);

            if (distance <= _game.Player.SightRange)
            {
                RenderSingleSprite(demon.Position, demon.Sprite);
            }
        }

        RenderSingleSprite(_game._player.Position, _game._player.Sprite);
        RenderHud();
    }
}