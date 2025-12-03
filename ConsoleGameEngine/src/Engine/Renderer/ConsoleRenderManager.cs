using System;
using System.Diagnostics;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager : IDisposable
{
    private readonly Lock _graphicsLock = new();
    private Thread _graphicsThread;
    private CancellationTokenSource _cts;
    private ConsoleRenderer2D _renderer;
    private ConsoleWindowComponent _rootComponent;
    private int _updatesPerSecond;
    
    public double CurrentFps {get; private set; }
    public FocusManager FocusManager { get; set; }
    
    public event EventHandler OnWindowResized;

    public ConsoleRenderManager(ConsoleRenderer2D renderer, ConsoleWindowComponent rootComponent, int updatesPerSecond)
    {
        _renderer = renderer;
        //_renderer.InitRenderer();
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
        if (_graphicsThread != null && _graphicsThread.IsAlive)
        {
            return;
        }
        
        _cts = new CancellationTokenSource();
        _graphicsThread = new Thread(() => RenderLoop(_cts.Token))
        {
            Name = nameof(ConsoleRenderManager),
            IsBackground = true,
        };
        
        _graphicsThread.Start();
    }

    public void Stop()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            _graphicsThread.Join();
            //windowEventThread.Join();
            _cts.Dispose();
        }
    }

    public void SetRootComponent(ConsoleWindowComponent rootComponent)
    {
        lock (_graphicsLock)
        {
            _rootComponent = rootComponent;
        }
    }
    
    private readonly Stopwatch _timer = new Stopwatch();
    private void RenderLoop(CancellationToken ct)
    {
        long targetTicksPerFrame = Stopwatch.Frequency / _updatesPerSecond;
        
        while (!ct.IsCancellationRequested)
        {
            _timer.Restart();
            
            if (IsWindowResized())
            {
                _renderer.SetDimension(Console.WindowWidth, Console.WindowHeight);
                OnWindowResized?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ConsoleWindowComponent root = Volatile.Read(ref _rootComponent);
                _renderer.FlushBuffer();
                root.Compute(_renderer);
                _renderer.Render();
            }

            while (targetTicksPerFrame > _timer.ElapsedTicks)
            {
                if (targetTicksPerFrame - _timer.ElapsedTicks > 20_000)
                {
                    Thread.Sleep(1);
                }
            }

            double frameTime = _timer.Elapsed.TotalMilliseconds;
            if (frameTime > 0)
            {
                CurrentFps = 1000.0D / frameTime;
            }
        }
    }

    public void SetTargetRenderFps(int fps)
    {
        _updatesPerSecond = fps;
    }

    private bool IsWindowResized()
    {
        return _renderer.ScreenWidth != Console.WindowWidth 
               || _renderer.ScreenHeight != Console.WindowHeight;
    }
    
    public void Dispose()
    {
        Stop();
    }
}