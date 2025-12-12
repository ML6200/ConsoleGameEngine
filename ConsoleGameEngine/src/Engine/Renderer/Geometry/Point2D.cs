using System;
using System.Runtime.CompilerServices;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public readonly struct Point2D(int x, int y) : IEquatable<Point2D>
{
    // =============================FIELDS_PUBLIC==============================
    public readonly int X = x;
    public readonly int Y = y;

    // =============================OPERATOR-OVERLOADS==============================
    
    /* ADD */
    
    /*
     * [MethodImpl(MethodImplOptions.AggressiveInlining)]
     * 
     * We give a hint to the JIT compiler to insert the body into the calling code
     * therefore it can reduce the amount of stack calls for small frequent calls
     * It can also increase the size of the compiled binary since its being duplicated
     * every call, but it can provide benefit for small math functions/helpers  
     */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator +(Point2D a, Point2D b) 
        => new(a.X + b.X, a.Y + b.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator +(Point2D a, int b) 
        => new(a.X + b, a.Y + b);
    
    /* SUBTRACT */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator -(Point2D a, Point2D b)
        => new(a.X - b.X, a.Y - b.Y);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Point2D operator -(Point2D a, int b)
        => new(a.X - b, a.Y - b);
    
    /* EQUALS */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Point2D a, Point2D b)
        => a.X == b.X && a.Y == b.Y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Point2D a, Point2D b)
        => a.X != b.X || a.Y != b.Y;
    
    
    // =============================DISTANCES==============================
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double EuclideanDistance(Point2D position1, Point2D position2)
    {
        return Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ChebyshevDistance(Point2D position1, Point2D position2)
    {
        int dx = position2.X - position1.X;
        int dy = position2.Y - position1.Y;
        
        if (dx < 0) dx = -dx;
        if (dy < 0) dy = -dy;
        
        return dx > dy ? dx : dy;
    }

    // =============================CLAMPS==============================
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D Clamp(int minX, int maxX, int minY, int maxY)
        => new(
            Math.Clamp(X, minX, maxX),
            Math.Clamp(Y, minY, maxY)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D Clamp(Point2D min, Point2D max)
        => new(
            Math.Clamp(X, min.X, max.X),
            Math.Clamp(Y, min.Y, max.Y)
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Point2D Clamp(Point2D min, Dimension2D max)
        => new(
            Math.Clamp(X, min.X, max.Width),
            Math.Clamp(Y, min.Y, max.Height)
        );

    // =========================VALUE-TYPE-UTILS==========================
    public bool Equals(Point2D other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Point2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
    
    public static Point2D NullPoint = new(0, 0);
    
    /// <summary>
    /// OutsideScreenPoint is for culling when the object is outside the sight range
    /// </summary>
    public static Point2D OutsideScreenPoint = new(-1, -1);
}