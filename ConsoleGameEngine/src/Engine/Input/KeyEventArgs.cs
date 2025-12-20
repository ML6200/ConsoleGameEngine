using System;

namespace ConsoleGameEngine.Engine.Input;

public class KeyEventArgs : EventArgs
{
    public ConsoleKey Key { get; set; }
    public char KeyChar { get; set; }
    public bool Control { get; set; }
    public bool Alt { get; set; }
    public bool Shift { get; set; }
    
    public bool IsPrintable => !char.IsControl(KeyChar);
    public bool IsNavigation => Key is ConsoleKey.Tab or ConsoleKey.Enter;
}