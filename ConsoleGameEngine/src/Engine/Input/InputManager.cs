using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;

namespace ConsoleGameEngine.Engine.Input;

public class InputManager : IDisposable
{
    // idea: We could use observables for better separation of concerns
    /*
     * Register(KeyBinding)
     * Subscribe(KeyBinding, EventHandler)
     *
     * We could register 
     */

    private class KeyRecord(bool isSolo = false)
    {
        public readonly List<Action> Listeners = [];
        // Set to false by default. It determines whether the input manager should register this
        public readonly bool IsSolo = isSolo;
    }
    
    /* Legacy manual key event (NOT RECOMMENDED)*/
    public event EventHandler<KeyEventArgs> OnKeyPressed;
    private readonly Thread _inputThread;
    private readonly Lock _lock = new();
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly CancellationTokenSource _cts;
    private readonly Dictionary<KeyBinding, KeyRecord> _keyBindings = new();
    

    public InputManager()
    {
        _cts = new CancellationTokenSource();
        
        _inputThread = new Thread(()=> InputLoop(_cts.Token))
        {
            Name = "Input Loop",
            IsBackground = true
        };
        
        _inputThread.Start();
        OnKeyPressed += HandleKeyEvent;
    }

    private void HandleKeyEvent(object? sender, KeyEventArgs e)
    {
        var binding = KeyBinding.Parse(e.Control, e.Alt, e.Shift, e.Key);
        lock (_lock)
        {
            if (!_keyBindings.TryGetValue(binding, out var value)) return;
            foreach (var listener in value.Listeners)
            {
                listener();
            }
        }
    }

    /// <summary>
    /// Register a keybind channel
    /// </summary>
    /// <param name="keyBinding"></param>
    /// <param name="isSolo"></param>
    /// <returns></returns>
    public bool Register(KeyBinding keyBinding, bool isSolo = false)
    {
        lock (_lock)
        {
            //string key = id ?? keyBinding.ToString();
            return !_keyBindings.ContainsKey(keyBinding)
                   && _keyBindings.TryAdd(keyBinding, new KeyRecord(isSolo));
        }
    }

    public bool Unregister(KeyBinding keyBinding)
    {
        lock (_lock)
        {
            return _keyBindings.ContainsKey(keyBinding)
                   && _keyBindings.Remove(keyBinding);
        }
    }

    /// <summary>
    /// Subscribes to a given channel if exits
    /// </summary>
    /// <param name="keyBinding"></param>
    /// <param name="listener"></param>
    public void Subscribe(KeyBinding keyBinding, Action listener)
    {
        lock (_lock)
        {
            if (!_keyBindings.TryGetValue(keyBinding, out var value)) return;
            if (value.Listeners.Contains(listener)) return;
            if (value.IsSolo)
            {
                _logger.Warn("You cannot add listeners to a solo KeyBinding!");
                return;
            }
            
            value.Listeners.Add(listener);
        }
    }
    
    public void UnSubscribe(KeyBinding keyBinding, Action listener)
    {
        lock (_lock)
        {
            if (_keyBindings.ContainsKey(keyBinding))
                _keyBindings[keyBinding].Listeners.Remove(listener);
        }
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

        OnKeyPressed.Invoke(this, keyEventArgs);
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