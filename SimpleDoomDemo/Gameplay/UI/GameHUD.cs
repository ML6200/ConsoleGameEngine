using System;
using ConsoleGameEngine.Engine;
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
    private readonly ConsoleEngine _engine;

    public GameHud(ConsoleEngine engine, Player player, int width, int height)
    {
        _engine = engine;
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
        int x = WordPosition.X;
        int y = WordPosition.Y;

        string hudText = $"HP: {_player.Health}/{_player.MaxHealth}  " +
                         $"Ammo: {_player.Ammo}/{_player.MaxAmmo}  " +
                         $"BFG: {_player.BFGCells}/{_player.MaxBFGCells}  " +
                         $"XP: {_player.CombatPoints}  " +
                         $"Framerate: {Math.Round(_engine.RenderManager.CurrentFps, 2)} FPS" +
                         $"Update rate: {Math.Round(_engine.CurrentUpdateRate, 2)} UPS";

        renderer.DrawText(x, y, hudText, ConsoleColor.White, ConsoleColor.Black);
    }
}
