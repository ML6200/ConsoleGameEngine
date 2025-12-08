using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Input;
using ConsoleGameEngine.Engine.Renderer;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class MapEditor : IGameScene
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
    private UiPanel _editorPanel;
    private UiLabel _placeHolder;
    private UiButton _backButton;
    private Mapper _mapper;
    
    private string _mapPath;

    public MapEditor(string mapPath)
    {
        _mapPath = mapPath;
    }

    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine = consoleEngine;
    }

    public void OnEnter()
    {
        _editorPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(_engine.RootPanel().ScreenSize.Width, _engine.RootPanel().ScreenSize.Height),
            HasBorder = false
        };
        _engine.RootPanel().AddChild(_editorPanel);
        
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;
        
        _placeHolder = new UiLabel()
        {
            RelativePosition = new Point2D(centerX, centerY),
            Text = "NOT IMPLEMENTED FEATURE",
            ForegroundColor = ConsoleColor.Red
        };

        _backButton = new UiButton()
        {
            Text = "Back",
            RelativePosition = new Point2D(centerX, centerY - 10),
            Size = new Dimension2D(20, 3),
            FocusedBgColor = ConsoleColor.Red,
            BackgroundColor = ConsoleColor.DarkRed,
            ForegroundColor = ConsoleColor.White,
        };
        
        _backButton.OnClick += BackButtonOnOnClick;
        _editorPanel.AddChild(_backButton);
        _editorPanel.AddChild(_placeHolder);
        
        _engine.RenderManager.FocusManager.Register(_backButton);
    }

    private void BackButtonOnOnClick(object? sender, EventArgs e)
    {
        _engine.LoadScene(new MainMenuScene(_mapPath));
    }

    public void OnUpdate(double deltaTime)
    {
    }

    public void OnExit()
    {
        _editorPanel.RemoveChild(_placeHolder);
        _editorPanel.RemoveChild(_backButton);
        _engine.RootPanel().RemoveChild(_editorPanel);
    }
}