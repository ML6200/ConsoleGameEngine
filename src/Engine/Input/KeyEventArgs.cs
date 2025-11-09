using System;

namespace ConsoleGameEngine.Engine.Input;

public class KeyEventArgs : EventArgs
{
    public ConsoleKey Key { get; set; }
    public char KeyChar { get; set; }
    public bool Control { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}