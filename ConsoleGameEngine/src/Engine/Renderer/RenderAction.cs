using System;

namespace ConsoleGameEngine.Engine.Renderer;

public struct RenderAction
{
    public int Layer;
    public int ZIndex;
    public int SequenceId;
    public Action Action;
}