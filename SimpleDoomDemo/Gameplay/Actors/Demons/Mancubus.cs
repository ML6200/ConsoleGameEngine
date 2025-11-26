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

    public override void Render(ConsoleRenderer2D consoleRenderer2D)
    {
        if (!Alive) return;
        consoleRenderer2D.SetCell(AbsolutePosition.X, 
            AbsolutePosition.Y, 
            new Cell('O', ConsoleColor.Black, ConsoleColor.DarkRed));
        base.Render(consoleRenderer2D);
    }
}