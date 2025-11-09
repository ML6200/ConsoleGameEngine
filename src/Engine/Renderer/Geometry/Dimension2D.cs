namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public class Dimension2D
{
    public int Width { get; set; }
    public int Height { get; set; }

    public Dimension2D(int width, int height)
    {
        Width = width;
        Height = height;
    }
}