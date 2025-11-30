using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleGameEngine.Engine.Input;

public class InputManager: IDisposable
{
    public event EventHandler<KeyEventArgs> OnKeyPressed;
    public event EventHandler OnEscapePressed;
    public event EventHandler OnEnterPressed;
    public event EventHandler OnSpacePressed;
    
    private readonly Thread _inputThread;
    private readonly Dictionary<ConsoleKey, bool> _keyStates = new();
    private CancellationTokenSource _cts;

    public InputManager()
    {
        _cts = new CancellationTokenSource();
        
        _inputThread = new Thread(()=> InputLoop(_cts.Token))
        {
            Name = "Input Loop",
            IsBackground = true
        };
        
        _inputThread.Start();
    }

    private void InputLoop(CancellationToken ctxToken)
    {
        while (!ctxToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                HandleKeyPressed(keyInfo);
            }
            // Egy keves kesleltetes a cpu-spinning elkerulesere:
            Thread.Sleep(5);
        }
    }

    private void HandleKeyPressed(ConsoleKeyInfo keyInfo)
    {
        KeyEventArgs keyEventArgs = new KeyEventArgs
        {
            Key = keyInfo.Key,
            KeyChar = keyInfo.KeyChar,
            Shift = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift),
            Control = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control),
            Alt = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Alt),
        };

        OnKeyPressed?.Invoke(this, keyEventArgs);

        switch (keyInfo.Key)
        {
            case ConsoleKey.Escape:
                OnEscapePressed?.Invoke(this, EventArgs.Empty);
                break;
            case ConsoleKey.Enter:
                OnEnterPressed?.Invoke(this, EventArgs.Empty);
                break;
            case ConsoleKey.Spacebar:
                OnSpacePressed?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    public bool IsKeyDown(ConsoleKey key)
    {
        return _keyStates.ContainsKey(key) && _keyStates[key];    
    }
    
    public void Dispose()
    {
        if (_cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
            _inputThread.Join();
            _cts.Dispose();
        }
    }
}