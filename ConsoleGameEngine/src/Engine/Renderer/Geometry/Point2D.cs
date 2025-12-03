using System;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public class Point2D
{
    // =============================FIELDS_PRIVATE==============================
    private int _x, _y;
    
    // =============================SETTERS&GETTERS==============================
    public int X
    {
        get { return _x;}
        set { _x = value; }
    }

    public int Y
    {
        get => _y;
        set => _y = value;
    }

    public Point2D(int x, int y)
    {
        this._x = x;
        this._y = y;
    }

    public static Point2D operator +(Point2D a, Point2D b)
    {
        return new Point2D(a.X + b.X, a.Y + b.Y);
    }

    public static bool operator ==(Point2D? a, Point2D? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.X == b.X && a.Y == b.Y;
    }

    public static bool operator !=(Point2D? a, Point2D? b)
    {
        return !(a == b);
    }

    public static Point2D operator -(Point2D left, Point2D right)
    {
        return new Point2D(left.X - right.X, left.Y - right.Y);
    }

    // =============================METHODS==============================

    public static double Distance(Point2D position1, Point2D position2)
    {
        return Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }

    public Point2D Clamp(int minX, int maxX, int minY, int maxY)
    {
         _x = Math.Max(minX, _x);
         _x = Math.Min(maxX, _x);

         _y = Math.Max(minY, _y);
         _y = Math.Min(maxY, _y);
         
         return new Point2D(_x, _y);
    }
    
    public Point2D Clamp(Point2D minPoint, Point2D maxPoint)
    {
        _x = Math.Max(minPoint.X, _x);
        _x = Math.Min(maxPoint.X, _x);

        _y = Math.Max(minPoint.Y, _y);
        _y = Math.Min(maxPoint.Y, _y);
         
        return new Point2D(_x, _y);
    }

    public static void Clamp(int min, int max, out int value)
    {
        value = 0;
        value = Math.Max(min, value);
        value = Math.Min(max, value);
    }
    
    public Point2D Clamp(Point2D minPoint, Dimension2D maxDimension)
    {
        return Clamp(
            minPoint.X, 
            maxDimension.Width, 
            minPoint.Y, 
            maxDimension.Height
        );
    }
}

