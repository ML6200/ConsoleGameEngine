
using System;
using System.Security.AccessControl;
using System.Threading;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Animation;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Demo;

class Program
{
    static void Main(string[] args)
    {
        UiEngineTest();
    }
    
    static void RenderTester()
    {
        var renderer = new ConsoleRenderer2D(Console.WindowWidth, Console.WindowHeight);
        while (true)
        {
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawBox(2, 1, 30, 6);
            renderer.DrawText(4, 3, "Hello world.");
            renderer.Render();
            Thread.Sleep(500);
            
            renderer.DrawBox(2, 1, 30, 6);
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawText(4, 3, "Hello world..");
            renderer.Render();
            Thread.Sleep(500);
            
            renderer.DrawBox(2, 1, 30, 6);
            renderer.FillRect(2, 1, 30, 6, ' ', ConsoleColor.Green);
            renderer.DrawText(4, 3, "Hello world...");
            renderer.Render();
            Thread.Sleep(500);
        }
    }

    
    /*
    static void UiTester()
    {
        InputManager _inputManager = new InputManager();
        ConsoleRenderer2D _renderer = new ConsoleRenderer2D(Console.WindowWidth, Console.WindowHeight);
        
        ConsoleGraphicsPanel rootGraphicsPanel = new ConsoleGraphicsPanel 
        { 
            RelativePosition = new Position2D(0, 0),
            Size = new Dimension2D(Console.WindowWidth, Console.WindowHeight),
            HasBorder = false
        };
        ConsoleWindowComponent consoleWindowComponent = new ConsoleWindowComponent(rootGraphicsPanel);
        consoleWindowComponent.Visible = true;
        
        ConsoleRenderManager renderManager = new ConsoleRenderManager(_renderer, consoleWindowComponent);
        
        var title = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(30, 2),
            Text = "SIMPLE DOOM ENGINE",
            ForegroundColor = ConsoleColor.Red
        };
        
        var panel = new ConsoleGraphicsPanel
        {
            RelativePosition = new Position2D(10, 5),
            Size = new Dimension2D(60, 15),
            BackgroundColor = ConsoleColor.DarkBlue,
            BorderColor = ConsoleColor.Cyan
        };
        var text = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(0, 0),
            Text = "Sprite",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkGray,
            Visible = true
        };
        
        var panel2 = new ConsoleGraphicsPanel
        {
            RelativePosition = new Position2D(20, 10),
            Size = new Dimension2D(9, 5),
            BackgroundColor = ConsoleColor.DarkGray,
            BorderColor = ConsoleColor.Black
        };
        
        var info = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(12, 6),
            Text = "Press Enter to exit",
            ForegroundColor = ConsoleColor.White
        };
        bool _isRunning = true;

        var label = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(0, 0),
            Text = "Hello World!",
            ForegroundColor = ConsoleColor.Green,
            Visible = false
        };
        panel2.AddChild(text);
        panel.AddChild(panel2);
        
        rootGraphicsPanel.AddChild(title);
        rootGraphicsPanel.AddChild(panel);
        panel.AddChild(info);
        panel.AddChild(label);
        
        _inputManager.OnEnterPressed += (s, e) => _isRunning = false;
        _inputManager.OnKeyPressed += (s, e) =>
        {
            if (e.Key == ConsoleKey.E)
            {
                label.Visible = !label.Visible;
            } else if (e.Key == ConsoleKey.A)
            {
                panel2.Visible = !panel2.Visible;
            } else if (e.Key == ConsoleKey.RightArrow)
            {
                panel2.RelativePosition.X += 1;
                //text.AbsolutePosition.X += 1;
                
            } else if (e.Key == ConsoleKey.LeftArrow)
            {
                panel2.RelativePosition.X -= 1;
                //text.AbsolutePosition.X -= 1;
            }
            else if (e.Key == ConsoleKey.UpArrow)
            {
                panel2.RelativePosition.Y -= 1;
                //text.AbsolutePosition.Y -= 1;
            }
            else if (e.Key == ConsoleKey.DownArrow)
            {
                panel2.RelativePosition.Y += 1;
                //text.AbsolutePosition.Y += 1;
            }
        };
        
        renderManager.Start();
    
        
        while (_isRunning)
        {
            // var currentTime = DateTime.Now;
            // var deltaTime = (currentTime - lastTime).TotalSeconds;
            // lastTime = currentTime;
            
            // rootGraphicsPanel.Update();
            
            // rootGraphicsPanel.Render(_renderer);
            //consoleWindowComponent.Render(_renderer);
            //_renderer.Update();
            // rootGraphicsPanel.Render(_renderer);
            // renderManager.RenderAll();
            // _renderer.Render();
        
            // Cap framerate (~60 FPS)
            Thread.Sleep(10);
        }
        
        //renderManager.Dispose();
        rootGraphicsPanel.Dispose();
    }
    */

