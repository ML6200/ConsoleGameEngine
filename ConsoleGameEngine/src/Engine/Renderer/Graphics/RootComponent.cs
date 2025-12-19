using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public sealed class RootComponent : IRenderable
{
    private readonly GraphicsComponent _graphicsComponent;

    public GraphicsComponent GraphicsComponent => _graphicsComponent;
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);

    public bool Visible
    {
        get; set;
    }

    public void Compute(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (!Visible) return;

        _graphicsComponent.Compute(renderer, camera);
    }

    public void Update(double deltaTime)
    {
        if (!Visible) return;
        
        _graphicsComponent.Update(deltaTime);
    }

    public RootComponent(GraphicsComponent graphicsComponent)
    {
        _graphicsComponent = graphicsComponent;
        Visible = true;
    }
}