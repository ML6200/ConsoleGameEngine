using System;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager : IDisposable
{
    private Thread windowEventThread;
    private Thread graphicsThread;
    private readonly object graphicsLock = new();
    private CancellationTokenSource _cts;
    private ConsoleRenderer2D _renderer;
    private ConsoleWindowComponent _rootComponent;
    private int _updatesPerSecond;
    
    public FocusManager FocusManager { get; set; }

    public event EventHandler OnWindowResized;

    public ConsoleRenderManager(ConsoleRenderer2D renderer, ConsoleWindowComponent rootComponent, int updatesPerSecond)
    {
        _renderer = renderer;
        _renderer.InitRenderer();
        _rootComponent = rootComponent;
        _updatesPerSecond = updatesPerSecond;
        
        FocusManager = new FocusManager();
    }

    public void SubsribeFocusEventsToInput(InputManager inputManager)
    {
        inputManager.OnKeyPressed += HandleFocusInput;
        inputManager.OnEnterPressed += (s, e) => FocusManager.ActivateFocused();
    }

    private void HandleFocusInput(object? sender, KeyEventArgs e)
    {
        if (e.Key == ConsoleKey.Tab)
        {
            if (e.Shift)
            {
                FocusManager.FocusPrevious();
            }
            else
            {
                FocusManager.FocusNext();
            }
        }
    }

    public void Start()
    {
        if (graphicsThread != null && graphicsThread.IsAlive)
        {
            return;
        }
        
        _cts = new CancellationTokenSource();
        graphicsThread = new Thread(() => RenderLoop(_cts.Token))
        {
            Name = nameof(ConsoleRenderManager),
            IsBackground = true,
        };
        
        graphicsThread.Start();

        windowEventThread = new Thread(() => WindowEventLoop(_cts.Token))
        {
            Name = nameof(WindowEventLoop),
            IsBackground = true,
        };
        windowEventThread.Start();
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            graphicsThread.Join();
            windowEventThread.Join();
            _cts.Dispose();
        }
    }

    public void SetRootComponent(ConsoleWindowComponent rootComponent)
    {
        lock (graphicsLock)
        {
            _rootComponent = rootComponent;
        }
    }
    
    private void RenderLoop(CancellationToken ct)
    {
        double targetFrameTime = 1.0 / _updatesPerSecond;
        
        while (!ct.IsCancellationRequested)
        {
            DateTime frameStartTime = DateTime.Now;
            
            ConsoleWindowComponent root;
            lock (graphicsLock)
            {
                root = _rootComponent;
            }
            
            _renderer.Clear();
            root?.Render(_renderer);
            _renderer.Render();
            
            DateTime frameEndTime = DateTime.Now;
            double elapsedTime = (frameEndTime - frameStartTime).TotalSeconds;
            double sleepTime = targetFrameTime - elapsedTime;

            if (sleepTime > 0)
            {
                Thread.Sleep((int)(sleepTime * 1000));
            }
        }
    }

    private void WindowEventLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (IsWindowResized())
            {
                _renderer.SetDimension(Console.WindowWidth, Console.WindowHeight);
                OnWindowResized?.Invoke(this, EventArgs.Empty);
            }
            
            Thread.Sleep(100);
        }
    }

    private bool IsWindowResized()
    {
        return _renderer.Width != Console.WindowWidth 
               || _renderer.Height != Console.WindowHeight;
    }
    
    public void Dispose()
    {
        Stop();
    }
}