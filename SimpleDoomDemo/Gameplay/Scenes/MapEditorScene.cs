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
    private enum EditorState
    {
        Editing,
        SavingForExit,
        SavingForOpen,
        SavingManual,
        OpeningFile,
        Unchanged
    }
    
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
    private UiPanel _mainPanel;
    private UiPanel _toolBarPanel;
    private UiPanel _editorPanel;
    private UiLabel _placeHolder;
    private UiButton _backButton;
    private Cursor _cursor;
    private Mapper _mapper;
    
    private string _mapPath;
    private EditorState _state = EditorState.Unchanged;
    private string _filePath = "";

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
        // _mainPanel = new UiPanel()
        // {
        //     RelativePosition = new Point2D(0, 0),
        //     BackgroundColor = ConsoleColor.Black,
        //     ForegroundColor = ConsoleColor.White,
        //     Size = _engine.RootPanel().ScreenSize,
        //     HasBorder = true,
        //     BorderColor = ConsoleColor.White
        // };
        //_engine.RootPanel().AddChild(_mainPanel);
        //_engine.RootPanel().AddChild(_editorPanel);
        
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
            Size = _engine.RootPanel().Size,
            HasBorder = false,
        };
        //_mainPanel.AddChild(_editorPanel);
        _engine.RootPanel().AddChild(_editorPanel);
        
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;
        
        _placeHolder = new UiLabel()
        {
            Text = "Map Editor",
            ForegroundColor = ConsoleColor.Black,
            BackgroundColor = ConsoleColor.Gray,
        };
        _placeHolder.RelativePosition = new Point2D(centerX - _placeHolder.Size.Width / 2, 0);
        //_toolBarPanel.AddChild(_placeHolder);

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
        _engine.Camera.CameraSize = _engine.RootPanel().ScreenSize;

        _cursor = new Cursor(0, 0);
        _editorPanel.AddChild(_cursor);
        _mapper = new Mapper();
        _engine.RenderManager.OnWindowResized += (sender, args) =>
        {
            _editorPanel.Size = _engine.RootPanel().ScreenSize;
            _engine.Camera.CameraSize = _engine.RootPanel().ScreenSize;
        };
    }

    private bool _isEntityAdded;
    
    private void HandleUserInput(object? sender, KeyEventArgs e)
    {
        // Mono Keys
        switch (e.Key)
        {
            // Movement
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
            
            // Game Items
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
            
            // Demons
            case ConsoleKey.Z:
                AddEntity(Mapper.DcmEntity.Zombieman, Mapper.DcmType.Demon);
                break;
            case ConsoleKey.C:
                AddEntity(Mapper.DcmEntity.Mancubus, Mapper.DcmType.Demon);
                break;
            case ConsoleKey.I:
                AddEntity(Mapper.DcmEntity.Imp, Mapper.DcmType.Demon);
                break;
            case ConsoleKey.P:
                AddEntity(Mapper.DcmEntity.Player, Mapper.DcmType.Player);
                break;
            
            // Controls
            case ConsoleKey.Escape:
                HandleExit();
                break;
            case ConsoleKey.Backspace:
                RemoveEntity(_cursor.WorldPosition);
                break;
        }

        // Keybindings
        if (e.Control)
        {
            switch (e.Key)
            {
                case ConsoleKey.S:
                    _state = EditorState.SavingManual;
                    HandleSave();   
                    break;
                case ConsoleKey.O:
                    HandleOpen();
                    break;
            }
        }
    }

    private void PlaceCursorOnTop()
    {
        if (!_isEntityAdded) return;

        _editorPanel.RemoveChild(_cursor);
        _editorPanel.AddChild(_cursor);
        _isEntityAdded = false;
        _state = EditorState.Editing;
    }

    private void OpenMap(string filename)
    {
        _state = EditorState.OpeningFile;
        _filePath = filename;
        ReloadMap();
        _editorPanel.AddChild(_cursor);
        _engine.Input.OnKeyPressed += HandleUserInput;
    }

    private void ReloadMap()
    {
        _editorPanel.RemoveAllChildren();
        _mapper.DcmList.Clear();
        _mapper.LoadFromDcmfFile(_filePath);
        
        foreach (var dcm in _mapper.DcmList)
        {
            _editorPanel.AddChild(dcm.Value);
        }
        _editorPanel.AddChild(_cursor);
    }

    private void SaveMap(string filename)
    {
        if (_state is EditorState.SavingForExit)
        {
            _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
            return;
        }
        
        _mapper.SaveMap(filename);
        _mapper.ClearObjects();
        _filePath = filename;
        _state = EditorState.SavingManual;
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
        
        _mapper.AddObject(targetPoint, dmType, entity);
        _editorPanel.AddChild(_mapper.DcmList[^1].Value); 
        _isEntityAdded =  true;
        _state = EditorState.Editing;
    }

    private void RemoveEntity(Point2D point)
    {
        var removed = _mapper.RemoveObject(point);
        if (removed != null) _editorPanel.RemoveChild(removed);
        
        _isEntityAdded = false;
        _state = EditorState.Editing;
    }
    
    private void MoveCursorBy(int x, int y)
    {
        Point2D targetPoint = _cursor.WorldPosition + new Point2D(x, y);
        _cursor.RelativePosition = targetPoint;
        PlaceCursorOnTop();
    }

    private void HandleExit()
    {
        _state = EditorState.SavingForExit;
        HandleSave();
    }

    private void HandleOpen()
    {
        _state = EditorState.SavingForOpen;
        
        if (_state is EditorState.Editing)
            HandleUnsaved();
        else 
            HandleOpenDialog();
    }

    private void HandleOpenDialog()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.Input.OnKeyPressed -= HandleUserInput;
        
        UiInputBox msgBox2 = new UiInputBox(_engine.RootPanel(),
            _engine.RenderManager, _engine.Input,
            "Enter the path of the file to be opened:", "");
            
        msgBox2.OnOk += OpenMap;
        msgBox2.OnCancelled += (sender, args) => 
        {
            _state = EditorState.Editing;
            _engine.Input.OnKeyPressed += HandleUserInput;
        };
    }
    
    private void HandleSave()
    {
        if (_filePath.Equals(String.Empty) &&
            _isEntityAdded) // (_state is not EditorState.SavingManual && _isEntityAdded)
        {
            _engine.RenderManager.FocusManager.UnregisterAll();
            _engine.Input.OnKeyPressed -= HandleUserInput;

            if (_state is EditorState.SavingForExit or EditorState.SavingForOpen)
                HandleUnsaved();
            else
                HandleSaveDialog();
        }
        else
            SaveMap(_filePath);
    }
    
    private void HandleUnsaved()
    {
        UiMsgBox msgBox = new UiMsgBox(_engine.RootPanel(),
            _engine.RenderManager, _engine.Input,
            "Save?", "Do you want to save your work? " +
                     "If you hit cancel all changes WILL BE LOST!");

        msgBox.OnComplete += state =>
        {
            _engine.RenderManager.FocusManager.UnregisterAll();
            _engine.Input.OnKeyPressed -= HandleUserInput;
            
            if (state == MessageOptionState.Ok)
                HandleSaveDialog();
            else
                _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
        };
    }

    private void HandleSaveDialog()
    {
        UiInputBox inpBox = new UiInputBox(_engine.RootPanel(), 
            _engine.RenderManager, _engine.Input,
            "Enter the path of the file", "");
        inpBox.OnOk += s =>
        {
            SaveMap(s);
            
            if (_state is EditorState.OpeningFile) HandleOpen();
            _engine.Input.OnKeyPressed += HandleUserInput;
        };
        inpBox.OnCancelled += SaveAborted;
    }

    private void SaveAborted(object? sender, EventArgs e)
    {
        _engine.Input.OnKeyPressed += HandleUserInput;
    }
    
    public void OnUpdate(double deltaTime)
    {
        // Not used
    }

    public void OnExit()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.RootPanel().RemoveAllChildren();
    }

    class Cursor : GraphicsComponent
    {
        public Cursor(int x, int y)
        {
            RelativePosition = new Point2D(x, y);
        }

        private readonly char _glyph  = '⊡';

        protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
        {
            Point2D? screenPos = camera.TransformPoint(WorldPosition);
            if (screenPos == null) return; // Off-screen culling

            renderer.SetCell(screenPos.X, screenPos.Y,
                new Cell(_glyph, ConsoleColor.Black, ConsoleColor.Green));
        }
    }
}