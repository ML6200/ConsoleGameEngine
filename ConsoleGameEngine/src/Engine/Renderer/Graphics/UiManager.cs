using System.Collections.Generic;
using ConsoleGameEngine.Engine.Input;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class UiManager
{
    private readonly InputManager _inputManager;
    private readonly List<IFocusable> _focusableComponents = new();
    
    private readonly KeyBinding _tabKey = KeyBinding.Commons.Tab;
    private readonly KeyBinding _shiftTabKey = KeyBinding.Parse("shift+tab");

    private int _currentFocusIndex = -1;
    private IFocusable? FocusedComponent => 
        _currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count 
            ? _focusableComponents[_currentFocusIndex] :  null;

    
    public UiManager(InputManager inputManager)
    {
        _inputManager = inputManager;
        
        _inputManager.Register(_tabKey);
        _inputManager.Register(_shiftTabKey);
        _inputManager.Register(KeyBinding.Commons.Enter);

        _inputManager.Subscribe(_tabKey, FocusNext);
        _inputManager.Subscribe(_shiftTabKey, FocusPrevious);
        _inputManager.Subscribe(KeyBinding.Commons.Enter, () =>
        {
            if (FocusedComponent is UiButton)
            {
                FocusedComponent.OnFocusActivate();
            }
        });
        
        _inputManager.SubscribeToRawInput(HandleInput);
    }

    private void HandleInput(KeyEventArgs e)
    {
        if (FocusedComponent is IUiInput inputComponent)
        {
            inputComponent.HandleInput(e);
        }
    }

    public void Register(IFocusable focusable)
    {
        _focusableComponents.Add(focusable);

        if (_focusableComponents.Count == 1)
            ActivateFocus(0);
    }

    public void Unregister(IFocusable focusable)
    {
        if (_focusableComponents.Contains(focusable))
        {
            focusable.IsFocused = false;
            focusable.OnFocusLost();
            _focusableComponents.Remove(focusable);
        }
    }

    public void UnregisterAll()
    {
        _focusableComponents.Clear();
        _currentFocusIndex = -1;
    }
    private void ActivateFocus(int index)
    {
        if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
        {
            IFocusable previous = _focusableComponents[_currentFocusIndex];
            previous.IsFocused = false;
            previous.OnFocusLost();
        }

        if (_focusableComponents[index].CanFocus)
        {
            _currentFocusIndex = index;
            if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
            {
                var current = _focusableComponents[_currentFocusIndex];
                current.IsFocused = true;
                current.OnFocusGained();
                
                if (current is IUiInput)
                    _inputManager.Mode = InputManager.InputMode.TextInput;
                else
                    _inputManager.Mode = InputManager.InputMode.KeyInput;
            }
        }
    }
    
    
    public void FocusNext()
    {
        if (_focusableComponents.Count == 0) return;

        // same as: _currentFocusIndex + 1 < _focusableComponents.Count ? _currentFocusIndex + 1 : 0;
        int nextIndex = (_currentFocusIndex + 1) % _focusableComponents.Count;
        ActivateFocus(nextIndex);
    }

    public void FocusPrevious()
    {
        if (_focusableComponents.Count == 0) return;

        int prevIndex = (_currentFocusIndex - 1) % _focusableComponents.Count;
        if (prevIndex < 0) prevIndex = _focusableComponents.Count - 1;
        
        ActivateFocus(prevIndex);
    }
    
    public void ActivateFocused()
    {
        FocusedComponent?.OnFocusActivate();
    }

    public void ClearAll()
    {
        if (FocusedComponent != null)
        {
            FocusedComponent.IsFocused = false;
            FocusedComponent.OnFocusLost();
        }
        
        _focusableComponents.Clear();
        _currentFocusIndex = -1;
    }
}