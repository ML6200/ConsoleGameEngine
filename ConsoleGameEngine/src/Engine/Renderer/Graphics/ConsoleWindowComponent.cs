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
    public ConsoleCamera Camera {get; set;}

    public bool Visible
    {
        get; set;
    }

    public void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        if (_consoleGraphicsComponent.WorldPosition != null)
            _consoleGraphicsComponent.SetAbsolutePosition(
                Camera.TransformPoint(_consoleGraphicsComponent.WorldPosition));

        foreach (var child in _consoleGraphicsComponent.GetChildrenSnapshot())
        {
            child.Render(renderer);
        }
    }

    public ConsoleWindowComponent(ConsoleGraphicsComponent consoleGraphicsComponent, ConsoleCamera camera)
    {
        _consoleGraphicsComponent = consoleGraphicsComponent;
        Visible = true;

        Camera = camera;
    }
}