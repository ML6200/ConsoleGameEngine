using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

public class RenderPipeline
{
    private List<RenderAction> _queue = new();
    private int _counter = 0;

    public ConsoleCamera? Camera { get; set; }

    public RenderPipeline()
    {
    }

    public void Submit(Action action)
    {
        _queue.Add(new RenderAction()
        {
            Action = action,
            SequenceId = _counter++
        });
    }

    public void Compute(ConsoleRenderer2D renderer)
    {
        var sortedAction = _queue
            .OrderBy(c => c.Layer)
            .ThenBy(c => c.ZIndex)
            .ThenBy(c => c.SequenceId);

        foreach (var action in sortedAction)
        {
            action.Action();
        }
        _queue.Clear();
        _counter = 0;
    }
    
    public void ComputeComponentTree(GraphicsComponent root, ConsoleRenderer2D renderer)
    {
        WalkTree(root, renderer);
    }

    private void WalkTree(GraphicsComponent component, ConsoleRenderer2D renderer)
    {
        if (!component.Visible) return;

        if (component is Viewport viewport)
        {
            var previousCamera = Camera;
            Camera = viewport.Camera;
            
            foreach (var child in component.Children)
                WalkTree(child, renderer);

            Camera = previousCamera;
            return;
        }

        Point2D screenPos = Camera != null
            ? Camera.TransformPoint(component.WorldPosition)
            : component.WorldPosition; 

        Submit(() => component.Draw(renderer, screenPos));

        foreach (var child in component.Children)
            WalkTree(child, renderer);
    }
}