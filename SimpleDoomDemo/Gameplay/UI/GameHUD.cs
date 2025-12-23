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
        BackgroundColor = AnsiColor.Black;
        ForegroundColor = AnsiColor.White;
        HasBorder = false;
        Size = new Dimension2D(width, height);
    }

    public void UpdateHud(Point2D screenPosition)
    {
        // Update position directly (for UI in screen space)
        RelativePosition = screenPosition;
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // Use the pre-calculated screen position from the render pipeline
        // Since this is in UiViewport (no camera), screenPoint = WorldPosition
        string hudText = $"HP: {_player.Health}/{_player.MaxHealth}  " +
                         $"Ammo: {_player.Ammo}/{_player.MaxAmmo}  " +
                         $"BFG: {_player.BfgCells}/{_player.MaxBfgCells}  " +
                         $"XP: {_player.CombatPoints}  ";

        var style = new RenderStyle(AnsiColor.Black, AnsiColor.White, FontStyle.Regular);
        renderer.DrawText(screenPoint.X, screenPoint.Y, hudText, style);
    }
}
