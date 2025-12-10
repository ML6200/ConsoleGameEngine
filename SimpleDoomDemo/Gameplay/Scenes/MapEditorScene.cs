using System;
using System.Drawing;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MapEditorScene : IGameScene
{
    /*
     * +-----------------------------------
     * |
     * |           C
     * |
     * |   E  E
     * |   EEEEEEE
     * |
     * |
     *
     * C: Cursor
     * E: Placed Object
     *
     */
    private ConsoleEngine _engine;
    private UiPanel _toolBarPanel;
    private UiPanel _editorPanel;
    private UiLabel _placeHolder;
    private UiButton _backButton;
    private Cursor _cursor;
    private Mapper _mapper;
    
    private string _mapPath;
    private bool _isModified = false;
    private bool _isOpenPending = false;

    public MapEditorScene(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
    }

    public void OnEnter()
    {
        _toolBarPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            BackgroundColor = ConsoleColor.Gray,
            ForegroundColor = ConsoleColor.Black,
            Size = new Dimension2D(_engine.RootPanel().ScreenSize.Width, 5),
            HasBorder = true
        };
        _editorPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            BackgroundColor = ConsoleColor.Black,
            ForegroundColor = ConsoleColor.White,
            Size = new Dimension2D(_engine.RootPanel().ScreenSize.Width,
                _engine.RootPanel().ScreenSize.Height),
            HasBorder = false
        };
        _engine.RootPanel().AddChild(_editorPanel);
        //_engine.RootPanel().AddChild(_toolBarPanel);
        
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;
        
        _placeHolder = new UiLabel()
        {
            Text = "Map Editor",
            ForegroundColor = ConsoleColor.Black,
            BackgroundColor = ConsoleColor.Gray,
        };
        _placeHolder.RelativePosition = new Point2D(centerX - _placeHolder.Size.Width / 2, 0);
        _toolBarPanel.AddChild(_placeHolder);

        _backButton = new UiButton()
        {
            Text = "Back",
            RelativePosition = new Point2D(centerX, centerY - 10),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.DarkRed,
            ForegroundColor = ConsoleColor.White,
        };
        
        //_backButton.OnClick += HandleBack;
        //_toolBarPanel.AddChild(_backButton);
        
        _engine.RenderManager.FocusManager.Register(_backButton);
        _engine.Input.OnKeyPressed += HandleUserInput;

        _cursor = new Cursor(0, 0);
        _editorPanel.AddChild(_cursor);
        _mapper = new Mapper();
    }

    private void HandleUserInput(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case ConsoleKey.LeftArrow:
                MoveCursorBy(-1, 0);
                break;
            case ConsoleKey.RightArrow:
                MoveCursorBy(1, 0);
                break;
            case ConsoleKey.UpArrow:
                MoveCursorBy(0, -1);
                break;
            case ConsoleKey.DownArrow:
                MoveCursorBy(0, 1);
                break;
            
            case ConsoleKey.W:
                AddEntity(Mapper.DcmEntity.Wall, Mapper.DcmType.GameItem);
                break;
            case ConsoleKey.T:
                AddEntity(Mapper.DcmEntity.ToxicWaste, Mapper.DcmType.GameItem);
                break;
            case ConsoleKey.A:
                AddEntity(Mapper.DcmEntity.Ammo, Mapper.DcmType.GameItem);
                break;
            case ConsoleKey.M:
                AddEntity(Mapper.DcmEntity.MedKit, Mapper.DcmType.GameItem);
                break;
            case ConsoleKey.B:
                AddEntity(Mapper.DcmEntity.BfgCell, Mapper.DcmType.GameItem);
                break;
            case ConsoleKey.D:
                AddEntity(Mapper.DcmEntity.Door, Mapper.DcmType.GameItem);
                break;
            
            case ConsoleKey.Z:
                AddEntity(Mapper.DcmEntity.Zombieman, Mapper.DcmType.Demon);
                break;
            case ConsoleKey.C:
                AddEntity(Mapper.DcmEntity.Zombieman, Mapper.DcmType.Demon);
                break;
            case ConsoleKey.I:
                AddEntity(Mapper.DcmEntity.Imp, Mapper.DcmType.Demon);
                break;
            
            case ConsoleKey.P:
                AddEntity(Mapper.DcmEntity.Player, Mapper.DcmType.Player);
                break;
            case ConsoleKey.S:
                HandleBack();
                break;
            case ConsoleKey.O:
                HandleOpen();
                break;
            case ConsoleKey.Backspace:
                RemoveEntity(_cursor.WorldPosition);
                break;
        }
    }

    private void OpenMap(string filename)
    {
        _engine.RootPanel().RemoveAllChildren();
        _mapper.DcmList.Clear();
        _mapper.LoadFromDcmfFile(filename);
        _engine.RootPanel().AddChild(_editorPanel);
        
        foreach (var dcm in _mapper.DcmList)
        {
            _editorPanel.AddChild(dcm.Value);
        }
        _editorPanel.AddChild(_cursor);
        
        _engine.Input.OnKeyPressed += HandleUserInput;
        
    }
    
    private void SaveMap(string filename)
    {
        _mapper.SaveMap(filename);
        _mapper.ClearObjects();
        if (!_isOpenPending)
            _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
        else HandleOpen();
    }
    
    /*
     * PROBLEM:
     * If we add entities to the pane the cursor gets hidden
     * at the position because its added earlier in the child list
     *
     * We could later solve this by creating a layer manager that can
     * prioritize the list based on given rules but for that we would
     * need to refactor the component class to be able to pass objects
     * down or up in the tree, however this would be more complex and may
     * not worth the effort yet
     *
     * for simplicity we could also create a priority modifier for each child
     * so we could set priority as HIGH or LOW.
     * Meaning that if we set the priority to high for an element it would always
     * end up in the end of the list meaning its more "up" in hierarchy
     *
     * Like:
     *
     * Children:             Layer(C) = HIGH
     * ----------            =>
     * A      ->             A
     * C      ->             B
     * B      ->             E
     * E      ->             D
     * D      ->             C
     *
     * Therefore its trivial that we set more to high priority
     * the last setting would be the highest
     * 
     */
    private void AddEntity(Mapper.DcmEntity entity, Mapper.DcmType dmType)
    {
        Point2D targetPoint = _cursor.WorldPosition;
        if (_mapper.IsPositionAcquired(targetPoint))
            return;
        _editorPanel.RemoveChild(_cursor);
        _mapper.AddObject(targetPoint, dmType, entity);
        _editorPanel.AddChild(_mapper.DcmList[^1].Value); 
        _editorPanel.AddChild(_cursor);
        _isModified = true;
    }

    private void RemoveEntity(Point2D point)
    {
        var removed = _mapper.RemoveObject(point);
        if (removed != null) _editorPanel.RemoveChild(removed);
    }
    
    private void MoveCursorBy(int x, int y)
    {
        Point2D targetPoint = _cursor.WorldPosition + new Point2D(x, y);
        _cursor.RelativePosition = targetPoint;
    }

    private void HandleOpen()
    {
        _isOpenPending = true;
        if (!_isModified)
        {
            _engine.Input.OnKeyPressed -= HandleUserInput;
            UiInputBox msgBox2 = new UiInputBox(_engine.RootPanel(),
                _engine.RenderManager, _engine.Input,
                "Do you want to save your work?", "");
            msgBox2.OnComplete += OpenMap;
        } else HandleBack();
    }

    private void HandleBack()
    {
        _engine.Input.OnKeyPressed -= HandleUserInput;
        UiMsgBox msgBox = new UiMsgBox(_engine.RootPanel(), _engine.RenderManager, _engine.Input,
            "Save?", "Do you want to save your work? " +
                     "If you hit cancel all changes WILL BE LOST!");
        msgBox.OnComplete += state =>
        {
            _isModified = false;
            _engine.RenderManager.FocusManager.UnregisterAll();
            if (state == MessageOptionState.Ok)
            {
                if (_mapper.DcmList.Count > 0)
                {
                    UiInputBox msgBox2 = new UiInputBox(_engine.RootPanel(), 
                        _engine.RenderManager, _engine.Input,
                        "Do you want to save your work?", "");
                    msgBox2.OnComplete += SaveMap;
                }
            }
            else
            {
                if (!_isOpenPending) 
                    _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
                else
                    HandleOpen();
            }
        };
    }

    public void OnUpdate(double deltaTime)
    {
    }

    public void OnExit()
    {
        _engine.RenderManager.FocusManager.Unregister(_backButton);
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.RootPanel().RemoveAllChildren();
    }

    class Cursor : GraphicsComponent
    {
        public Cursor(int x, int y)
        {
            RelativePosition = new Point2D(x, y);
        }

        protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
        {
            Point2D? screenPos = camera.TransformPoint(WorldPosition);
            if (screenPos == null) return; // Off-screen culling

            renderer.SetCell(screenPos.X, screenPos.Y,
                new Cell('⊡', ConsoleColor.Black, ConsoleColor.Green));
        }
    }
}