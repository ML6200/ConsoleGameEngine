using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Imp : Demon
{
    public Imp(int x, int y) : base(x, y)
    {
        FillingRatio = 0.4d;
        Health = 200;
        SightRange = 70;
        AttackRange = 6;
        _speed = 93;
    }
    
    public override void Render(ConsoleRenderer2D consoleRenderer2D)
    {
        if (!Alive || !Visible) return;
        consoleRenderer2D.SetCell(AbsolutePosition.X,
            AbsolutePosition.Y,
            new Cell('o', ConsoleColor.Black, ConsoleColor.White));
        base.Render(consoleRenderer2D);
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