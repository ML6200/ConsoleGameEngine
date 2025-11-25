using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer.Animation;

public static class AnimationTween
{
    public static Animation MoveTo(ConsoleGraphicsComponent target, Position2D end, double duration)
    {
        Position2D start = target.RelativePosition;
        return new Animation(duration, progress =>
        {
            float t = Easing.EaseOutQuad(progress);
            int x = Lerp(start.X, end.X, t);
            int y = Lerp(start.Y, end.Y, t);
            target.RelativePosition = new Position2D(x, y);
        });
    }

    public static Animation Blink(ConsoleGraphicsComponent target, double interval)
    {
        return new Animation(interval, progress =>
        {
            target.Visible = progress < 0.5f;
        })
        {
            Loop = true
        };
    }

    public static Animation FadeColor(ConsoleGraphicsButton button, ConsoleColor from, ConsoleColor to, double duration)
    {
        return new Animation(duration, progress =>
        {
            button.NormalBgColor = progress < 0.5f ? from : to;
        });
    }

    public static Animation Progress(Action<float> onProgress, double duration)
    {
        return new Animation(duration, onProgress);
    }

    private static int Lerp(int a, int b, float t) => (int)(a + (b - a) * t);
}
