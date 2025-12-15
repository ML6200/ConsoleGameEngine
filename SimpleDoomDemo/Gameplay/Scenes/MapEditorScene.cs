using System;
using System.IO;
using System.Linq;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using ConsoleGameEngine.Engine.System;
using NLog;
using SimpleDoomDemo.Gameplay.Scenes.Exceptions;

namespace SimpleDoomDemo.Gameplay.Scenes;

internal enum EditorState
{
    Saved,
    Changed,
    ChangedHasPath,
}

internal enum StateTrigger
{
    ManualSave,
    Open,
    Exit,
    NoTrigger
}

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
    private UiPanel _mainPanel;
    private MapToolbar _toolBarPanel;
    private UiPanel _editorPanel;
    private UiLabel _title;
    private Cursor _cursor;
    private StatusBar _statusBar;
    
    private MapParser _mapParser;

    private string _mapPath;
    private string _filePath = "";
    private bool _isLegacy = false;

    private EditorState _state = EditorState.Saved;
    private StateTrigger _stateTrigger = StateTrigger.NoTrigger;

    private Logger _logger = LogManager.GetCurrentClassLogger();

    private void SetState(EditorState newState)
    {
        _state = newState;
        string stateName = "";
        switch (_state)
        {
            case EditorState.Saved:
                stateName = "Saved";
                break;
            case EditorState.Changed:
            case EditorState.ChangedHasPath:
                stateName = "Changed";
                break;
        }
        
        _statusBar.SetStateLabel(stateName);
    }

    public MapEditorScene()
    {
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
    }

    public void OnEnter()
    {
        const int offset = 2;
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

        _title = new UiLabel()
        {
            Text = "Map Editor",
            ForegroundColor = ConsoleColor.White,
            BackgroundColor = ConsoleColor.Black,
        };

        _title.RelativePosition = new Point2D(centerX - _title.Size.Width / 2, 0);
        _mainPanel.AddChild(_title);


        _toolBarPanel = new MapToolbar();
        _toolBarPanel.RelativePosition = new Point2D(centerX - 50,
            _mainPanel.Size.Height - _toolBarPanel.Size.Height - offset);

        _editorPanel = new UiPanel()
        {
            RelativePosition = new Point2D(1, 1),
            BackgroundColor = ConsoleColor.Black,
            ForegroundColor = ConsoleColor.White,
            Size = _mainPanel.Size - offset,
            HasBorder = false,
        };
        _mainPanel.AddChild(_editorPanel);
        _editorPanel.AddChild(_toolBarPanel);

        _engine.Input.OnKeyPressed += HandleUserInput;
        _engine.Camera.CameraSize = _mainPanel.Size;
        _engine.RootPanel().RelativePosition = new Point2D(0, 0);

        _cursor = new Cursor(0, 0);
        _editorPanel.AddChild(_cursor);
        _mapParser = new MapParser();


        _statusBar = new StatusBar(_cursor.RelativePosition)
        {
            RelativePosition = new Point2D(2, 0)
        };
        _mainPanel.AddChild(_statusBar);

        _engine.RenderManager.OnWindowResized += (sender, args) =>
        {
            _mainPanel.Size = _engine.RootPanel().ScreenSize;
            _engine.Camera.CameraSize = _mainPanel.Size;

            _title.RelativePosition = new Point2D(_engine.RootPanel().ScreenSize.Width / 2
                                                  - _title.Size.Width / 2, 0);

            _toolBarPanel.RelativePosition = new Point2D(_engine.RootPanel().ScreenSize.Width / 2 - 50,
                _mainPanel.Size.Height - _toolBarPanel.Size.Height - offset);
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
                AddEntity(MapParser.DcmEntity.Wall, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.T:
                AddEntity(MapParser.DcmEntity.ToxicWaste, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.A:
                AddEntity(MapParser.DcmEntity.Ammo, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.M:
                AddEntity(MapParser.DcmEntity.MedKit, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.B:
                AddEntity(MapParser.DcmEntity.BfgCell, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.D:
                AddEntity(MapParser.DcmEntity.Door, MapParser.DcmType.GameItem);
                break;
            case ConsoleKey.E:
                AddEntity(MapParser.DcmEntity.LevelExit, MapParser.DcmType.GameItem);
                break;

            // Demons
            case ConsoleKey.Z:
                AddEntity(MapParser.DcmEntity.Zombieman, MapParser.DcmType.Demon);
                break;
            case ConsoleKey.C:
                if (e.Shift)
                    AddEntity(MapParser.DcmEntity.Mancubus, MapParser.DcmType.Demon);
                break;
            case ConsoleKey.I:
                AddEntity(MapParser.DcmEntity.Imp, MapParser.DcmType.Demon);
                break;
            case ConsoleKey.P:
                AddEntity(MapParser.DcmEntity.Player, MapParser.DcmType.Player);
                break;

            // Hide or show toolbar panel
            case ConsoleKey.H:
                _toolBarPanel.Visible = !_toolBarPanel.Visible;
                break;

            // Controls
            case ConsoleKey.Escape:
                HandleExit();
                break;
            case ConsoleKey.Backspace:
                RemoveEntity(_cursor.RelativePosition);
                break;
        }

        if (e.Shift)
        {
            switch (e.Key)
            {
                case ConsoleKey.O:
                    OptimizeMap();
                    break;
                case ConsoleKey.L:
                    _isLegacy = true;
                    HandleOpen();
                    break;
            }
        }

        bool mask = true;
        if (SystemInfo.Os.IsWindows())
        {
            mask = e.Alt;
        } 

        // Keybindings
        if (e.Control && mask)
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

    private void OptimizeMap()
    {
        if (_state is EditorState.ChangedHasPath or EditorState.Saved)
        {
            int count = _mapParser.Optimize();
            UiMsgBox msgBox = new UiMsgBox(_mainPanel,
                _engine.RenderManager,
                _engine.Input,
                "Map optimization",
                $"Map optimization complete:\nFound {count} duplicates.");
            msgBox.OnComplete += option =>
            {
                SetState(EditorState.ChangedHasPath);
                ReloadMap(false);
            };
        }
    }

    private void MarkStateSaved(string filename)
    {
        _filePath = filename;
        SetState(EditorState.Saved);
    }

    private void MarkStateUnsaved()
    {
        SetState(string.IsNullOrEmpty(_filePath) ? 
            EditorState.Changed : EditorState.ChangedHasPath);
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
        try
        {
            SetState(!_isLegacy ? EditorState.Saved : EditorState.Changed);

            _filePath = filename;
            ReloadMap();
            EnableEditor();
            _stateTrigger = StateTrigger.NoTrigger;
        }
        catch (Exception e)
        {
            if (e is PlayerNotFoundException or LevelExitNotFoundException)
            {
                ReloadMap();
                EnableEditor();
                _stateTrigger = StateTrigger.NoTrigger;
            }
            else
            {
                UiMsgBox msgBox = new UiMsgBox(_engine.RootPanel(),
                    _engine.RenderManager, _engine.Input,
                    "Failed to load", e.Message);

                msgBox.OnComplete += result =>
                {
                    _stateTrigger = StateTrigger.NoTrigger;
                    EnableEditor();
                    ReaddTools();
                };
            }
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

    private void ReloadMap(bool trustDcmf = true)
    {
        _editorPanel.RemoveAllChildren();

        if (trustDcmf)
        {
            _mapParser.ClearObjects();
            if (_isLegacy) _mapParser.LoadFromLegacy(_filePath);
            else _mapParser.LoadFromDcmfFile(_filePath, true);
        }
        //_mapParser.Optimize();

        foreach (var dcm in _mapParser.DcmList)
        {
            _editorPanel.AddChild(dcm.Value);
        }

        _isLegacy = false;
        ReaddTools();
    }

    private void ReaddTools()
    {
        _editorPanel.AddChild(_cursor);
        _editorPanel.AddChild(_toolBarPanel);
    }

    private void SaveMap(string filename)
    {
            if (_state is EditorState.ChangedHasPath)
            {
                try
                {
                    _mapParser.SaveMap(filename);
                    MarkStateSaved(filename);
                    ReloadMap();
                }
                catch (Exception e)
                {
                    SetState(EditorState.Changed);
                    DisableEditor();
                    UiMsgBox msgBox = new UiMsgBox(_mainPanel,
                        _engine.RenderManager, _engine.Input,
                        "Failed to load", e.Message);
                    msgBox.OnComplete += result =>
                    {
                        ReloadMap(false);
                        ReaddTools();
                        EnableEditor();
                    };
                    return;
                }
            }

            if (_stateTrigger is StateTrigger.Exit
                && _state is EditorState.Saved)
            {
                _engine.LoadScene(new MainMenuScene());
                return;
            }

            if (_stateTrigger is StateTrigger.Open
                && _state is EditorState.Saved)
            {
                HandleOpen();
            }
            EnableEditor(); 
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
     *
     * For now we just readd the cursor each time to keep it on top
     */
    private void AddEntity(MapParser.DcmEntity entity, MapParser.DcmType dmType)
    {
        Point2D targetPoint = _cursor.RelativePosition;
        if (_mapParser.IsPositionAcquired(targetPoint))
            return;

        _mapParser.AddObject(targetPoint, dmType, entity);
        _editorPanel.AddChild(_mapParser.DcmList[^1].Value);
        _isEntityAdded = true;
        MarkStateUnsaved();
    }

    private void RemoveEntity(Point2D point)
    {
        var removed = _mapParser.RemoveObject(point);
        if (removed == null) return;

        _editorPanel.RemoveChild(removed);
        _isEntityAdded = false;
        MarkStateUnsaved();
    }

    private void MoveCursorBy(int x, int y)
    {
        Point2D targetPoint = _cursor.RelativePosition + new Point2D(x, y);
        _cursor.RelativePosition = targetPoint.Clamp(new Point2D(0, 0),
            _mainPanel.Size - 3);
        PlaceCursorOnTop();
        _statusBar.SetCursorPosition(_cursor.RelativePosition);
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
        DisableEditor();
        UiInputBox msgBox2 = new UiInputBox(_editorPanel,
            _engine.RenderManager, _engine.Input,
            "Enter the path of the file to be opened:", "");

        msgBox2.OnOk += OpenMap;
        msgBox2.OnCancelled += (sender, args) =>
        {
            _stateTrigger = StateTrigger.NoTrigger;
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
        }
        else if (_state is EditorState.ChangedHasPath && _stateTrigger is StateTrigger.Exit)
        {
            HandleUnsavedDialog();
        }
        else
            SaveMap(_filePath);
    }

    private void HandleUnsavedDialog()
    {
        DisableEditor();
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
                    SetState(EditorState.Saved);
                    SaveMap(_filePath);
                }
                else if (_stateTrigger is StateTrigger.Open)
                {
                    SetState(EditorState.Saved);
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
        inpBox.OnOk += s =>
        {
            SetState(EditorState.ChangedHasPath);
            SaveMap(s);
        };
        inpBox.OnCancelled += (sender, args) => EnableEditor(false);
    }

    public void OnUpdate(double deltaTime)
    {
    }

    public void OnExit()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.RootPanel().RemoveAllChildren();
    }
}

internal sealed class Cursor : GraphicsComponent
{
    public Cursor(int x, int y)
    {
        RelativePosition = new Point2D(x, y);
        Visible = true;
    }

    protected override void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        Point2D screenPos = camera.TransformPoint(WorldPosition);
        if (screenPos == Point2D.OutsideScreenPoint) return; // Off-screen culling

        renderer.SetCell(screenPos.X, screenPos.Y,
            new Cell('⊡', ConsoleColor.Black, ConsoleColor.Green));
    }
}

internal class StatusBar : UiPanel
{
    private UiLabel _posLabel;
    private UiLabel _stateLabel;

    public StatusBar(Point2D screenPos)
    {
        BackgroundColor = ConsoleColor.DarkGray;
        HasBorder = false;

        _posLabel = new UiLabel()
        {
            RelativePosition = new Point2D(1, 0),
            ForegroundColor = ConsoleColor.Green,
        };
        SetCursorPosition(screenPos);

        _stateLabel = new UiLabel()
        {
            RelativePosition = new Point2D(_posLabel.Size.Width + 4, 0),
            ForegroundColor = ConsoleColor.Green,
            Text = "Status",
        };

        Children.Add(_posLabel);
        Children.Add(_stateLabel);
    }

    public void SetCursorPosition(Point2D pos)
    {
        _posLabel.Text = "Cursor Position: " + pos.X + ", " + pos.Y;
    }

    public void SetStateLabel(string stateName)
    {
        _stateLabel.Text = stateName;
    }
}

/// <summary>
/// AI generated toolbar class (ofc I had to intervene :D)
///
/// The best way to use AI is doing boring, repetitive things like this
/// </summary>
internal class MapToolbar : UiPanel
{
    public MapToolbar(int? width = null, int? height = null)
    {
        ConsoleColor panelBg = ConsoleColor.Black;
        ConsoleColor panelFg = ConsoleColor.White;
        ConsoleColor borderColor = ConsoleColor.DarkGray;
        ConsoleColor titleColor = ConsoleColor.White;
        ConsoleColor itemsColor = ConsoleColor.DarkGreen;
        ConsoleColor demonsColor = ConsoleColor.DarkYellow;
        ConsoleColor controlsColor = ConsoleColor.DarkCyan;
        ConsoleColor toolsColor = ConsoleColor.DarkMagenta;
        ConsoleColor infoColor = ConsoleColor.DarkGray;

        // Configure panel
        BackgroundColor = panelBg;
        ForegroundColor = panelFg;
        HasBorder = true;
        BorderColor = borderColor;

        int requiredWidth = 0;
        // Create labels for shortcuts
        var titleLabel = new UiLabel
        {
            Text = "MAP EDITOR SHORTCUTS",
            ForegroundColor = titleColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 0)
        };

        var gameItemsLabel = new UiLabel
        {
            Text = "Items: [W]Wall [T]Toxic [A]Ammo [M]MedKit [B]BFG [D]Door",
            ForegroundColor = itemsColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 1)
        };

        var demonsLabel = new UiLabel
        {
            Text = "Demons: [Z]Zombieman [Shift+C]Mancubus [I]Imp  |  [P]Player",
            ForegroundColor = demonsColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 2)
        };

        string specific = SystemInfo.Os.IsWindows() ? "[Ctrl+Alt+S]Save" : "[Ctrl+S]Save";
        string specific2 = SystemInfo.Os.IsWindows() ? "[Ctrl+Alt+O]Save" : "[Ctrl+O]Open";
        var controlsLabel = new UiLabel
        {
            Text = "Controls: [Arrows]Move [Backspace]Delete " + specific + " " + specific2 + " [Esc]Exit",
            ForegroundColor = controlsColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 3)
        };
        var toolsLabel = new UiLabel
        {
            Text = "Tools: [Shift+O] Map Optimization [Shift+L] Load legacy map",
            ForegroundColor = toolsColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 4)
        };

        var infoLabel = new UiLabel
        {
            Text = "Press H to hide or show",
            ForegroundColor = infoColor,
            BackgroundColor = panelBg,
            RelativePosition = new Point2D(2, 5)
        };

        var labels = new[] { titleLabel, gameItemsLabel, demonsLabel, controlsLabel, toolsLabel, infoLabel };
        foreach (var label in labels)
        {
            AddChild(label);
            requiredWidth = CompareSize(label, requiredWidth);
        }

        Height = height ?? labels.Sum(l => l.Size.Height) + 1; // +1 for border
        Width = width ?? requiredWidth + 4;
    }

    private static int CompareSize(UiLabel label, int requiredWidth)
    {
        return Math.Max(requiredWidth, label.Size.Width);
    }
}