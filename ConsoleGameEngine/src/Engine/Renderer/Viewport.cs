using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

public class Viewport : GraphicsComponent
{
    public ConsoleCamera? Camera { get; set; }
    public Dimension2D ViewportSize { get; set; }
    public Point2D ViewportPosition { get; set; }
}