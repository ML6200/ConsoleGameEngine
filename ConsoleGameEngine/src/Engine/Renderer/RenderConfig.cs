namespace ConsoleGameEngine.Engine.Renderer;

public static class RenderConfig
{
    public const bool EnablePerformanceMode =
#if PERFORMANCE_MODE
        true;
#else 
        false;
#endif
}