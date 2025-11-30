using System;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public readonly struct Cell : IEquatable<Cell>
{
    public readonly char Character;
    public readonly ConsoleColor ForegroundColor;
    public readonly ConsoleColor BackgroundColor;

    public Cell(char character = ' ', 
        ConsoleColor backgroundColor = ConsoleColor.Black, 
        ConsoleColor foregroundColor = ConsoleColor.White)
    {
        Character = character;
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() == typeof(Cell))
        {
            Cell other = (Cell)obj;
            return Character ==  other.Character &&
                   ForegroundColor == other.ForegroundColor &&
                   BackgroundColor == other.BackgroundColor;
        }
        
        return base.Equals(obj);
    }

    public bool Equals(Cell other)
    {
        return Character == other.Character 
               && BackgroundColor == other.BackgroundColor 
               && ForegroundColor == other.ForegroundColor;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Character, (int)BackgroundColor, (int)ForegroundColor);
    }
    
    public static Cell Empty => new Cell(' ', ConsoleColor.Black, ConsoleColor.White);
}