using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomEngine.Engine;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Mancubus : Demon
{
    public Mancubus(int x, int y) : base(x, y)
    {
        FillingRatio = 0.96d;
        Health = 600;
        SightRange = 70;
        AttackRange = 9;
        _speed = 70;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // Transform world coordinates to screen coordinates
        Point2D? screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == null) return; // Off-screen culling

        renderer.SetCell(screenPos.X, screenPos.Y,
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