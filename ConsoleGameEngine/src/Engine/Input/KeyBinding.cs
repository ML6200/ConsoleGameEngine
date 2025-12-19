using System;

namespace ConsoleGameEngine.Engine.Input;

/// <summary>
/// Key binding is a helper class for registering key events
/// </summary>
public class KeyBinding
{
    public int Modifiers { get; private set; }
    public ConsoleKey Key { get; private set; }
    
    // For deterministic encode/decode we use bitwise flags 
    [Flags]
    private enum KeyModifier
    {
        Control = 1 << 1, 
        Alt     = 1 << 2,
        Shift   = 1 << 3,
    }

    public KeyBinding(int modifiers, ConsoleKey key)
    {
        Modifiers = modifiers;
        Key = key;
    }

    private KeyBinding()
    {
    }

    public static KeyBinding Parse(bool isControl, bool isAlt, bool isShift, ConsoleKey key)
    {
        KeyBinding keyBinding = new KeyBinding
        {
            Key = key,
        };
        if (isControl)
            keyBinding.Modifiers |= (int) KeyModifier.Control;
        if (isAlt)
            keyBinding.Modifiers |= (int) KeyModifier.Alt;
        if (isShift)
            keyBinding.Modifiers |= (int) KeyModifier.Shift;
        
        return keyBinding;
    }

    /// <summary>
    /// Can create an object from a given literal
    /// </summary>
    /// <param name="literal">Key binding literal. Ex: ctrl+alt+x or control+alt+x</param>
    /// <param name="separator">Set to '+' by default. You can set your own separator Ex: ctrl:alt:x</param>
    /// binding to a single Action.</param>
    public static KeyBinding Parse(string literal, char separator = '+')
    {
        string[] parts = literal.Split(separator);
        if (parts.Length is > 3 or 0) throw new FormatException();

        KeyBinding keyBinding = new KeyBinding();
        if (parts.Length is 1)
        {
            keyBinding.Key = (ConsoleKey)Enum.Parse(typeof(ConsoleKey), NormalizeKey(parts[0]));
            return keyBinding;
        }
        
        foreach (var part in parts)
        {
            keyBinding.Key = EncodeModifier(part, keyBinding);
        }
        return keyBinding;
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return string.Empty;
        
        key = key.ToLower();
        Span<char> capital = stackalloc char[1];
        key.AsSpan(0, 1).ToUpperInvariant(capital);
        return $"{capital}{key.AsSpan(1)}";
    }

    private static ConsoleKey EncodeModifier(string part, KeyBinding keyBinding)
    {
        ConsoleKey key = ConsoleKey.None;
        switch (part.ToLower())
        {
            case "control":
            case "ctrl": 
                keyBinding.Modifiers |= (int) KeyModifier.Control;
                break;
            case "alt":
                keyBinding.Modifiers |= (int) KeyModifier.Alt;
                break;
            case "shift":
                keyBinding.Modifiers |= (int) KeyModifier.Shift;
                break;
            default:
                if (!Enum.TryParse(NormalizeKey(part), out key))
                {
                    throw new FormatException();
                }
                break;
        }
        return key;
    }

    private static string DecodeModifiers(int modifiers)
    {
        bool hasControl = (modifiers & (int)KeyModifier.Control) != 0;
        bool hasAlt = (modifiers & (int)KeyModifier.Alt) != 0;
        bool hasShift = (modifiers & (int)KeyModifier.Shift) != 0;
        
        string ctrlStr = hasControl ? "control+" : "";
        string altStr = hasAlt ? "alt+" : "";
        string shiftStr = hasShift ? "shift+" : "";
        
        return $"{ctrlStr}{altStr}{shiftStr}";
    }

    public static bool operator ==(KeyBinding keyBinding1, KeyBinding keyBinding2)
    {
        return keyBinding1.Equals(keyBinding2);
    }

    public static bool operator !=(KeyBinding keyBinding1, KeyBinding keyBinding2)
    {
        return !(keyBinding1 == keyBinding2);
    }

    public override bool Equals(object? obj)
    {
        if (obj is KeyBinding keyBinding)
            return keyBinding.Key == Key && keyBinding.Modifiers == Modifiers;
        return false;
    }

    private bool Equals(KeyBinding other)
    {
        return Modifiers == other.Modifiers && Key == other.Key;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Modifiers, (int)Key);
    }

    public override string ToString()
    {
        string modLiteral = DecodeModifiers(Modifiers);
        string keyLiteral = Key.ToString().ToLower();
        return $"{modLiteral}{keyLiteral}";
    }
    
    public static class Commons
    {
        public static readonly KeyBinding CtrlX = Parse("ctrl+x");
        public static readonly KeyBinding CtrlY = Parse("ctrl+y");
        public static readonly KeyBinding Enter = Parse("enter");
        public static readonly KeyBinding Tab = Parse("tab");
        public static readonly KeyBinding Backspace = Parse("backspace");
        public static readonly KeyBinding Space = Parse("spacebar");
        public static readonly KeyBinding Escape = Parse("Escape");
        public static readonly KeyBinding CtrlC = Parse("ctrl+c");

    }
}