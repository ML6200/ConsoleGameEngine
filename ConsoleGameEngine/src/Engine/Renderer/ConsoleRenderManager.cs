using System;
using System.Diagnostics;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using NLog;

namespace ConsoleGameEngine.Engine.Renderer;

public class ConsoleRenderManager : IDisposable
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly RootComponent _rootCanvas;
    private Thread _graphicsThread;
    private CancellationTokenSource _cts;
    private ConsoleRenderer2D _renderer;
    private ConsoleCamera _camera;
    private int _updatesPerSecond;
    
    public double CurrentFps {get; private set; }
    
    public event EventHandler OnWindowResized;
    
    private RenderPipeline _renderPipeline;

    public ConsoleRenderManager(ConsoleEngine engine, int updatesPerSecond)
    {
        _renderer = new ConsoleRenderer2D(engine.ScreenSize);
        _renderPipeline = new RenderPipeline();
        _renderPipeline.Camera = engine.Camera;
        _rootCanvas = engine.RootComponent;
        _updatesPerSecond = updatesPerSecond;
    }

    public void Start()
    {
        if (_graphicsThread != null && _graphicsThread.IsAlive)
        {
            _logger.Warn("Cant start process." +
                         "Process is already running.");
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

                // Update root panel size to match new window size
                _rootCanvas.Canvas.Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight);
                OnWindowResized?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _renderPipeline.ComputeComponentTree(_rootCanvas.Canvas, _renderer);
                _renderPipeline.Compute(_renderer);
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
        _renderer.Dispose();
    }
}