using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;

public class Zombieman : Demon
{
    public Zombieman(int x, int y) : base(x, y)
    {
        Solidity = 4;
        Health = 60;
        SightRange = 70;
        AttackRange = 3;
        _speed = 93;
    }
    
    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        renderer.DrawText(screenPoint.X, screenPoint.Y, "☠", default, ConsoleColor.DarkCyan);
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