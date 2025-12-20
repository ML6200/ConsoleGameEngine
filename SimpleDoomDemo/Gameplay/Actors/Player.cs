using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomEngine.Gameplay.Actors;

public class Player : GraphicsComponent
{
    // =============================FIELDS_PRIVATE==============================
    private int _health;
    private int _ammo;
    private int _bfgCells;
    private int _compatPoints;
    private readonly int _sightRange;
    private bool _alive;

    // ==========================FIELDS_SETTERS&GETTERS=========================

    public static readonly int PlayerSolidity = 4;
    
    public int Ammo 
    { 
        get => _ammo;
        private set { 
            if (value > MaxAmmo) _ammo = MaxAmmo;
            else if(value < 0) _ammo = 0;
            else _ammo = value;
        }   
    }

    public int CombatPoints => _compatPoints;

    public bool Alive => _alive;

    public int Health
    {
        get => _health;
        private set 
        { 
            if (value > MaxHealth) _health = MaxHealth;
            else if(value < 0) _health = 0;
            else _health = value;
        }       
    }

    public int MaxHealth => CombatPoints / 10 + 100;

    public int MaxAmmo => CombatPoints / 50 + 10;

    public int BfgCells
    {
        get => _bfgCells;
        private set {
            if (value > MaxBfgCells) _bfgCells = MaxBfgCells;
            else if(value < 0) _bfgCells = 0;
            else _bfgCells = value;
        }
    }

    public int MaxBfgCells => 3;

    public int SightRange => _sightRange;

    // =============================METHODS==============================
    public Player(int x, int y)
    {
        RelativePosition = new Point2D(x, y);
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
        BfgCells += cells;
    }

    public void ShootBFG()
    {
        if (_bfgCells > 0)
            _bfgCells--;
    }
    

    protected override void Draw(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (!_alive)
        {
            Visible = false;
            return;
        }

        // Transform world coordinates to screen coordinates
        Point2D screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == Point2D.OutsideScreenPoint) return; // Off-screen culling

        renderer.SetCell(screenPos.X, screenPos.Y,
            new Cell('●', ConsoleColor.Black, ConsoleColor.Green));
    }
}
