namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public struct Dimension2D
{
    public int Width { get; set; }
    public int Height { get; set; }

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
    {
        return new Dimension2D(a.Width + scalar, a.Height + scalar);
    }
    
    public static Dimension2D operator -(Dimension2D a, Dimension2D b)
    {
        return new Dimension2D(a.Width - b.Width, a.Height - b.Height);
    }
    
    public static Dimension2D operator -(Dimension2D a, int scalar)
    {
        return new Dimension2D(a.Width - scalar, a.Height - scalar);
    }
}