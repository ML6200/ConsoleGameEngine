using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Zombieman : Demon
{
    public Zombieman(int x, int y) : base(x, y)
    {
        // FillingRatio = 0.4d;
        // Health = 20;
        // SightRange = 70;
        // AttackRange = 3;
        // _speed = 93;
        FillingRatio = 0.4D;
        Health = 100;
        SightRange = 70;
        AttackRange = 3;
        _speed = 93;
    }
    
    public override void Compute(ConsoleRenderer2D consoleRenderer2D)
    {
        if (!Visible) return;
        consoleRenderer2D.SetCell(WorldPosition.X,
            WorldPosition.Y,
            new Cell('o', ConsoleColor.Black, ConsoleColor.Red));
        base.Compute(consoleRenderer2D);
    }

    public override int GetAttackDamageRange(out int min, out int max)
    {
        min = 3;
        max = 15;
        return min;
    }

    public override int GetCombatPoints()
    {
        return 1;
    }
}