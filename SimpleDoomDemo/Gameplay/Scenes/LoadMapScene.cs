using System;
using ConsoleGameEngine.Engine;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace SimpleDoomDemo.Gameplay.Scenes;

public class LoadMapScene : IGameScene
{
    private ConsoleEngine _engine;
    private UiPanel _loadPanel;
    private UiButton _playButton;
    
    public void Initialize(ConsoleEngine consoleEngine)
    {
        _engine =  consoleEngine;
    }

    public void OnEnter()
    {
        int centerX = _engine.RootPanel().ScreenSize.Width / 2;
        int centerY = _engine.RootPanel().ScreenSize.Height / 2;
        
        _loadPanel = new UiPanel()
        {
            RelativePosition = new Point2D(0, 0),
            Size = new Dimension2D(_engine.RootPanel().ScreenSize.Width, _engine.RootPanel().ScreenSize.Height),
            HasBorder = false
        };
        _engine.RootPanel().AddChild(_loadPanel);
    }

    public void OnUpdate(double deltaTime)
    {
        // Doesnt need update
    }

    public void OnExit()
    {
        _engine.RenderManager.FocusManager.UnregisterAll();
        _engine.Stop();
        _engine.Dispose();
    }
}