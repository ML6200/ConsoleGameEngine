using System.Threading;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager : IDisposable
{
    private Thread windowEventThread;
    private Thread graphicsThread;
    private ConsoleRenderer2D _renderer;
    private bool _isRunning;

    public ConsoleRenderManager(ConsoleRenderer2D renderer)
    {
        _renderer = renderer;
        _renderer.InitRenderer();
        
        graphicsThread = new Thread(RenderLoop);
        graphicsThread.Start();
        
        windowEventThread = new Thread(RenderLoop);
    }

    public void RenderAll()
    {
        _renderer.Render();
    }

    private void RenderLoop()
    {
        while (_isRunning)
        {
            _renderer.Render();
        }
    }
    
    public void Dispose()
    {
        _isRunning = false;
        windowEventThread.Interrupt();
        graphicsThread.Interrupt();
    }
}