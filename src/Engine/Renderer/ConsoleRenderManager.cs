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
        
        windowEventThread = new Thread(WindowEventLoop);
        windowEventThread.Start();
    }
    
    private void RenderLoop()
    {
        while (_isRunning)
        {
            _renderer.Update();
        }
    }

    private void WindowEventLoop()
    {
        while (_isRunning)
        {
            if (IsWindowResized())
            {
                _renderer.setDimension(Console.WindowWidth, Console.WindowHeight);
                onWindowResized?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool IsWindowResized()
    {
        return _renderer.Width != Console.WindowWidth 
               || _renderer.Height != Console.WindowHeight;
    }
    
    public void Dispose()
    {
        _isRunning = false;
        windowEventThread.Interrupt();
        graphicsThread.Interrupt();
    }
}