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

    public void Submit(GraphicsComponent component,  ConsoleRenderer2D renderer, Point2D screenPos)
    {
        _queue.Add(new RenderAction()
        {
            Component = component,
            Renderer = renderer,
            ScreenPosition = screenPos,
            SequenceId = _counter++
        });
    }

    public void Compute(RootComponent root, ConsoleRenderer2D renderer)
    {
        TraverseComponentTree(root, renderer);
        var sortedAction = _queue
            .OrderBy(c => c.Layer)
            .ThenBy(c => c.ZIndex)
            .ThenBy(c => c.SequenceId);

        foreach (var action in sortedAction)
        {
            action.Component.Draw(renderer, action.ScreenPosition);
        }
        _queue.Clear();
        _counter = 0;
    }
    
    private void TraverseComponentTree(RootComponent root, ConsoleRenderer2D renderer)
    {
        WalkTree(root.Canvas, renderer);
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

        Submit(component, renderer, screenPos);

        foreach (var child in component.Children)
            WalkTree(child, renderer);
    }
}