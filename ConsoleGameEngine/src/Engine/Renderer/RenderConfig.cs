namespace ConsoleGameEngine.Engine.Renderer;

public abstract class RenderConfig
{
    public static bool EnablePerformanceMode =
#if PERFORMANCE_MODE
        true;
#else 
        false;
#endif
}