using System;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class ProgressBar : GraphicsComponent
{
    private float _progress = 0;

    public void SetProgress(float progress, double animationDuration = 0.5)
    {
        if (animationDuration <= 0)
        {
            // Set immediately without animation
            _progress = progress;
            return;
        }

        float startProgress = _progress;

        // elorehaladas animalasa
        AddAnimation(new Animation(animationDuration, t =>
        {
            _progress = startProgress + (progress - startProgress) * t;
        }));
    }

    public override void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        // UI progress bars render directly at world position (no camera transformation)
        int filledWidth = (int)(Size.Width * _progress);

        // Draw empty part
        var emptyStyle = new RenderStyle(AnsiColor.DarkGray, AnsiColor.Gray, FontStyle.Regular);
        renderer.FillRect(
            WorldPosition.X + filledWidth,
            WorldPosition.Y,
            Size.Width - filledWidth,
            Size.Height,
            '░',
            emptyStyle
        );

        // Draw filled part
        var filledStyle = new RenderStyle(AnsiColor.Green, AnsiColor.White, FontStyle.Regular);
        renderer.FillRect(
            WorldPosition.X,
            WorldPosition.Y,
            filledWidth,
            Size.Height,
            '█',
            filledStyle
        );
    } 
}