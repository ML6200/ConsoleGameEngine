using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using ConsoleGameEngine.Engine.System;
using NLog;

namespace ConsoleGameEngine.Engine;

public class ConsoleEngine : IEngineLifecycle, IDisposable
{
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private InputManager _inputManager;
    
    private Thread? _updateThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
    private bool _isInitialized;
    
    private IGameScene? _currentScene;
    private IGameScene? _pendingScene;
    
    private int _targetUpdatesPerSecond = 60; // A jatek logikahoz
    private int _targetRenderFps = 60; // rendereléshez
    
    private Monitoring _monitoring;
    private StatsPanel _statsPanel;
    
    private readonly ConsoleRenderManager _renderManager;
    private readonly RootComponent _rootComponent;
    private readonly Lock _sceneLock = new();
    private readonly ManualResetEvent _exitEvent = new(false);
    
    private readonly Queue<double> _updateSamples = new();
    private readonly Queue<double> _renderSamples = new();

    public Monitoring Monitoring => _monitoring;
    
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);
    
    public RootComponent RootComponent => _rootComponent;

    public int TargetUpdatesPerSecond
    {
        get => _targetUpdatesPerSecond;
        set => _targetUpdatesPerSecond = value != 0 ? value : 60;
    }

    public int TargetRenderFps
    {
        get => _targetRenderFps;
        set
        {
            _targetRenderFps = value != 0 ? value : 60;
            _renderManager?.SetTargetRenderFps(_targetRenderFps);
        }
        
    }

    public UiManager UiManager { get; private set; }
    public InputManager Input => _inputManager;
    public ConsoleRenderManager RenderManager => _renderManager;
    public bool IsRunning => _isRunning;
    public IGameScene? CurrentScene => _currentScene;
    public ConsoleCamera Camera {get; set;}
    
    public double CurrentUpdateRate { get; private set; }
    

    public double GetAverageUpdateRate()
    {
        double current = CurrentUpdateRate;
        _updateSamples.Enqueue(current);

        if (_updateSamples.Count > _targetUpdatesPerSecond)
            _updateSamples.Dequeue();
        
        return current;
    }
    
    public double GetAverageFrameRate()
    {
        double current = _renderManager.CurrentFps;
        _renderSamples.Enqueue(current);

        if (_renderSamples.Count > _targetRenderFps)
            _renderSamples.Dequeue();
        
        return current;
    }
    
    private void AddStats()
    {
        _statsPanel = new StatsPanel(this, "Press-X-to-hide")
        {
            RelativePosition = new Point2D(0, 0)
        };
        RootPanel().AddChild(_statsPanel);
    }

    public ConsoleEngine()
    {
        _inputManager = new InputManager();
        
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        GraphicsComponent rootPane = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(width, height),
            HasBorder = false,
            Visible = true
        };
        
        UiManager = new UiManager(_inputManager);
        _rootComponent = new RootComponent(rootPane);

        // Connect UiManager as observer to the component tree
        _rootComponent.Canvas.SetObserver(UiManager);

        // Initialize camera after _rootComponent is created
        Camera = new ConsoleCamera(this,
            new Dimension2D(width, height),  
            new Point2D(0, 0),
            new Dimension2D(width, height)
        );

        _renderManager = new ConsoleRenderManager(this, _targetUpdatesPerSecond);
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            throw new InvalidOperationException("Engine already initialized");
        }
        
        Console.CursorVisible = false;
        Console.Clear();
        _isInitialized = true;
        _logger.Info("Engine initialized");
        _monitoring = new(_targetUpdatesPerSecond);
        
        AddStats();

        var channel = KeyBinding.Parse("X");
        _inputManager.Register(channel);
        _inputManager.Subscribe(channel, HandleInput);
    }

    private void HandleInput()
    {
        _statsPanel.Visible = !_statsPanel.Visible;
    }

    public void OnStart()
    {
        if (!_isInitialized)
        {
            const string message = "Engine must be initialized before starting";
            _logger.Error(message);
            throw 
                new InvalidOperationException(message);
        }

        if (_isRunning)
        {
            const string message = "Engine is already running";
            _logger.Error(message);
            throw new InvalidOperationException(message);
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

    private readonly Stopwatch _updateDeltaTimer = Stopwatch.StartNew();
    public void OnUpdate()
    {
        _monitoring.StartTimer();
        double deltaTime = _updateDeltaTimer.Elapsed.TotalSeconds;
        _updateDeltaTimer.Restart();
        
        lock (_sceneLock)
        {
            if (_pendingScene != null)
            {
                _currentScene?.OnExit();
                _currentScene = _pendingScene;
                _pendingScene = null;
                _currentScene.OnEnter();
                
                RootPanel().RemoveChild(_statsPanel);
                RootPanel().AddChild(_statsPanel);
            }
        }
        Camera.Follow(deltaTime);
        // update all components
        _rootComponent.Update(deltaTime);
        _currentScene?.OnUpdate(deltaTime);
        _monitoring.StopTimer();
    }
    
    public void LoadScene(IGameScene? scene)
    {
        if (scene == null)
        {
            _logger.Error("Scene cannot be null.");
            throw new ArgumentNullException(nameof(scene));
        }

        lock (_sceneLock)
        {
            _pendingScene = scene;
            _pendingScene.Initialize(this);
        }
    }
    
    private readonly Stopwatch _updateTimer = Stopwatch.StartNew();

    private void UpdateLoop()
    {
        if (!_isInitialized)
        {
            const string message = "Engine must be initialized " +
                                   "before starting update loop.";
            _logger.Error(message);
            throw new InvalidOperationException(message);
        }

        while (_isRunning
               && !_cancellationTokenSource!.Token.IsCancellationRequested)
        { 
            long targetTicksPerUpdate = Stopwatch.Frequency / _targetUpdatesPerSecond;
            
            _updateTimer.Restart();

            OnUpdate();
            
            while (targetTicksPerUpdate > _updateTimer.ElapsedTicks)
            {
                if (targetTicksPerUpdate - _updateTimer.ElapsedTicks > 20_000)
                {
                    Thread.Sleep(1);
                }
            }
            
            double updateTime = _updateTimer.Elapsed.TotalMilliseconds;
            if (updateTime > 0)
            {
                CurrentUpdateRate = 1000.0D / updateTime;
            }
        }
        _exitEvent.Set();
    }
    
    public UiPanel RootPanel()
    {
        return (UiPanel)_rootComponent.Canvas;
    }
    
    public void SetInitialScene(IGameScene scene)
    {
        if (_isRunning)
        {
            const string message = "Cannot set initial scene while " +
                                   "engine is running. Use LoadScene instead.";
            _logger.Error(message);
            throw new InvalidOperationException(message);
        }

        _currentScene = scene;
        _currentScene.Initialize(this);
    }
    
    public void Stop()
    {
        if (!_isRunning)
        {
            _logger.Warn("Cant stop engine. Engine is not running.");
            return;
        }

        _isRunning = false;
        _cancellationTokenSource?.Cancel();

        // timeout for canceled thread
        if (_updateThread != null && _updateThread.IsAlive)
        {
            _updateThread.Join(1000);
        }

        _renderManager.Stop();
        _currentScene?.OnExit();
        _inputManager.Dispose();
    }
    
    public void WaitForExit()
    {
        _exitEvent.WaitOne(); 
    }

    public void Dispose()
    {
        
        Stop();
        _cancellationTokenSource?.Dispose();
        _renderManager.Dispose();
        _inputManager.Dispose();
    }
}