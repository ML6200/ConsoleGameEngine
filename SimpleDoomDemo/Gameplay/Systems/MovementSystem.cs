using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Handles movement logic for all entities (player and demons).
/// Checks collision with walls and other entities using FillingRatio.
/// </summary>
public class MovementSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly Random _random = new Random();

    public MovementSystem(DoomGameScene game)
    {
        _game = game;
    }

    public void Update(long deltaTime)
    {
        // Demon movement is handled here
        foreach (Demon demon in _game.Demons)
        {
            if (demon.State == DemonState.Move)
            {
                UpdateDemonMovement(demon, deltaTime);
            }
        }
    }

    /// <summary>
    /// Attempts to move the player to the specified position.
    /// Returns true if movement was successful.
    /// </summary>
    public bool MovePlayer(Position2D targetPosition)
    {
        if (!IsPointWithinBounds(targetPosition))
            return false;

        double totalFillingRatio = GetTotalFillingRatio(targetPosition) + _game.PlayerFillingRatio;

        if (totalFillingRatio < 1.0)
        {
            _game.Player.RelativePosition = targetPosition;
            return true;
        }

        return false;
    }

    private void UpdateDemonMovement(Demon demon, long deltaTime)
    {
        double pMove = (demon.Speed / 100.0) * (deltaTime / 1000.0);
        double randomValue = _random.NextDouble();

        if (randomValue < pMove)
        {
            // Random movement
            int x = demon.AbsolutePosition.X + _random.Next(-1, 2);
            int y = demon.AbsolutePosition.Y + _random.Next(-1, 2);
            Position2D targetPosition = new Position2D(x, y);

            if (IsPointWithinBounds(targetPosition))
            {
                double totalFillingRatio = GetTotalFillingRatio(targetPosition) + demon.FillingRatio;

                if (totalFillingRatio < 1.0)
                {
                    demon.RelativePosition = targetPosition;
                }
            }
        }
    }

    private double GetTotalFillingRatio(Position2D position)
    {
        double sum = 0;

        // Check items
        foreach (var item in _game.Items)
        {
            if (Position2D.Distance(position, item.AbsolutePosition) <= 0)
            {
                sum += item.FillingRatio;
            }
        }

        // Check demons
        foreach (var demon in _game.Demons)
        {
            if (Position2D.Distance(position, demon.AbsolutePosition) <= 0)
            {
                sum += demon.FillingRatio;
            }
        }

        return sum;
    }

    private bool IsPointWithinBounds(Position2D position)
    {
        return position.X < Console.WindowWidth
               && position.X >= 0
               && position.Y < Console.WindowHeight - 1
               && position.Y >= 0;
    }
}
