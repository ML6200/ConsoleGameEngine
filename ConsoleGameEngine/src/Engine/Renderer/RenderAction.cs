using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

public struct RenderAction
{
    public int Layer;
    public int ZIndex;
    public int SequenceId;
    public GraphicsComponent Component;
    public ConsoleRenderer2D Renderer;
    public Point2D ScreenPosition;
}