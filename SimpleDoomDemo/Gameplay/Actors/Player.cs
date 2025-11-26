using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine.Engine;

namespace SimpleDoomEngine.Gameplay.Actors;

public class Player : ConsoleGraphicsComponent
{
    // =============================FIELDS_PRIVATE==============================
    private int _health;
    private int _ammo;
    private int _bfgCells;
    private int _compatPoints;
    private int _sightRange;
    private bool _alive;

    // ==========================FIELDS_SETTERS&GETTERS=========================
    public int Ammo 
    { 
        get { return _ammo; } 
        set { 
            if (value > MaxAmmo) _ammo = MaxAmmo;
            else if(value < 0) _ammo = 0;
            else _ammo = value;
        }   
    }

    public int CombatPoints
    {
        get {return _compatPoints;} 
    }

    public bool Alive
    {
        get { return _alive; }
    }
    public int Health
    {
        get { return _health; }
        set { 
            if (value > MaxHealth) _health = MaxHealth;
            else if(value < 0) _health = 0;
            else _health = value;
        }       
    }

    public int MaxHealth
    {
        get { return CombatPoints / 10 + 100; }
    }

    public int MaxAmmo
    {
        get { return CombatPoints / 50 + 10; }
    }

    public int BFGCells
    {
        get { return _bfgCells; }
        set {
            if (value > MaxBFGCells) _bfgCells = MaxBFGCells;
            else if(value < 0) _bfgCells = 0;
            else _bfgCells = value;
        }
    }

    public int MaxBFGCells
    {
        get { return 3; }
    }

    public int SightRange
    {
        get { return _sightRange; }
    }

    // =============================METHODS==============================
    public Player(int x, int y)
    {
        _health = 100;
        _sightRange = 8;
        _ammo = 10;
        _bfgCells = 0;
        _alive = true;
    }

    public void Shoot()
    {
        if (_ammo > 0)
            _ammo--;
    }

    public void AddCombatPoints(int compatPoint)
    {
        _compatPoints += compatPoint;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health == 0) _alive = false;
    }

    public void PickUpAmmo(int ammo)
    {
        Ammo += ammo;
    }

    public void PickUpHealth(int health)
    {
        Health += health;
    }

    public void PickUpBFGCell(int cells)
    {
        BFGCells += cells;
    }

    public void ShootBFG()
    {
        if (_bfgCells > 0)
            _bfgCells--;
    }
    

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!_alive)
        {
            Visible = false;
            return;
        }

        renderer.SetCell(AbsolutePosition.X, AbsolutePosition.Y,
            new Cell('0', ConsoleColor.Black, ConsoleColor.Green));

        base.Render(renderer);
    }
}
