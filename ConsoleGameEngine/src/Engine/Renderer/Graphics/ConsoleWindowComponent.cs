using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleWindowComponent : IConsoleRenderable
{
    private readonly ConsoleGraphicsComponent _consoleGraphicsComponent;
    public List<Animation.Animation> Animations { get; } = new List<Animation.Animation>();
    
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
}