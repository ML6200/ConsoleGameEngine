using System;
using System.Drawing;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public struct Dimension2D : IEquatable<Dimension2D>
{
    public int Width { get; set; }
    public int Height { get; set; }
    
    public static Dimension2D NullSize => new(0, 0);

    public Dimension2D(int width, int height)
    {
        Width = width;
        Height = height;
    }
    
    public static Dimension2D operator +(Dimension2D a, Dimension2D b)
    {
        return new Dimension2D(a.Width + b.Width, a.Height + b.Height);
    }

    public static Dimension2D operator +(Dimension2D a, int scalar) 
        => new Dimension2D(a.Width + scalar, a.Height + scalar);

    public static Dimension2D operator -(Dimension2D a, Dimension2D b)
        => new Dimension2D(a.Width - b.Width, a.Height - b.Height);
    
    public static Dimension2D operator -(Dimension2D a, int scalar) 
        => new Dimension2D(a.Width - scalar, a.Height - scalar);
    
    public static bool operator ==(Dimension2D a, Dimension2D b) 
        => a.Equals(b);

    public static bool operator !=(Dimension2D a, Dimension2D b) 
        => !(a == b);

    public bool Equals(Dimension2D other)
    {
        return Width == other.Width && Height == other.Height;
    }

    public override bool Equals(object? obj)
    {
        return obj is Dimension2D other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }
}