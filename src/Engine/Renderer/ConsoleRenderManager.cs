using System.Threading;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager
{
    private Thread graphicsThread;
    private ConsoleRenderer2D _renderer;

    public ConsoleRenderManager(ConsoleRenderer2D renderer)
    {
        _renderer = renderer;
        _renderer.InitRenderer();
        
        graphicsThread = new Thread(Run);
        graphicsThread.Start();
    }

    public void Start()
    {
        _renderer.Render();
    }

    private void Run()
    {
        _renderer.Render();
    }
}