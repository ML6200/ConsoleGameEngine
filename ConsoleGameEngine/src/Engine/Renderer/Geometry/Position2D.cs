using System;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public class Position2D
{
    // =============================FIELDS_PRIVATE==============================
    private int x, y;
    
    // =============================SETTERS&GETTERS==============================
    public int X
    {
        get { return x;}
        set { x = value; }
    }

    public int Y
    {
        get => y;
        set => y = value;
    }

    public Position2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static Position2D operator +(Position2D a, Position2D b)
    {
        return new Position2D(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator ==(Position2D a, Position2D b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Position2D a, Position2D b)
    {
        return !(a == b);
    }

    public static Position2D operator -(Position2D left, Position2D right)
    {
        return new Position2D(left.X - right.X, left.Y - right.Y);
    }

    // =============================METHODS==============================

    public static double Distance(Position2D position1, Position2D position2)
    {
        return Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }

    public void Clamp(int minX, int maxX, int minY, int maxY)
    {
         x = Math.Max(minX, x);
         x = Math.Min(maxX, x);

         y = Math.Max(minY, y);
         y = Math.Min(maxY, y);
    }
}

