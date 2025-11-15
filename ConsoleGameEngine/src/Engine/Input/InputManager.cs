using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleGameEngine.Engine.Input;

public class InputManager: IDisposable
{
    public event EventHandler<KeyEventArgs> OnKeyPressed;
    public event EventHandler<KeyEventArgs> OnKeyReleased;
    
    public event EventHandler OnEscapePressed;
    public event EventHandler OnEnterPressed;
    public event EventHandler OnSpacePressed;
    
    private Thread inputThread;
    private bool isRunning;

    private CancellationTokenSource _cts;

    private Dictionary<ConsoleKey, bool> keyStates = new Dictionary<ConsoleKey, bool>();

    public InputManager()
    {
        _cts = new CancellationTokenSource();
        
        inputThread = new Thread(()=> InputLoop(_cts.Token))
        {
            Name = "Input Loop",
            IsBackground = true
        };
        
        inputThread.Start();
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
            Thread.Sleep(10); // Delay to prevent cpu spinning
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
        return keyStates.ContainsKey(key) && keyStates[key];    
    }
    
    public void Dispose()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            inputThread.Join();
            _cts.Dispose();
        }
    }
}