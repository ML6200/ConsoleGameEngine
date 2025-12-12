using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Zombieman : Demon
{
    public Zombieman(int x, int y) : base(x, y)
    {
        FillingRatio = 4;
        Health = 60;
        SightRange = 70;
        AttackRange = 3;
        _speed = 93;
    }
    
    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // Transform world coordinates to screen coordinates
        Point2D screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == Point2D.OutsideScreenPoint) return; // Off-screen culling

        //renderer.SetCell(screenPos.X, screenPos.Y,
        //    new Cell('o', ConsoleColor.Black, ConsoleColor.Red));
        
        renderer.DrawText(screenPos.X, screenPos.Y, "☠", default, ConsoleColor.DarkCyan);
    }

    public override void GetAttackDamageRange(out int min, out int max)
    {
        min = 3;
        max = 15;
    }

    public override int GetCombatPoints()
    {
        return 1;
    }
}