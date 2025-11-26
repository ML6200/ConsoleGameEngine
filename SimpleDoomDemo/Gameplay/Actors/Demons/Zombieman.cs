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
        Health = 20;
        SightRange = 70;
        AttackRange = 3;
        _speed = 93;
    }
    
    public override void Render(ConsoleRenderer2D consoleRenderer2D)
    {
        if (!Alive) return;
        consoleRenderer2D.SetCell(AbsolutePosition.X, 
            AbsolutePosition.Y, 
            new Cell('o', ConsoleColor.Black, ConsoleColor.Red));
        base.Render(consoleRenderer2D);
    }
}