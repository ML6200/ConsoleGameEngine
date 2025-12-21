using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Mancubus : Demon
{
    public Mancubus(int x, int y) : base(x, y)
    {
        Solidity = 9;
        Health = 600;
        SightRange = 70;
        AttackRange = 9;
        _speed = 70;
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // Transform world coordinates to screen coordinates
        //if (screenPoint == Point2D.OutsideScreenPoint) return; // Off-screen culling

        renderer.SetCell(screenPoint.X, screenPoint.Y,
            new Cell('Ω', ConsoleColor.Black, ConsoleColor.DarkRed));
    }

    public override void GetAttackDamageRange(out int min, out int max)
    {
        min = 8;
        max = 64;
    }

    public override int GetCombatPoints()
    {
        return 10;
    }
}