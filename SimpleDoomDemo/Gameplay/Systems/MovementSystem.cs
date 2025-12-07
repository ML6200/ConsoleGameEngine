using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Systems;

public class MovementSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly Random _random = new();

    public MovementSystem(DoomGameScene game)
    {
        _game = game;
    }

    public void Update(double deltaTime)
    {
        foreach (var demon in _game.Demons)
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

        double totalFillingRatio = GetTotalFillingRatio(targetPos) + demon.FillingRatio;
        if (rndMove < pMove)
        {
            if (totalFillingRatio < 1.0)
            {
                demon.WorldPosition = targetPos;
            }
        }
    }
    
    public bool MovePlayer(Point2D targetPoint)
    {
        if (!IsPointWithinBounds(targetPoint))
            return false;

        double totalFillingRatio = GetTotalFillingRatio(targetPoint) + _game.PlayerFillingRatio;

        if (totalFillingRatio < 1.0)
        {
            _game.Player.RelativePosition = targetPoint;
            return true;
        }

        return false;
    }

    public double GetTotalFillingRatio(Point2D position)
    {
        List<GameItem> items = GetGameItemsWithinDistance(position, 0);
        List<Demon> dems = GetDemonsWithinDistance(position, 0);
        
        double sum = 0;
        
        foreach (var t in items)
        {
            sum += t.FillingRatio;
        }
        
        for (int i = 0; i < dems.Count; i++)
        {
            sum += dems[i].FillingRatio;
        }
        
        return sum;
    }
    
    public List<Demon> GetDemonsWithinDistance(Point2D position, double distanceTreshold)
    {
        List<Demon> closeItems = new List<Demon>();

        for (int i = 0; i < _game.Demons.Count; i++)
        {
            double distance = Point2D.Distance(position, _game.Demons[i].WorldPosition);

            if (distance <= distanceTreshold)
            {
                closeItems.Add(_game.Demons[i]);
            }
        }
        
        return closeItems;
    }
    
    public List<GameItem> GetGameItemsWithinDistance(Point2D position, double distanceTreshold)
    {
        List<GameItem> closeItems = new List<GameItem>();

        for (int i = 0; i < _game.Items.Count; i++)
        {
            double distance = Point2D.Distance(position, _game.Items[i].WorldPosition);

            if (distance <= distanceTreshold)
            {
                closeItems.Add(_game.Items[i]);
            }
        }
        
        return closeItems;
    }



    private bool IsPointWithinBounds(Point2D point)
    {
        return point.X < Console.WindowWidth
               && point.X >= 0
               && point.Y < Console.WindowHeight - 1
               && point.Y >= 0;
    }
}
