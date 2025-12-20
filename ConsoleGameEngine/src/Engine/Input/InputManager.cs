using System;
using System.Collections.Generic;
using System.Threading;
using NLog;

namespace ConsoleGameEngine.Engine.Input;

public class InputManager : IDisposable
{
    public enum InputMode
    {
        TextInput,
        KeyInput
    }
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
    public InputMode Mode {get; set;} = InputMode.KeyInput;
    
    private readonly Thread _inputThread;
    private readonly Lock _lock = new();
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private readonly CancellationTokenSource _cts;
    private readonly Dictionary<KeyBinding, KeyRecord> _keyBindings = new();
    private readonly List<Action<KeyEventArgs>> _rawInputChannel = new();

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
            if (value.IsSolo && value.Listeners.Contains(listener))
            {
                _logger.Warn("You cannot add listeners to a solo KeyBinding!");
                return;
            }
            
            value.Listeners.Add(listener);
        }
    }

    public void SubscribeToRawInput(Action<KeyEventArgs> action)
    {
        lock (_lock)
        {
            _rawInputChannel.Add(action);
        }
    }
    
    public void UnsubscribeFromRawInput(Action<KeyEventArgs> action)
    {
        lock (_lock)
        {
            _rawInputChannel.Remove(action);
        }
    }
    
    public void UnSubscribe(KeyBinding keyBinding, Action listener)
    {
        lock (_lock)
        {
            if (_keyBindings.TryGetValue(keyBinding, out var binding))
                binding.Listeners.Remove(listener);
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

        HandleKeyEvent(keyEventArgs);
    }
    
    private void HandleKeyEvent(KeyEventArgs e)
    {
        var binding = KeyBinding.Parse(e.Control, e.Alt, e.Shift, e.Key);
        lock (_lock)
        {
            if (Mode == InputMode.KeyInput)
            {
                if (_keyBindings.TryGetValue(binding, out var value))
                {
                    foreach (var listener in value.Listeners)
                    {
                        listener();
                    }
                }
            }
            else
            {
                if (e.IsNavigation && _keyBindings.TryGetValue(binding, out var value))
                {
                    foreach (var listener in value.Listeners)
                    {
                        listener();
                    }
                }
                else
                {
                    foreach (var action in _rawInputChannel)
                    {
                        action(e);
                    }
                }
            }
        }
    }
    
    public void Dispose()
    {
        if (_cts is not { IsCancellationRequested: false }) return;
        
        _cts.Cancel();
        _inputThread.Join();
        _cts.Dispose();
    }
}