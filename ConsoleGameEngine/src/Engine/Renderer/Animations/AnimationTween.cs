using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer.Animations;

public static class AnimationTween
{
    public static Animation MoveTo(GraphicsComponent target, Point2D end, double duration)
    {
        Point2D start = target.RelativePosition;
        return new Animation(duration, progress =>
        {
            float t = Easing.EaseOutQuad(progress);
            int x = LerpForScalar(start.X, end.X, t);
            int y = LerpForScalar(start.Y, end.Y, t);
            target.RelativePosition = new Point2D(x, y);
        });
    }

    public static Animation Blink(GraphicsComponent target, double interval, bool loop = true)
    {
        bool visible = target.Visible;
        return new Animation(interval, progress =>
        {
            if (loop) target.Visible = progress < 0.5f;
            else target.Visible = progress >= 0.5f;
        })
        {
            Loop = loop
        }.OnComplete(()=>
        {
            if (!loop) target.Visible = visible;
        });
    }

    public static Animation FadeColor(UiButton button, ConsoleColor from, ConsoleColor to, double duration)
    {
        return new Animation(duration, progress =>
        {
            button.BackgroundColor = progress < 0.5f ? from : to;
        });
    }

    public static Animation Progress(Action<float> onProgress, double duration)
    {
        return new Animation(duration, onProgress);
    }

    // Linear interpolation for smooth animations
    /*
     * Linear interpolation between two points:
     *
     * (x-x0/y-y0) = (y2-y1)/(x2-x1)
     *
     * Where (x,y) are the interpolated coordinates
     * 
     * But since we apply it by each coordinate:
     * For a scalar axial component "x": x = x0 + t*(x1-x0)
     * or more precisely: x = (1-t) * v0 + t * v1
     * 
     */
    private static int LerpForScalar(int v0, int v1, float t)
    {
        // SOURCE: https://en.wikipedia.org/wiki/Linear_interpolation
        return (int) (v0 * (1 - t)+  v1 * t);
    }
}
