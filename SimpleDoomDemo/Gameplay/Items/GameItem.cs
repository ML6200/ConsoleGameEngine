using System;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomEngine.Gameplay.Items;

public class GameItem : GraphicsComponent
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
            case ItemType.Ammo:
                FillingRatio = 0.0;
                ForegroundColor = ConsoleColor.Yellow;
                _glyph = '⁍';
                break;

            case ItemType.BfgCell:
                FillingRatio = 0.0;
                ForegroundColor = ConsoleColor.Green;
                _glyph = 'B';
                break;

            case ItemType.Door:
                FillingRatio = 1.0;
                BackgroundColor = ConsoleColor.Gray;
                ForegroundColor = ConsoleColor.Yellow;
                _glyph = '/';
                break;

            case ItemType.LevelExit:
                FillingRatio = 1.0;
                BackgroundColor = ConsoleColor.Blue;
                ForegroundColor = ConsoleColor.Black;
                _glyph = 'E';
                break;

            case ItemType.MedKit:
                FillingRatio = 0.0;
                BackgroundColor = ConsoleColor.DarkGray;
                ForegroundColor = ConsoleColor.Red;
                _glyph = '+';
                break;

            case ItemType.ToxicWaste:
                FillingRatio = 0.0;
                ForegroundColor = ConsoleColor.Green;
                _glyph = '☣';
                break;

            case ItemType.Wall:
                FillingRatio = 1.0;
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
    public void UpdateVisibility(Point2D playerPoint, double sightRange)
    {
        double distance = Point2D.Distance(WorldPosition, playerPoint);
        Visible = Available && distance <= sightRange;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        // Transform world coordinates to screen coordinates
        Point2D? screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == null) return; // Off-screen culling

        renderer.SetCell(screenPos.X, screenPos.Y,
            new Cell(_glyph, BackgroundColor, ForegroundColor));
    }
}