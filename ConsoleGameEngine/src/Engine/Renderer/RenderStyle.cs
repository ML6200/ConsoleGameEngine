using System;

namespace ConsoleGameEngine.Engine.Renderer;

public readonly struct RenderStyle : IEquatable<RenderStyle>
{
    public readonly AnsiColor Background;
    public readonly AnsiColor Foreground;
    public readonly FontStyle FontStyle;

    public RenderStyle(AnsiColor background, AnsiColor foreground, FontStyle fontStyle)
    {
        Background = background;
        Foreground = foreground;
        FontStyle = fontStyle;
    }

    public static bool operator ==(RenderStyle style1, RenderStyle style2)
    {
        return style1.Equals(style2);
    }

    public static bool operator !=(RenderStyle style1, RenderStyle style2)
    {
        return !(style1 == style2);
    }

    public bool Equals(RenderStyle other)
    {
        return Background == other.Background 
               && Foreground == other.Foreground 
               && FontStyle == other.FontStyle;
    }

    public override bool Equals(object? obj)
    {
        return obj is RenderStyle other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)Background, (int)Foreground, (int)FontStyle);
    }
    
    public static readonly RenderStyle Default = 
        new(AnsiColor.White, 
            AnsiColor.Black, 
            FontStyle.Regular);
}