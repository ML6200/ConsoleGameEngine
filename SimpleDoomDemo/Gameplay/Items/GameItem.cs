using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomEngine.Gameplay.Items;

public class GameItem : GraphicsComponent
{
    // =============================FIELDS_PUBLIC==============================
    public ItemType Type { get; }
    public int Solidity { get; set; }
    public bool Available { get; private set; }
    
    // =============================METHODS==============================
    private volatile char _glyph;

    private void SetInitialProperties()
    {
        Available = true;
        switch (Type)
        {
            case ItemType.Ammo:
                Solidity = 0;
                ForegroundColor = ConsoleColor.Yellow;
                _glyph = '⁍';
                break;

            case ItemType.BfgCell:
                Solidity = 0;
                ForegroundColor = ConsoleColor.Green;
                _glyph = 'B';
                break;

            case ItemType.Door:
                Solidity = 10;
                BackgroundColor = ConsoleColor.Gray;
                ForegroundColor = ConsoleColor.Black;
                _glyph = '/';
                break;

            case ItemType.LevelExit:
                Solidity = 0;
                BackgroundColor = ConsoleColor.Blue;
                ForegroundColor = ConsoleColor.Black;
                _glyph = 'E';
                break;

            case ItemType.MedKit:
                Solidity = 0;
                BackgroundColor = ConsoleColor.DarkGray;
                ForegroundColor = ConsoleColor.Red;
                _glyph = '+';
                break;

            case ItemType.ToxicWaste:
                Solidity = 0;
                ForegroundColor = ConsoleColor.Green;
                _glyph = '☣';
                break;

            case ItemType.Wall:
                Solidity = 10;
                BackgroundColor = ConsoleColor.Gray;
                ForegroundColor = ConsoleColor.Gray;
                _glyph = ' ';
                break;
        }
    }

    public GameItem(int x, int y, ItemType type)
    {
        RelativePosition = new Point2D(x, y);
        Type = type;
        SetInitialProperties();
    }

    public void Interact()
    {
        if (Type == ItemType.Ammo
            || Type == ItemType.BfgCell
            || Type == ItemType.MedKit)
        {
            Available = false;
        }
        else if (Type == ItemType.Door)
        {
            if (Solidity.Equals(10))
            {
                Solidity = 0;
                ForegroundColor = ConsoleColor.Black;
                _glyph = '/';
            }
            else
            {
                ForegroundColor = ConsoleColor.Black;
                Solidity = 10;
                _glyph = '\\';
            }
        }
    }

    /// <summary>
    /// Update visibility based on distance from player.
    /// </summary>
    public void UpdateVisibility(Point2D playerPoint, double sightRange)
    {
        double distance = Point2D.ChebyshevDistance(WorldPosition, playerPoint);
        Visible = Available && distance <= sightRange;
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        if (screenPoint == Point2D.OutsideScreenPoint) return;

        renderer.SetCell(screenPoint.X, screenPoint.Y,
            new Cell(_glyph, BackgroundColor, ForegroundColor));
    }
}