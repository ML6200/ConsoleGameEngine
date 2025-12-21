using System.Collections.Generic;
using ConsoleGameEngine.Engine.Input;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiManager : IComponentObserver
{
    private readonly InputManager _inputManager;
    private readonly KeyboardFocusManager _keyboardFocusManager;
    
    private readonly KeyBinding _tabKey = KeyBinding.Commons.Tab;
    private readonly KeyBinding _shiftTabKey = KeyBinding.Parse("shift+tab");

    
    public UiManager(InputManager inputManager)
    {
        _inputManager = inputManager;
        _keyboardFocusManager = new KeyboardFocusManager(inputManager);
        
        _inputManager.Register(_tabKey);
        _inputManager.Register(_shiftTabKey);
        _inputManager.Register(KeyBinding.Commons.Enter);

        _inputManager.Subscribe(_tabKey, _keyboardFocusManager.FocusNext);
        _inputManager.Subscribe(_shiftTabKey, _keyboardFocusManager.FocusPrevious);
        _inputManager.Subscribe(KeyBinding.Commons.Enter, () =>
        {
            if (_keyboardFocusManager.FocusedComponent is UiButton)
            {
                _keyboardFocusManager.FocusedComponent.OnFocusActivate();
            }
        });
        
        _inputManager.SubscribeToRawInput(HandleInput);
    }

    private void HandleInput(KeyEventArgs e)
    {
        if (_keyboardFocusManager.FocusedComponent is IUiInput inputComponent)
        {
            inputComponent.HandleInput(e);
        }
    }

    public void OnComponentAdded(GraphicsComponent component)
    {
        if (component is IFocusable focusable)
            _keyboardFocusManager.Register(focusable);
    }

    public void OnComponentRemoved(GraphicsComponent component)
    {
        if (component is IFocusable focusable)
            _keyboardFocusManager.Unregister(focusable);
    }
}