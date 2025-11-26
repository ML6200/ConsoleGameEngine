using System;
using ConsoleGameEngine.Engine.Renderer.Animations;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ConsoleProgressBar : ConsoleGraphicsComponent
{
    private float _progress = 0;

    public void SetProgress(float progress, double animationDuration = 0.5)
    {
        float startProgress = _progress;

        // Animate progress change
        AddAnimation(new Animation(animationDuration, t =>
        {
            _progress = startProgress + (progress - startProgress) * t;
        }));
    }

    public override void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        int filledWidth = (int)(Size.Width * _progress);

        // Draw filled part
        renderer.FillRect(
            AbsolutePosition.X,
            AbsolutePosition.Y,
            filledWidth,
            Size.Height,
            '█',
            ConsoleColor.Green,
            ConsoleColor.White
        );

        // Draw empty part
        renderer.FillRect(
            AbsolutePosition.X + filledWidth,
            AbsolutePosition.Y,
            Size.Width - filledWidth,
            Size.Height,
            '░',
            ConsoleColor.DarkGray,
            ConsoleColor.Gray
        );
    } 
}