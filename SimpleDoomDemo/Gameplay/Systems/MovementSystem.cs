using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Systems;

public class MovementSystem(DoomGameScene game) : IGameSystem
{
    private readonly Random _random = new();

    public void Update(double deltaTime)
    {
        foreach (var demon in game.Demons)
        {
            if (demon.State == DemonState.Move)
            {
                MoveDemon(demon, deltaTime);
            }
        }
    }
    
    private void MoveDemon(Demon demon, double deltaTime)
    {
        int rndX = _random.Next(-1, 2);
        int rndY = _random.Next(-1, 2);
        Point2D targetPos = new Point2D(demon.WorldPosition.X + rndX, demon.WorldPosition.Y + rndY);

        // probability calculation
        double pMove = (demon.Speed / 100.0) * deltaTime;
        double rndMove = _random.NextDouble();

        if (!IsPointWithinBounds(targetPos))
            return;

        int totalSolidity = GetTotalSolidity(targetPos) + demon.Solidity;
        if (rndMove < pMove)
        {
            if (totalSolidity < 10)
            {
                demon.WorldPosition = targetPos;
            }
        }
    }
    
    public void MovePlayer(Point2D targetPoint)
    {
        if (!IsPointWithinBounds(targetPoint)) return;

        int totalSolidity = GetTotalSolidity(targetPoint) + Player.PlayerSolidity;

        if (totalSolidity < 10)
        {
            game.Player.RelativePosition = targetPoint;
        }
    }

    private int GetTotalSolidity(Point2D position)
    {
        List<GameItem> items = GetGameItemsWithinDistance(position, 0);
        List<Demon> dems = GetDemonsWithinDistance(position, 0);
        
        int sum = 0;
        
        foreach (var t in items)
        {
            sum += t.Solidity;
        }
        
        foreach (var d in dems)
        {
            sum += d.Solidity;
        }
        
        return sum;
    }

    private List<Demon> GetDemonsWithinDistance(Point2D position, int distanceThreshold)
    {
        List<Demon> closeItems = new List<Demon>();

        for (int i = 0; i < game.Demons.Count; i++)
        {
            int distance = Point2D.ChebyshevDistance(position, game.Demons[i].WorldPosition);

            if (distance <= distanceThreshold)
            {
                closeItems.Add(game.Demons[i]);
            }
        }

        return closeItems;
    }

    private List<GameItem> GetGameItemsWithinDistance(Point2D position, int distanceTreshold)
    {
        List<GameItem> closeItems = new List<GameItem>();
        foreach (var items in game.Items)
        {
            int distance = Point2D.ChebyshevDistance(position, items.WorldPosition);

            if (distance <= distanceTreshold)
            {
                closeItems.Add(items);
            }
        }

        return closeItems;
    }



    private bool IsPointWithinBounds(Point2D point)
    {
        return point.X >= 0 && point.X < game.WorldSize.Width
               && point.Y >= 0 && point.Y < game.WorldSize.Height;
    }
}
