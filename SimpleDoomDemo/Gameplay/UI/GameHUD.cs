using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.UI;

/// <summary>
/// Heads-Up Display showing player stats with progress bars.
/// </summary>
public class GameHUD : ConsoleGraphicsPanel
{
    private readonly Player _player;
    private ConsoleProgressBar _healthBar;
    private ConsoleProgressBar _ammoBar;
    private ConsoleProgressBar _bfgBar;

    public GameHUD(Player player, int width, int height)
    {
        _player = player;

        // Configure panel
        BackgroundColor = ConsoleColor.DarkGray;
        ForegroundColor = ConsoleColor.White;
        HasBorder = true;
        BorderColor = ConsoleColor.Gray;
        Size = new Dimension2D(width, height);

        // Create health bar
        _healthBar = new ConsoleProgressBar
        {
            RelativePosition = new Position2D(2, 1),
            Size = new Dimension2D(width - 4, 1),
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColor = ConsoleColor.Green
        };

        // Create ammo bar
        _ammoBar = new ConsoleProgressBar
        {
            RelativePosition = new Position2D(2, 3),
            Size = new Dimension2D(width - 4, 1),
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColor = ConsoleColor.Yellow
        };

        // Create BFG cells bar
        _bfgBar = new ConsoleProgressBar
        {
            RelativePosition = new Position2D(2, 5),
            Size = new Dimension2D(width - 4, 1),
            BackgroundColor = ConsoleColor.DarkGray,
            ForegroundColor = ConsoleColor.Cyan
        };

        // Add bars as children
        AddChild(_healthBar);
        AddChild(_ammoBar);
        AddChild(_bfgBar);
    }

    public void UpdateHUD()
    {
        // Update health bar
        float healthRatio = (float)_player.Health / _player.MaxHealth;
        _healthBar.SetProgress(healthRatio, animationDuration: 0.2);

        // Update ammo bar
        float ammoRatio = (float)_player.Ammo / _player.MaxAmmo;
        _ammoBar.SetProgress(ammoRatio, animationDuration: 0.1);

        // Update BFG cells bar
        float bfgRatio = (float)_player.BFGCells / _player.MaxBFGCells;
        _bfgBar.SetProgress(bfgRatio, animationDuration: 0.1);
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        // Render panel background
        base.Render(renderer);

        // Render labels
        int labelX = AbsolutePosition.X + 2;

        renderer.DrawText(labelX, AbsolutePosition.Y + 1,
            $"HP: {_player.Health}/{_player.MaxHealth}",
            ConsoleColor.White, ConsoleColor.DarkGray);

        renderer.DrawText(labelX, AbsolutePosition.Y+3,
            $"Ammo: {_player.Ammo}/{_player.MaxAmmo}",
            ConsoleColor.Yellow, ConsoleColor.DarkGray);

        renderer.DrawText(labelX, AbsolutePosition.Y + 5,
            $"BFG: {_player.BFGCells}/{_player.MaxBFGCells}",
            ConsoleColor.Cyan, ConsoleColor.DarkGray);

        renderer.DrawText(labelX, AbsolutePosition.Y + 7,
            $"XP: {_player.CombatPoints}",
            ConsoleColor.Magenta, ConsoleColor.DarkGray);
    }
}
