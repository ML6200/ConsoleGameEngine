using System;
using System.Drawing;
using System.IO;
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
        Saved,
        Changed,
        ChangedHasPath,
    }
    
    private enum StateTrigger
    {
        ManualSave,
        Open,
        Exit,
        NoTrigger
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
    private UiLabel _title;
    private Cursor _cursor;
    private Mapper _mapper;

    private string _mapPath;
    private string _filePath = "";
    private bool _isSaved =  false;
    
    private EditorState _state = EditorState.Saved;
    private StateTrigger _stateTrigger = StateTrigger.NoTrigger;

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
         _mainPanel = new UiPanel()
         {
             RelativePosition = new Point2D(0, 0),
             BackgroundColor = ConsoleColor.Black,
             ForegroundColor = ConsoleColor.White,
             Size = _engine.RootPanel().ScreenSize,
             HasBorder = true,
             BorderColor = ConsoleColor.White,
         };
        _engine.RootPanel().AddChild(_mainPanel);
        
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;
        
        _title = new UiLabel()
        {
            Text = "Map Editor",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Black,
        };
        
        _title.RelativePosition = new Point2D(centerX - _title.Size.Width / 2, 0);
        _mainPanel.AddChild(_title);
        
        
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
            RelativePosition = new Point2D(1, 1),
            BackgroundColor = ConsoleColor.Black,
            ForegroundColor = ConsoleColor.White,
            Size = _mainPanel.Size - 2,
            HasBorder = false,
        };
        _mainPanel.AddChild(_editorPanel);
        
        _engine.Input.OnKeyPressed += HandleUserInput;
        _engine.Camera.CameraSize = _mainPanel.Size;

        _cursor = new Cursor(0, 0);
        _editorPanel.AddChild(_cursor);
        _mapper = new Mapper();
        
        _engine.RenderManager.OnWindowResized += (sender, args) =>
        {
            _mainPanel.Size = _engine.RootPanel().ScreenSize;
            _engine.Camera.CameraSize = _mainPanel.Size;
            
            _title.RelativePosition = new Point2D(_engine.RootPanel().ScreenSize.Width / 2
                                                  - _title.Size.Width / 2, 0);
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
                    _stateTrigger = StateTrigger.ManualSave;
                    HandleSave();   
                    break;
                case ConsoleKey.O:
                    _stateTrigger = StateTrigger.Open;
                    HandleOpen();
                    break;
            }
        }
    }

    private void MarkStateSaved(string filename)
    {
        _filePath = filename;
        _state = EditorState.Saved;
    }

    private void MarkStateUnsaved()
    {
        _state = string.IsNullOrEmpty(_filePath) ?  
            EditorState.Changed : EditorState.ChangedHasPath;
    }

    private void PlaceCursorOnTop()
    {
        if (!_isEntityAdded) return;

        _editorPanel.RemoveChild(_cursor);
        _editorPanel.AddChild(_cursor);
        _isEntityAdded = false;
    }

    private void OpenMap(string filename)
    {
        // better to handle file existence and format mismatch in the mapper later
        if (File.Exists(filename))
        {
            _state = EditorState.Saved;
            _filePath = filename;
            ReloadMap();
            EnableEditor();
        }
        else
        {
            UiMsgBox msgBox = new UiMsgBox(_engine.RootPanel(),
                _engine.RenderManager, _engine.Input,
                "Failed to load", $"File '{filename}' not found");

            msgBox.OnComplete += result =>
            {
                EnableEditor();
            };
        }
    }

    private void EnableEditor(bool readdCursor = true)
    {
        if (readdCursor) _editorPanel.AddChild(_cursor);
        _engine.Input.OnKeyPressed += HandleUserInput;
    }
    
    private void DisableEditor()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.Input.OnKeyPressed -= HandleUserInput;
    }

    private void ReloadMap()
    {
        _editorPanel.RemoveAllChildren();
        _mapper.ClearObjects();
        _mapper.LoadFromDcmfFile(_filePath);
        
        foreach (var dcm in _mapper.DcmList)
        {
            _editorPanel.AddChild(dcm.Value);
        }
        _editorPanel.AddChild(_cursor);
    }

    private void SaveMap(string filename)
    {
        if (_state is EditorState.ChangedHasPath)
        {
            _mapper.SaveMap(filename);
            ReloadMap();
            
            MarkStateSaved(filename);
        }
        
        if (_stateTrigger is StateTrigger.Exit 
            && _state is EditorState.Saved)
        {
            _engine.LoadScene(new MainMenuScene(DoomGameManager.DefaultMapPath));
            return;
        }

        if (_stateTrigger is StateTrigger.Open 
            && _state is EditorState.Saved)
        {
            HandleOpen();
        }
        
        EnableEditor(false);
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
        Point2D targetPoint = _cursor.RelativePosition;
        if (_mapper.IsPositionAcquired(targetPoint))
            return;
        
        _mapper.AddObject(targetPoint, dmType, entity);
        _editorPanel.AddChild(_mapper.DcmList[^1].Value); 
        _isEntityAdded =  true;
        MarkStateUnsaved();
    }

    private void RemoveEntity(Point2D point)
    {
        var removed = _mapper.RemoveObject(point);
        if (removed != null)
        {
            _editorPanel.RemoveChild(removed);
            _isSaved = false;
        }
        
        _isEntityAdded = false;
        MarkStateUnsaved();
    }
    
    private void MoveCursorBy(int x, int y)
    {
        Point2D targetPoint = _cursor.RelativePosition + new Point2D(x, y);
        _cursor.RelativePosition = targetPoint.Clamp(new Point2D(0, 0), 
            _mainPanel.Size - 3);
        PlaceCursorOnTop();
    }

    private void HandleExit()
    {
        _stateTrigger = StateTrigger.Exit;
        HandleSave();
    }

    private void HandleOpen()
    {
        DisableEditor();
        if (_state is not EditorState.Saved)
        {
            _stateTrigger = StateTrigger.Open;
            HandleUnsavedDialog();
        }
        else 
            HandleOpenDialog();
    }

    private void HandleOpenDialog()
    {
        UiInputBox msgBox2 = new UiInputBox(_editorPanel,
            _engine.RenderManager, _engine.Input,
            "Enter the path of the file to be opened:", "");
            
        msgBox2.OnOk += OpenMap;
        msgBox2.OnCancelled += (sender, args) => 
        {
            EnableEditor(false);
        };
    }
    
    private void HandleSave()
    {
        DisableEditor();
        
        if (_state is EditorState.Changed)
        {
            if (_stateTrigger is StateTrigger.Exit or StateTrigger.Open)
                HandleUnsavedDialog();
            else
                HandleSaveDialog();
        } else if (_state is EditorState.ChangedHasPath)
        {
            HandleUnsavedDialog();
        }
        else
            SaveMap(_filePath);
    }
    
    private void HandleUnsavedDialog()
    {
        UiMsgBox msgBox = new UiMsgBox(_editorPanel,
            _engine.RenderManager, _engine.Input,
            "You have unsaved work!", "Do you want to save your work? " +
                     "If you hit cancel all changes WILL BE LOST!");

        msgBox.OnComplete += result =>
        {
            if (result == MessageOptionState.Ok)
            {
                if (_stateTrigger is StateTrigger.Exit 
                    && _state is EditorState.ChangedHasPath)
                    SaveMap(_filePath);
                else
                    HandleSaveDialog();
            }
            else
            {
                if (_stateTrigger is StateTrigger.Exit)
                {
                    _state = EditorState.Saved;
                    SaveMap(_filePath);
                } else if (_stateTrigger is StateTrigger.Open)
                {
                    _state = EditorState.Saved;
                    HandleOpen();
                }
                else EnableEditor(false);
            }
        };
    }

    private void HandleSaveDialog()
    {
        DisableEditor();
        
        UiInputBox inpBox = new UiInputBox(_editorPanel, 
            _engine.RenderManager, _engine.Input,
            "Enter the path of the file to be saved:", "");
        inpBox.OnOk += SaveMap;
        inpBox.OnCancelled +=  (sender, args) => EnableEditor(false);
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
            Point2D screenPos = camera.TransformPoint(WorldPosition);
            if (screenPos == Point2D.OutsideScreenPoint) return; // Off-screen culling

            renderer.SetCell(screenPos.X, screenPos.Y,
                new Cell(_glyph, ConsoleColor.Black, ConsoleColor.Green));
        }
    }
}