using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public sealed class GameViewport : Viewport
{
    public GameViewport(ConsoleCamera camera)
    {
        Camera = camera ?? throw new ArgumentNullException(nameof(camera));;
        Visible = true;
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
    }

    protected override void UpdateSelf(double deltaTime)
    {
        Camera?.Follow(deltaTime);
    }
}