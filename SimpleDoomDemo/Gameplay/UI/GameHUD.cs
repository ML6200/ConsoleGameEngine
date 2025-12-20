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
public class GameHud : UiPanel
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

    public void UpdateHud(Point2D worldPosition)
    {
        WorldPosition = worldPosition;
    }

    protected override void Draw(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // HUD should always be visible (don't use camera transformation for UI)
        // Render directly at world position (which should be screen position for UI)
        int x = WorldPosition.X;
        int y = WorldPosition.Y;

        string hudText = $"HP: {_player.Health}/{_player.MaxHealth}  " +
                         $"Ammo: {_player.Ammo}/{_player.MaxAmmo}  " +
                         $"BFG: {_player.BfgCells}/{_player.MaxBfgCells}  " +
                         $"XP: {_player.CombatPoints}  ";
                         
        renderer.DrawText(x, y, hudText, ConsoleColor.White, ConsoleColor.Black);
    }
}
