using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Systems;

public class MovementSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly Random _random = new Random();

    public MovementSystem(DoomGameScene game)
    {
        _game = game;
    }
    
    private Dictionary<int, List<object>> _spatialGrid = new();
    private const int GRID_SIZE = 5;

    private int GetGridKey(Point2D pos)
    {
        int gridX = pos.X / GRID_SIZE;
        int gridY = pos.Y / GRID_SIZE;
        return (gridY << 16) | gridX;
    }

    public void Update(double deltaTime)
    {
        _spatialGrid.Clear();

        foreach (var item in _game.Items)
        {
            int key = GetGridKey(item.WorldPosition);
            if (!_spatialGrid.ContainsKey(key))
                _spatialGrid[key] = new List<object>();
            _spatialGrid[key].Add(item);
        }

        foreach (var demon in _game.Demons)
        {
            int key = GetGridKey(demon.WorldPosition);
            if (!_spatialGrid.ContainsKey(key))
                _spatialGrid[key] = new List<object>();
            _spatialGrid[key].Add(demon);
        }
        
        foreach (var demon in _game.Demons)
        {
            if (demon.State == DemonState.Move)
            {
                MoveDemonTowardsPlayer(demon);
            }
        }
    }
    
    private void MoveDemonTowardsPlayer(Demon demon)
    {
        Point2D demonPos = demon.WorldPosition;
        Point2D playerPos = _game.Player.WorldPosition;

        // Calculate direction to player
        int dx = playerPos.X - demonPos.X;
        int dy = playerPos.Y - demonPos.Y;

        // Normalize to unit steps (-1, 0, or 1)
        int stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
        int stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);

        // Try to move directly towards player
        Point2D targetPos = new Point2D(demonPos.X + stepX, demonPos.Y + stepY);

        if (TryMoveDemon(demon, targetPos))
            return;

        // If diagonal movement failed, try horizontal then vertical
        if (stepX != 0)
        {
            targetPos = new Point2D(demonPos.X + stepX, demonPos.Y);
            if (TryMoveDemon(demon, targetPos))
                return;
        }

        if (stepY != 0)
        {
            targetPos = new Point2D(demonPos.X, demonPos.Y + stepY);
            if (TryMoveDemon(demon, targetPos))
                return;
        }

        // If all else fails, try random adjacent movement
        int randomDir = _random.Next(4);
        switch (randomDir)
        {
            case 0: targetPos = new Point2D(demonPos.X + 1, demonPos.Y); break;
            case 1: targetPos = new Point2D(demonPos.X - 1, demonPos.Y); break;
            case 2: targetPos = new Point2D(demonPos.X, demonPos.Y + 1); break;
            case 3: targetPos = new Point2D(demonPos.X, demonPos.Y - 1); break;
        }

        TryMoveDemon(demon, targetPos);
    }

    /// <summary>
    /// Attempts to move a demon to the specified position.
    /// Returns true if movement was successful.
    /// </summary>
    private bool TryMoveDemon(Demon demon, Point2D targetPoint)
    {
        if (!IsPointWithinBounds(targetPoint))
            return false;

        double totalFillingRatio = GetTotalFillingRatio(targetPoint) + demon.FillingRatio;

        if (totalFillingRatio < 1.0)
        {
            demon.RelativePoint = targetPoint;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to move the player to the specified position.
    /// Returns true if movement was successful.
    /// </summary>
    public bool MovePlayer(Point2D targetPoint)
    {
        if (!IsPointWithinBounds(targetPoint))
            return false;

        double totalFillingRatio = GetTotalFillingRatio(targetPoint) + _game.PlayerFillingRatio;

        if (totalFillingRatio < 1.0)
        {
            _game.Player.RelativePoint = targetPoint;
            return true;
        }

        return false;
    }

    private double GetTotalFillingRatio(Point2D point)
    {
        double sum = 0;
        int key = GetGridKey(point);

        // Only check nearby objects!
        if (_spatialGrid.TryGetValue(key, out var nearbyObjects))
        {
            foreach (var obj in nearbyObjects)
            {
                Point2D? objPos = obj is GameItem item
                    ? item.WorldPosition
                    : ((Demon)obj).WorldPosition;

                if (Point2D.Distance(point, objPos) <= 0)
                {
                    sum += obj is GameItem i ? i.FillingRatio : ((Demon)obj).FillingRatio;
                }
            }
        }

        return sum;
    }

    private bool IsPointWithinBounds(Point2D point)
    {
        return point.X < Console.WindowWidth
               && point.X >= 0
               && point.Y < Console.WindowHeight - 1
               && point.Y >= 0;
    }
}
