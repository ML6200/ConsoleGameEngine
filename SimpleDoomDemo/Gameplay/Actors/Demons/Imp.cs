using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Imp : Demon
{
    public Imp(int x, int y) : base(x, y)
    {
        FillingRatio = 0.4d;
        Health = 20;
        SightRange = 70;
        AttackRange = 6;
        _speed = 93;
    }
    
    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // Transform world coordinates to screen coordinates
        Point2D? screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == null) return; // Off-screen culling

        renderer.SetCell(screenPos.X, screenPos.Y,
            new Cell('o', ConsoleColor.Black, ConsoleColor.White));
    }

    public override int GetAttackDamageRange(out int min, out int max)
    {
        min = 3;
        max = 24;
        return min;
    }

    public override int GetCombatPoints()
    {
        return 3;
    }
}