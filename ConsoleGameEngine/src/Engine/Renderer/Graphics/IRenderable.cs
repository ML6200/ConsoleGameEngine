using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public interface IRenderable
{
    Dimension2D ScreenSize { get; }
    bool Visible { get; set; }
    void Compute(ConsoleRenderer2D renderer, ConsoleCamera camera);
}