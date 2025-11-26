using System;

namespace ConsoleGameEngine.Engine.Renderer.Animations;

public class Animation
{
    private double _elapsedTime = 0;
    private Action<float> _onUpdate;
    private Action _onComplete;
    
    public double Duration { get; set; }
    public bool Loop { get; set; }
    public bool IsComplete => !Loop && _elapsedTime >= Duration;

    public Animation(double duration, Action<float> onUpdate)
    {
        Duration = duration;
        _onUpdate = onUpdate;
    }

    public Animation OnComplete(Action callback)
    {
        _onComplete = callback;
        return this;
    }

    public void OnUpdate(double deltaTime)
    {
        if (IsComplete) return;

        _elapsedTime += deltaTime;
        if (_elapsedTime >= Duration)
        {
            if (Loop)
            {
                _elapsedTime %= Duration;
            }
            else
            {
                _elapsedTime = Duration;
                _onComplete?.Invoke();
            }
        }
        float progress = (float) Math.Min(1f, _elapsedTime / Duration);
        _onUpdate?.Invoke(progress);
    }
}