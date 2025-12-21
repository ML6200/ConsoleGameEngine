using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public interface IRenderable
{
    Dimension2D ScreenSize { get; }
    bool Visible { get; set; }
    void Draw(ConsoleRenderer2D renderer, Point2D screenPosition);
    void Update(double deltaTime);
}