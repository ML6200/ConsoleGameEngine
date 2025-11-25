using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleWindowRenderable : IConsoleRenderable
{
    private readonly ConsoleGraphicsRenderable _consoleGraphicsRenderable;
    
    public ConsoleGraphicsRenderable ConsoleGraphicsRenderable => _consoleGraphicsRenderable;
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

        foreach (var child in _consoleGraphicsRenderable.Children)
        {
            child.Render(renderer);
        }
    }

    public ConsoleWindowRenderable(ConsoleGraphicsRenderable consoleGraphicsRenderable)
    {
        _consoleGraphicsRenderable = consoleGraphicsRenderable;
        Visible = true;
    }
}