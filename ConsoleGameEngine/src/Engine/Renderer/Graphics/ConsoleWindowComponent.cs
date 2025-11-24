using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleWindowComponent : IConsoleComponent
{
    private readonly ConsoleGraphicsComponent _consoleGraphicsComponent;
    
    public ConsoleGraphicsComponent ConsoleGraphicsComponent => _consoleGraphicsComponent;
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

        foreach (var child in _consoleGraphicsComponent.Children)
        {
            child.Render(renderer);
        }
    }

    public ConsoleWindowComponent(ConsoleGraphicsComponent consoleGraphicsComponent)
    {
        _consoleGraphicsComponent = consoleGraphicsComponent;
        Visible = true;
    }

    public void Update()
    {
        if (!Visible) return;
        
        foreach (var child in _consoleGraphicsComponent.Children)
            child.Update();
    }
}