    static void EngineTest()
    {
        var engine = new ConsoleEngine();
        engine.Initialize();
        engine.SetInitialScene(new GameScene());
        engine.OnStart();

        // Keep main thread alive while engine runs
        while (engine.IsRunning)
        {
            Thread.Sleep(100);
        }
    }

    static void UiEngineTest()
    {
        var engine = new ConsoleEngine();
        engine.TargetUpdatesPerSecond = 100;
        engine.Initialize();
        engine.SetInitialScene(new SimpleScene());
        engine.OnStart();

        // Keep main thread alive while engine runs
        while (engine.IsRunning)
        {
            Thread.Sleep(100);
        }
    }
    
}

class SimpleScene : IGameScene
{
    ConsoleEngine? _engine;
    ConsoleGraphicsLabel? _titleLabel;
    ConsoleGraphicsLabel? _instructionLabel;
    ConsoleGraphicsPanel? _rootPanel;
    ConsoleGraphicsPanel? panel, spritePanel2;
    private ConsoleGraphicsPanel? spritePanel;

    private int _spriteX = 0;
    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
        
        var root = consoleEngine.GetRootPanel();
        
        var title = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(30, 2),
            Text = "SIMPLE DOOM ENGINE",
            ForegroundColor = ConsoleColor.Red
        };
        
        panel = new ConsoleGraphicsPanel
        {
            RelativePosition = new Position2D(10, 5),
            Size = new Dimension2D(60, 15),
            BackgroundColor = ConsoleColor.DarkBlue,
            BorderColor = ConsoleColor.Cyan
        };
        var text = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(0, 0),
            Text = "Sprite",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkGray,
            Visible = true
        };

        spritePanel = new ConsoleGraphicsPanel
        {
            RelativePosition = new Position2D(20, 10),
            Size = new Dimension2D(9, 5),
            BackgroundColor = ConsoleColor.DarkGray,
            BorderColor = ConsoleColor.Black
        };

        var text2 = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(0, 0),
            Text = "Sprite",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.DarkGray,
            Visible = true
        };

        spritePanel2 = new ConsoleGraphicsPanel
        {
            RelativePosition = new Position2D(5, 10),
            Size = new Dimension2D(9, 5),
            BackgroundColor = ConsoleColor.DarkGray,
            BorderColor = ConsoleColor.Black
        };
        
        var info = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(12, 6),
            Text = "Press ESC to exit",
            ForegroundColor = ConsoleColor.White
        };
        bool _isRunning = true;

        var label = new ConsoleGraphicsLabel
        {
            RelativePosition = new Position2D(0, 0),
            Text = "Hello World!",
            ForegroundColor = ConsoleColor.Green,
            Visible = false
        };
        
        var startButton = new ConsoleGraphicsButton
        {
            Text = "Start",
            RelativePosition = new Position2D(30, 10),
            Size = new Dimension2D(9, 3),
        };
        
        var button = new ConsoleGraphicsButton("Click")
        {
            RelativePosition = new Position2D(50, 10),
            Size = new Dimension2D(0, 1),
            HasBorder = false
        };
        
        /*
        startButton.OnClick += (s, e) =>
        {
            var originalY = button.RelativePosition.Y;

            button.AddAnimation(
                AnimationTween.MoveTo(button, new Position2D(10, originalY + 1), 0.1)
                    .OnComplete(() =>
                    {
                        button.AddAnimation(AnimationTween.MoveTo(button, new Position2D(30, originalY), 0.1));
                    })
            );
            
            button.AddAnimation(
                AnimationTween.FadeColor(button, ConsoleColor.White, button.NormalBgColor, 0.2)
            );
        };
        */
        
        
        button.OnClick += (s, e) =>
        {
            var originalY = button.RelativePosition.Y;


            button.AddAnimation(
                AnimationTween.MoveTo(button, new Position2D(30, originalY + 1), 0.1)
                    .OnComplete(() =>
                    {
                        button.AddAnimation(AnimationTween.MoveTo(button, new Position2D(30, originalY), 0.1));
                    })
            );
            
            
            button.AddAnimation(
                AnimationTween.FadeColor(button, ConsoleColor.White, button.NormalBgColor, 0.2)
            );
        };
        
        //startButton.OnClick += (s, e) => _engine.LoadScene(new GameScene());
        _engine.RenderManager.FocusManager.Register(startButton);
        _engine.RenderManager.FocusManager.Register(button);
        
        root.AddChild(title);
        root.AddChild(panel);
        panel.AddChild(info);
        panel.AddChild(label);
        root.AddChild(startButton);
        root.AddChild(button);
        
        spritePanel.AddChild(text);
        panel.AddChild(spritePanel);
        
        //consoleEngine.Renderer.DrawText(1, 2, "Hello World!");
        root.AddChild(spritePanel2);
        
        consoleEngine.Input.OnEscapePressed += OnEscapePressed;
        consoleEngine.Input.OnKeyPressed += OnKeyPressed;
    }
    

    public void OnEnter()
    {
    }

    public void OnUpdate(double deltaTime)
    {
        spritePanel2.RelativePosition.X += 1;
    }
    
    private void OnKeyPressed(object? sender, KeyEventArgs e)
    {
        // Move player
        switch (e.Key)
        {
            case ConsoleKey.UpArrow:
                spritePanel.RelativePosition.Y -= 1;
                break;

            case ConsoleKey.DownArrow:
               spritePanel.RelativePosition.Y += 1;
                break;
            
            case ConsoleKey.LeftArrow:
                spritePanel.RelativePosition.X += -1;
                break;
            
            case ConsoleKey.RightArrow:
                spritePanel.RelativePosition.X += 1;
                break;
            case ConsoleKey.E:
                spritePanel.Visible = !spritePanel.Visible;
                break;
        }
    }


    public void OnExit()
    {
        if (_engine != null)
        {
            _engine.Input.OnSpacePressed -= OnEscapePressed;
        }
    }
    
    private void OnEscapePressed(object? sender, EventArgs eventArgs)
    {
        _engine?.Stop();
    }
}


 class MenuScene : IGameScene
  {
      private ConsoleEngine? _engine;
      private ConsoleGraphicsLabel? _titleLabel;
      private ConsoleGraphicsLabel? _instructionLabel;

      public void Initialize(ConsoleEngine engine)
      {
          _engine = engine;

          // Get root panel
          var root = engine.GetRootPanel();

          // Create title
          _titleLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = new Position2D(30, 5),
              Text = "=== CONSOLE GAME ===",
              ForegroundColor = ConsoleColor.Cyan
          };

          // Create instructions
          _instructionLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = new Position2D(25, 10),
              Text = "Press SPACE to start | ESC to quit",
              ForegroundColor = ConsoleColor.White
          };
 
          
          root.AddChild(_titleLabel);
          root.AddChild(_instructionLabel);
          

          // Subscribe to input
          engine.Input.OnSpacePressed += OnSpacePressed;
          engine.Input.OnEscapePressed += OnEscapePressed;
      }

      public void OnEnter()
      {
          // Scene activated
      }

      public void OnUpdate(double deltaTime)
      {
          // Could animate title here, e.g., blinking effect
      }

      public void OnExit()
      {
          // Cleanup
          if (_engine != null)
          {
              _engine.Input.OnSpacePressed -= OnSpacePressed;
              _engine.Input.OnEscapePressed -= OnEscapePressed;
          }
      }

      private void OnSpacePressed(object? sender, EventArgs eventArgs)
      {
          // Start game
          _engine?.LoadScene(new GameScene());
      }

      private void OnEscapePressed(object? sender, EventArgs eventArgs)
      {
          _engine?.Stop();
      }
  }

  // ========================================
  // Game Scene
  // ========================================

  class GameScene : IGameScene
  {
      private ConsoleEngine? _engine;
      private ConsoleGraphicsLabel? _playerLabel;
      private ConsoleGraphicsLabel? _scoreLabel;
      private ConsoleGraphicsLabel? _enemyLabel;

      private Position2D _playerPosition;
      private Position2D _enemyPosition;
      private int _score;
      private double _enemySpeed = 5.0; // cells per second

      public void Initialize(ConsoleEngine engine)
      {
          _engine = engine;
          var root = engine.GetRootPanel();

          // Initial positions
          _playerPosition = new Position2D(10, 15);
          _enemyPosition = new Position2D(70, 15);

          // Create player
          _playerLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = _playerPosition,
              Text = "@",
              ForegroundColor = ConsoleColor.Green
          };

          // Create enemy
          _enemyLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = _enemyPosition,
              Text = "X",
              ForegroundColor = ConsoleColor.Red
          };

          // Create score display
          _scoreLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = new Position2D(2, 1),
              Text = "Score: 0",
              ForegroundColor = ConsoleColor.Yellow
          };

          // Create game area panel
          var gamePanel = new ConsoleGraphicsPanel
          {
              RelativePosition = new Position2D(5, 5),
              Size = new Dimension2D(70, 20),
              HasBorder = true,
              BorderColor = ConsoleColor.DarkGray
          };

          root.AddChild(gamePanel);
          root.AddChild(_scoreLabel);
          root.AddChild(_playerLabel);
          root.AddChild(_enemyLabel);

          // Subscribe to input
          engine.Input.OnKeyPressed += OnKeyPressed;
      }

      public void OnEnter()
      {
          _score = 0;
          UpdateScore();
      }

      public void OnUpdate(double deltaTime)
      {
          // Move enemy towards player
          if (_enemyPosition.X > _playerPosition.X)
          {
              _enemyPosition = new Position2D(
                  _enemyPosition.X - (int)(_enemySpeed * deltaTime),
                  _enemyPosition.Y
              );
              _enemyLabel!.RelativePosition = _enemyPosition;
          }

          // Check collision
          if (_enemyPosition.X <= _playerPosition.X)
          {
              // Game over!
              _engine?.LoadScene(new GameOverScene(_score));
          }
      }

      public void OnExit()
      {
          if (_engine != null)
          {
              _engine.Input.OnKeyPressed -= OnKeyPressed;
          }
      }

      private void OnKeyPressed(object? sender, KeyEventArgs e)
      {
          // Move player
          switch (e.Key)
          {
              case ConsoleKey.UpArrow:
                  _playerPosition = new Position2D(_playerPosition.X, _playerPosition.Y - 1);
                  _playerLabel!.RelativePosition = _playerPosition;
                  break;

              case ConsoleKey.DownArrow:
                  _playerPosition = new Position2D(_playerPosition.X, _playerPosition.Y + 1);
                  _playerLabel!.RelativePosition = _playerPosition;
                  break;

              case ConsoleKey.Spacebar:
                  // Shoot! Reset enemy and gain points
                  _enemyPosition = new Position2D(70, _playerPosition.Y);
                  _enemyLabel!.RelativePosition = _enemyPosition;
                  _score += 10;
                  UpdateScore();
                  break;

              case ConsoleKey.Escape:
                  _engine?.LoadScene(new MenuScene());
                  break;
          }
      }

      private void UpdateScore()
      {
          _scoreLabel!.Text = $"Score: {_score}";
      }
  }

  // ========================================
  // Game Over Scene
  // ========================================

  class GameOverScene : IGameScene
  {
      private readonly int _finalScore;
      private ConsoleEngine? _engine;

      public GameOverScene(int finalScore)
      {
          _finalScore = finalScore;
      }

      public void Initialize(ConsoleEngine engine)
      {
          _engine = engine;
          var root = engine.GetRootPanel();

          // Game over panel
          var panel = new ConsoleGraphicsPanel
          {
              RelativePosition = new Position2D(20, 10),
              Size = new Dimension2D(40, 10),
              HasBorder = true,
              BorderColor = ConsoleColor.Red,
              BackgroundColor = ConsoleColor.DarkRed
          };
          
          int w = panel.Size.Width;
          int h = panel.Size.Height;

          var gameOverLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = new Position2D(w/2-5, h/2 -5),
              Text = "GAME OVER",
              ForegroundColor = ConsoleColor.White
          };

          var scoreLabel = new ConsoleGraphicsLabel
          {
              RelativePosition = new Position2D(3, h/2 -3),
              Text = $"Final Score: {_finalScore}",
              ForegroundColor = ConsoleColor.Yellow
          };

          var restartLabel = new ConsoleGraphicsLabel   
          {
              RelativePosition = new Position2D(3, h/2),
              Text = "Press R to restart | ESC to menu",
              ForegroundColor = ConsoleColor.Gray
          };
          
          panel.AddChild(gameOverLabel);
          panel.AddChild(scoreLabel);
          panel.AddChild(restartLabel);

          root.AddChild(panel);

          engine.Input.OnKeyPressed += OnKeyPressed;
      }

      public void OnEnter() { }

      public void OnUpdate(double deltaTime) { }

      public void OnExit()
      {
          if (_engine != null)
          {
              _engine.Input.OnKeyPressed -= OnKeyPressed;
          }
      }

      private void OnKeyPressed(object? sender, KeyEventArgs e)
      {
          if (e.Key == ConsoleKey.R)
          {
              _engine?.LoadScene(new GameScene());
          }
          else if (e.Key == ConsoleKey.Escape)
          {
              _engine?.LoadScene(new MenuScene());
          }
      }
  }
