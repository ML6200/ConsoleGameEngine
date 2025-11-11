using System;
using System.Threading;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine;

public class ConsoleEngine : IEngineLifecycle, IDisposable
{
    
    private readonly InputManager _inputManager;
    private readonly ConsoleRenderManager _renderManager;
    private readonly ConsoleRenderer2D _renderer;
    private readonly ConsoleWindowComponent _window;
    
    private Thread? _updateThread;
    private CancellationTokenSource? _cancellationTokenSource;
    private bool _isRunning;
    private bool _isInitialized;
    
    private IGameScene? _currentScene;
    private IGameScene? _pendingScene;
    private readonly object _sceneLock = new object();
    
    
    private const int TargetUpdatesPerSecond = 60;
    private const double TargetUpdateInterval = 1000.0 / TargetUpdatesPerSecond;
    private DateTime _lastUpdateTime;
    
    public InputManager Input => _inputManager;
    public ConsoleRenderer2D Renderer => _renderer;
    public bool IsRunning => _isRunning;
    public IGameScene? CurrentScene => _currentScene;


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
        
        _window = new ConsoleWindowComponent(rootPane);
        _renderManager = new ConsoleRenderManager(_renderer, _window);
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
    }

    public void OnStart()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Engine must be initialized before starting");
        }

        if (_isRunning)
        {
            throw new InvalidOperationException("Engine is already running");
        }
        
        _isRunning = true;
        _cancellationTokenSource = new CancellationTokenSource();
        
        _renderManager.Start();

        _updateThread = new Thread(UpdateLoop)
        {
            IsBackground = true,
            Name = nameof(ConsoleEngine)
        };

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
    
    public void UpdateLoop()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("Engine must be initialized before disposing");
        }
    }
    
    public ConsoleGraphicsPanel GetRootPanel()
    {
        return (ConsoleGraphicsPanel)_window.ConsoleGraphicsComponent;
    }
    
    public void SetInitialScene(IGameScene scene)
    {
        if (_isRunning)
        {
            throw new InvalidOperationException("Cannot set initial scene while engine is running. Use LoadScene instead.");
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