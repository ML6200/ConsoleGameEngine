using System.Collections.Generic;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

public class FocusManager
{
    private readonly List<IFocusable> _focusableComponents = new List<IFocusable>();
    private int _currentFocusIndex = -1;
    
    public IFocusable? FocusedComponent => 
        _currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count 
            ? _focusableComponents[_currentFocusIndex] :  null;
    
    public void Register(IFocusable focusable)
    {
        _focusableComponents.Add(focusable);

        if (_focusableComponents.Count == 1)
        {
            SetFocus(0);
        }
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
    private void SetFocus(int index)
    {
        if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
        {
            IFocusable previous = _focusableComponents[_currentFocusIndex];
            previous.IsFocused = false;
            previous.OnFocusLost();
        }
        
        _currentFocusIndex = index;
        
        if (_currentFocusIndex >= 0 && _currentFocusIndex < _focusableComponents.Count)
        {
            var current = _focusableComponents[_currentFocusIndex];
            current.IsFocused = true;
            current.OnFocusGained();
        }
    }
    
    
    public void FocusNext()
    {
        if (_focusableComponents.Count == 0) return;

        int nextIndex = (_currentFocusIndex + 1) % _focusableComponents.Count;
        SetFocus(nextIndex);
    }

    public void FocusPrevious()
    {
        if (_focusableComponents.Count == 0) return;

        int prevIndex = (_currentFocusIndex - 1) % _focusableComponents.Count;
        if (prevIndex < 0) prevIndex = _focusableComponents.Count - 1;
        
        SetFocus(prevIndex);
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