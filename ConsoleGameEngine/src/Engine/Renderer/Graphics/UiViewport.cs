using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public sealed class UiViewport : Viewport
{
    public UiViewport()
    {
        Camera = null;
        Visible = true;
    }
    
    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
    }
}