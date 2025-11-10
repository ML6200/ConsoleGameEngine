using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public interface IConsoleComponent
{
    Dimension2D WorldSize { get; }
    bool Visible { get; set; }
    void Render(ConsoleRenderer2D renderer);
    void Update();
}