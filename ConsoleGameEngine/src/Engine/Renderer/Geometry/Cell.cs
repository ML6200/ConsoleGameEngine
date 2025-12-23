using System;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

public readonly struct Cell : IEquatable<Cell>
{
    public readonly char Character;
    public readonly RenderStyle RenderStyle;

    public Cell(char character = ' ', RenderStyle renderStyle = default)
    {
        RenderStyle = renderStyle;
        Character = character;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj?.GetType() == typeof(Cell))
        {
            Cell other = (Cell)obj;
            return Character ==  other.Character && 
                   RenderStyle == other.RenderStyle;
        }
        
        return base.Equals(obj);
    }

    public bool Equals(Cell other)
    {
        return Character ==  other.Character && 
               RenderStyle == other.RenderStyle;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Character, RenderStyle.GetHashCode());
    }
    
    public static Cell Empty => new Cell(' ');
}