using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer;

public abstract class ConsoleComponent
{
    private int _width;
    private int _height;

    public Dimension2D Size
    {
        get
        {
            return new Dimension2D(_width, _height);
        }
        set
        {
            _width = value.Width;
            _height = value.Height;
        }
    }
    
    public Position2D Position { get; set; }
    public virtual bool Visible { get; set; } = true;
    
    public ConsoleColor BackgroundColor { get; protected set; }
    public ConsoleColor ForegroundColor { get; protected set; }
    protected List<ConsoleComponent> Children { get; } = new();
    
    public virtual void AddChild(ConsoleComponent child) => Children.Add(child);
    public virtual void RemoveChild(ConsoleComponent child) => Children.Remove(child);
    
    public abstract void Update();

    public virtual void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        foreach (var child in Children)
        {
            child.Render(renderer);
        }
    }
    
    public virtual void Dispose()
    {
        Children.Clear();
    }
}