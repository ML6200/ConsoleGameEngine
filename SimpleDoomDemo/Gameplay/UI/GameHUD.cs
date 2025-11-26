using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.UI;

/// <summary>
/// Heads-Up Display showing player stats in a single line.
/// </summary>
public class GameHud : ConsoleGraphicsPanel
{
    private readonly Player _player;

    public GameHud(Player player, int width, int height)
    {
        _player = player;

        // Configure panel
        BackgroundColor = ConsoleColor.Black;
        ForegroundColor = ConsoleColor.White;
        HasBorder = false;
        Size = new Dimension2D(width, height);
    }

    public void UpdateHUD(Position2D absolutePosition)
    {
        SetAbsolutePosition(absolutePosition);
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        // Render all stats in one line
        int x = AbsolutePosition.X;
        int y = AbsolutePosition.Y;

        string hudText = $"HP: {_player.Health}/{_player.MaxHealth}  Ammo: {_player.Ammo}/{_player.MaxAmmo}  BFG: {_player.BFGCells}/{_player.MaxBFGCells}  XP: {_player.CombatPoints}";

        renderer.DrawText(x, y, hudText, ConsoleColor.White, ConsoleColor.Black);
    }
}
