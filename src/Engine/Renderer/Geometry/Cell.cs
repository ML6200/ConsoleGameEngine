using System;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public struct Cell
{
    public char Character {get; set;}
    public ConsoleColor BackgroundColor 
    { 
        get; 
        set; 
    }

    public ConsoleColor ForegroundColor
    {
        get; 
        set;
    }

    public Cell(char character = ' ', 
        ConsoleColor backgroundColor = ConsoleColor.Black, 
        ConsoleColor foregroundColor = ConsoleColor.White)
    {
        this.Character = character;
        this.BackgroundColor = backgroundColor;
        this.ForegroundColor = foregroundColor;
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
}