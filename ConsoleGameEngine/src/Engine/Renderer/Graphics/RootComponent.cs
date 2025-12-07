using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Animations;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class RootComponent : IRenderable
{
    private readonly GraphicsComponent _graphicsComponent;
    public List<Animation> Animations { get; } = new();

    public GraphicsComponent GraphicsComponent => _graphicsComponent;
    public Dimension2D WorldSize => new(Console.WindowWidth, Console.WindowHeight);

    public bool Visible
    {
        get; set;
    }

    public void Compute(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        foreach (var child in _graphicsComponent.GetChildrenSnapshot())
        {
            child.Compute(renderer);
        }
    }

    public RootComponent(GraphicsComponent graphicsComponent)
    {
        _graphicsComponent = graphicsComponent;
        Visible = true;
    }
}