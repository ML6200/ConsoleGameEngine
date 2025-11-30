using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomEngine.Gameplay.Items;

public class GameItem : ConsoleGraphicsComponent
{
    // =============================FIELDS_PUBLIC==============================
    public ItemType Type { get; }
    public double FillingRatio { get; set; }
    public bool Available { get; private set; }

    
    // =============================METHODS==============================
    private char _glyph;

    private void SetInitialProperties()
    {
        Available = true;
        switch (Type)
        {
            case ItemType.AMMO:
                FillingRatio = 0.0;
                BackgroundColor = ConsoleColor.Red;
                ForegroundColor = ConsoleColor.Yellow;
                _glyph = 'A';
                break;

            case ItemType.BFGCELL:
                FillingRatio = 0.0;
                BackgroundColor = ConsoleColor.Green;
                ForegroundColor = ConsoleColor.White;
                _glyph = 'B';
                break;

            case ItemType.DOOR:
                FillingRatio = 1.0;
                BackgroundColor = ConsoleColor.Gray;
                ForegroundColor = ConsoleColor.Yellow;
                _glyph = '/';
                break;

            case ItemType.LEVELEXIT:
                FillingRatio = 1.0;
                BackgroundColor = ConsoleColor.Blue;
                ForegroundColor = ConsoleColor.Black;
                _glyph = 'E';
                break;

            case ItemType.MEDKIT:
                FillingRatio = 0.0;
                BackgroundColor = ConsoleColor.DarkGray;
                ForegroundColor = ConsoleColor.Red;
                _glyph = '+';
                break;

            case ItemType.TOXICWASTE:
                FillingRatio = 0.0;
                BackgroundColor = ConsoleColor.Green;
                ForegroundColor = ConsoleColor.DarkGray;
                _glyph = ':';
                break;

            case ItemType.WALL:
                FillingRatio = 1.0;
                BackgroundColor = ConsoleColor.Gray;
                ForegroundColor = ConsoleColor.Gray;
                _glyph = ' ';
                break;
        }
    }

    public GameItem(int x, int y, ItemType type)
    {
        RelativePosition = new Position2D(x, y);
        Type = type;
        SetInitialProperties();
    }

    public void Interact()
    {
        if (Type == ItemType.AMMO
            || Type == ItemType.BFGCELL
            || Type == ItemType.MEDKIT)
        {
            Available = false;
        }
        else if (Type == ItemType.DOOR)
        {
            if (FillingRatio.Equals(1.0))
            {
                FillingRatio = 0.0;
                ForegroundColor = ConsoleColor.DarkYellow;
            }
            else
            {
                ForegroundColor = ConsoleColor.Yellow;
                FillingRatio = 1.0;
            }
        }
    }

    /// <summary>
    /// Update visibility based on distance from player.
    /// </summary>
    public void UpdateVisibility(Position2D playerPosition, double sightRange)
    {
        double distance = Position2D.Distance(WordPosition, playerPosition);
        Visible = Available && distance <= sightRange;
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        renderer.SetCell(WordPosition.X, WordPosition.Y,
            new Cell(_glyph, BackgroundColor, ForegroundColor));

        base.Render(renderer);
    }
}