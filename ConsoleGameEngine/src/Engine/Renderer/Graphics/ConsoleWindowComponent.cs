using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Animations;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleWindowComponent : IConsoleRenderable
{
    private readonly ConsoleGraphicsComponent _consoleGraphicsComponent;
    public List<Animation> Animations { get; } = new();

    public ConsoleGraphicsComponent ConsoleGraphicsComponent => _consoleGraphicsComponent;
    public Dimension2D WorldSize => new(Console.WindowWidth, Console.WindowHeight);

    public bool Visible
    {
        get; set;
    }

    public void Compute(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        foreach (var child in _consoleGraphicsComponent.GetChildrenSnapshot())
        {
            child.Compute(renderer);
        }
    }

    public ConsoleWindowComponent(ConsoleGraphicsComponent consoleGraphicsComponent)
    {
        _consoleGraphicsComponent = consoleGraphicsComponent;
        Visible = true;
    }
}