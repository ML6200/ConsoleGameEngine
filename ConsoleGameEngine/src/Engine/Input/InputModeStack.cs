using System.Collections;
using System.Collections.Generic;

namespace ConsoleGameEngine.Engine.Input;

public class InputModeStack
{
    private readonly Stack<InputMode> _stack = new();

    public InputModeStack()
    {
        _stack.Push(InputMode.KeyInput);
    }

    public InputMode Current => _stack.Peek();

    public void PushMode(InputMode mode)
    {
        _stack.Push(mode);
    }

    public void PopMode()
    {
        if (_stack.Count == 1) return;
        _stack.Pop();
    }
}