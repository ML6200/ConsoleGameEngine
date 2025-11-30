using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine;

public class ConsoleEngine : IEngineLifecycle, IDisposable
{
    private InputManager _inputManager;
    private readonly ConsoleRenderManager _renderManager;
    private readonly ConsoleRenderer2D _renderer;
    private readonly ConsoleWindowComponent _rootComponent;
    
    private Thread? _updateThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
    private bool _isInitialized;
    
    private IGameScene? _currentScene;
    private IGameScene? _pendingScene;
    private readonly object _sceneLock = new object();
    
    private int _targetUpdatesPerSecond = 60; // A jatek logikahoz
    private int _targetRenderFps = 60; // rendereléshez

    public int TargetUpdatesPerSecond
    {
        get => _targetUpdatesPerSecond;
        set { _targetUpdatesPerSecond = value != 0 ? value : 60; }
    }

    public int TargetRenderFPS
    {
        get => _targetRenderFps;
        set
        {
            _targetRenderFps = value != 0 ? value : 60;
            _renderManager?.SetTargetRenderFps(_targetRenderFps);
        }
        
    }

    private DateTime _lastUpdateTime;
    
    public InputManager Input => _inputManager;
    public ConsoleRenderer2D Renderer => _renderer;
    public ConsoleRenderManager RenderManager => _renderManager;
    public bool IsRunning => _isRunning;
    public IGameScene? CurrentScene => _currentScene;
    
    public double CurrentUpdateRate { get; private set; }


    public ConsoleEngine(int? windowWidth = null, int? windowHeight = null)
    {
        _inputManager = new InputManager();
        
        int width = windowWidth ?? Console.WindowWidth;
        int height = windowHeight ?? Console.WindowHeight;
        _renderer = new ConsoleRenderer2D(width, height);

        ConsoleGraphicsComponent rootPane = new ConsoleGraphicsPanel()
        {
            RelativePosition = new Position2D(0, 0),
            Size = new Dimension2D(width, height),
            HasBorder = false,
            Visible = true
        };
        
        _rootComponent = new ConsoleWindowComponent(rootPane);
        _renderManager = new ConsoleRenderManager(_renderer, _rootComponent, _targetUpdatesPerSecond);
    }
    
    public void Initialize()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("Engine already initialized");
        }
        
        Console.CursorVisible = false;
        Console.Clear();
        _renderManager.SubsribeFocusEventsToInput(_inputManager);
        _isInitialized = true;
    }

    public void OnStart()
    {
        if (!_isInitialized)
        {
            throw 
                new InvalidOperationException(
                    "Engine must be initialized " +
                    "before starting"
                );
        }

        if (_isRunning)
        {
            throw 
                new InvalidOperationException(
                    "Engine is already running"
                );
        }
        
        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        _renderManager.Start();

        _updateThread = new Thread(UpdateLoop)
        {
            IsBackground = true,
            Name = nameof(ConsoleEngine)
        };
        _updateThread.Start();

        if (_currentScene != null)
        {
            _currentScene.OnEnter();
        }
    }

    public void OnUpdate()
    {
        DateTime now = DateTime.Now;
        double deltaTime = (now - _lastUpdateTime).TotalMilliseconds / 1000.0;
        _lastUpdateTime = now;
        
        lock (_sceneLock)
        {
            if (_pendingScene != null)
            {
                _currentScene?.OnExit();
                _currentScene = _pendingScene;
                _pendingScene = null;
                _currentScene.OnEnter();
            }
        }
        
        // minden komponens frissitese
        GetRootPanel().Update(deltaTime);
        _currentScene?.OnUpdate(deltaTime);
        
    }
    
    public void LoadScene(IGameScene scene)
    {
        if (scene == null)
        {
            throw new ArgumentNullException(nameof(scene));
        }

        lock (_sceneLock)
        {
            _pendingScene = scene;
            _pendingScene.Initialize(this);
        }
    }

    public void Dispose()
    {
        Stop();
        _cancellationTokenSource?.Dispose();
        _renderManager.Dispose();
        _inputManager.Dispose();
    }
    
    Stopwatch timer = new Stopwatch();
    public void UpdateLoop()
    {
        if (!_isInitialized)
        {
            throw
                new InvalidOperationException(
                    "Engine must be initialized " +
                    "before starting update loop"
                );
        }

        while (_isRunning
               && !_cancellationTokenSource!.Token.IsCancellationRequested)
        { 
            long targetTicksPerUpdate = Stopwatch.Frequency / _targetUpdatesPerSecond;
            
            timer.Restart();

            OnUpdate();
            
            while (targetTicksPerUpdate > timer.ElapsedTicks)
            {
                if (targetTicksPerUpdate - timer.ElapsedTicks > 20_000)
                {
                    Thread.Sleep(1);
                }
            }
            
            double updateTime = timer.Elapsed.TotalMilliseconds;
            if (updateTime > 0)
            {
                CurrentUpdateRate = 1000.0D / updateTime;
            }
        }
    }
    
    public ConsoleGraphicsPanel GetRootPanel()
    {
        return (ConsoleGraphicsPanel)_rootComponent.ConsoleGraphicsComponent;
    }
    
    public void SetInitialScene(IGameScene scene)
    {
        if (_isRunning)
        {
            throw 
                new InvalidOperationException(
                    "Cannot set initial scene while " +
                    "engine is running. Use LoadScene instead."
                );
        }

        _currentScene = scene;
        _currentScene.Initialize(this);
    }
    
    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        // Wait for update thread to finish
        if (_updateThread != null && _updateThread.IsAlive)
        {
            _updateThread.Join(1000);
        }

        _renderManager.Stop();
        _currentScene?.OnExit();
        Console.CursorVisible = true;
        Console.Clear();
    }
}