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

    private Dictionary<ConsoleKey, bool> keyStates = new Dictionary<ConsoleKey, bool>();

    public InputManager()
    {
        isRunning = true;
        
        inputThread = new Thread(InputLoop);
        inputThread.IsBackground = true;
        inputThread.Start();
    }

    private void InputLoop()
    {
        while (isRunning)
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

    public void HandleKeyReleased(ConsoleKeyInfo keyInfo)
    {
        throw new NotImplementedException();
    }

    public bool IsKeyDown(ConsoleKey key)
    {
        return keyStates.ContainsKey(key) && keyStates[key];    
    }
    
    public void Dispose()
    {
        isRunning = false;
        inputThread.Join(1000);
    }
}