using System;
using System.Threading;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager : IDisposable
{
    private Thread windowEventThread;
    private Thread graphicsThread;
    private ConsoleRenderer2D _renderer;
    private bool _isRunning;

    public event EventHandler onWindowResized;

    public ConsoleRenderManager(ConsoleRenderer2D renderer)
    {
        _isRunning = true;
        _renderer = renderer;
        _renderer.InitRenderer();
        
        graphicsThread = new Thread(RenderLoop);
        graphicsThread.Start();
        
        windowEventThread = new Thread(RenderLoop);
        windowEventThread.Start();
    }
    
    private void RenderLoop()
    {
        while (_isRunning)
        {
            int x = _renderer.Width;
            int y = _renderer.Height;

            if (Console.WindowWidth != x || Console.WindowHeight != y)
            {
                _renderer.setDimension(Console.WindowWidth, Console.WindowHeight);
            }
        }
    }
    
    public void Dispose()
    {
        _isRunning = false;
        windowEventThread.Interrupt();
        graphicsThread.Interrupt();
    }
}