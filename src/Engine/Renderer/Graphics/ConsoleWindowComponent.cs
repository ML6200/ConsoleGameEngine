using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleWindowComponent(ConsoleGraphicsComponent consoleGraphicsComponent) : IConsoleComponent
{
    public Dimension2D WorldSize
    {
        get { return new Dimension2D(Console.WindowWidth, Console.WindowHeight); }
    }

    public bool Visible
    {
        get; set;
    }

    public void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        foreach (var child in consoleGraphicsComponent.Children)
        {
            child.Render(renderer);
        }
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}