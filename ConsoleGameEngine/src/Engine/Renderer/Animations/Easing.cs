using System;

namespace ConsoleGameEngine.Engine.Renderer.Animations;

public static class Easing
{
    public static float Linear(float t) => t;
    public static float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
    public static float EaseInOutQuad(float t) =>
        t < 0.5f ? 2 * t * t : 1 - (float)Math.Pow(-2 * t + 2, 2) / 2;
    public static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (t < 1 / d1)
            return n1 * t * t;
        if (t < 2 / d1)
            return n1 * (t -= 1.5f / d1) * t + 0.75f;
        if (t < 2.5 / d1)
            return n1 * (t -= 2.25f / d1) * t + 0.9375f;
        return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }
}