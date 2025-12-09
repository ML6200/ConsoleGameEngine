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

        renderer.DrawText(screenPos.X, screenPos.Y, "☠");
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