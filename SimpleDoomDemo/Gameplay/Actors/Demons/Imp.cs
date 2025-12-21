using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Imp : Demon
{
    public Imp(int x, int y) : base(x, y)
    {
        Solidity = 4;
        Health = 20;
        SightRange = 70;
        AttackRange = 6;
        _speed = 93;
    }
    
    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // Transform world coordinates to screen coordinates
        //if (screenPoint == Point2D.OutsideScreenPoint) return; // Off-screen culling

        renderer.DrawText(screenPoint.X, screenPoint.Y, "☠");
    }

    public override void GetAttackDamageRange(out int min, out int max)
    {
        min = 3;
        max = 24;
    }

    public override int GetCombatPoints()
    {
        return 3;
    }
}