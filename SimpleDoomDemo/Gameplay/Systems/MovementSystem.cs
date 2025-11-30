using System;
using System.Collections.Generic;
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
    
    private Dictionary<int, List<object>> _spatialGrid = new();
    private const int GRID_SIZE = 5;  // 5x5 cell buckets

    private int GetGridKey(Position2D pos)
    {
        int gridX = pos.X / GRID_SIZE;
        int gridY = pos.Y / GRID_SIZE;
        return (gridY << 16) | gridX;  // Pack into int
    }

    public void Update(long deltaTime)
    {
        // Build spatial grid ONCE per update
        _spatialGrid.Clear();

        foreach (var item in _game.Items)
        {
            int key = GetGridKey(item.AbsolutePosition);
            if (!_spatialGrid.ContainsKey(key))
                _spatialGrid[key] = new List<object>();
            _spatialGrid[key].Add(item);
        }

        foreach (var demon in _game.Demons)
        {
            int key = GetGridKey(demon.AbsolutePosition);
            if (!_spatialGrid.ContainsKey(key))
                _spatialGrid[key] = new List<object>();
            _spatialGrid[key].Add(demon);
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

    private double GetTotalFillingRatio(Position2D position)
    {
        double sum = 0;
        int key = GetGridKey(position);

        // Only check nearby objects!
        if (_spatialGrid.TryGetValue(key, out var nearbyObjects))
        {
            foreach (var obj in nearbyObjects)
            {
                Position2D objPos = obj is GameItem item
                    ? item.AbsolutePosition
                    : ((Demon)obj).AbsolutePosition;

                if (Position2D.Distance(position, objPos) <= 0)
                {
                    sum += obj is GameItem i ? i.FillingRatio : ((Demon)obj).FillingRatio;
                }
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
