using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Actors.Demons;


public abstract class Demon : ConsoleGraphicsComponent
{
    // =========================FIELDS_PRIVATE==============================
    protected int _speed;

    // =========================FIELDS_SETTERS&GETTERS==============================
    public double FillingRatio { get; protected set; }
    public bool Alive { get; private set; }
    public int Health { get; set; }
    public int SightRange { get; set; }
    public int AttackRange { get; set; }
    public int Speed { get { return _speed; } }
    public DemonState  State { get; private set; }
    
    // =============================METHODS==============================

    public Demon(int x, int y)
    {
        RelativePosition = new Position2D(x, y);
        Alive = true;
    }


    public void UpdateState(Player player)
    {
        Position2D playerPos = player.AbsolutePosition;
        Position2D demonPos = AbsolutePosition; 
        double dist = Position2D.Distance(playerPos, demonPos);

        if (dist < AttackRange)
        {
            State = DemonState.Attack;
        } 
        else if (dist < SightRange)
        {
            State = DemonState.Move;
        }
        else State = DemonState.Idle;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0) Alive = false;
    }

    public abstract int GetAttackDamageRange(out int min, out int max);
    public abstract int GetCombatPoints();
}