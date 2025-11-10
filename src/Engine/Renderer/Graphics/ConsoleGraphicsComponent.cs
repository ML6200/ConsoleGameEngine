using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

/*
 * Egyszerű fa nézet:
 * 
 * Root - Child3 - Child4
 *   |
 * Child1
 *   |
 * Child2
 *
 * -----------------------------
 *
 *
 * Root (0, 0)
 *  |
 * Child1 (1, 1) Render->(0+1, 0+1)-> (1, 1)
 *  |
 * Child2 (1, 1) Render->(1+1, 1+1)->(2, 2)
 * +-----------------------------------------------------+
 * | A gyerek komponensek mindig relatív pozíciót várnak,|
 * | melyet az adott komponens Render() metódusa kezel.  |
 * +-----------------------------------------------------+
 *
 * ################MEGJEGYZÉS########################
 * # Későbbiekben célszerű ezt a relativisztikus    #
 * # megoldást egy külön osztályban kezelni vagy    #
 * # akár a renderelő motor által.                  #
 * ##################################################
 *
 *
 * Minden komponens rendelkezik egy Szülő és egy Gyerek
 * tulajdonsággal. Egy komponensnek több gyereke lehet
 * viszont csak egy szülője.
 *
 *
 *
 * Abszolút & Relatív pocíció
 *  
 *  y
 *  |
 *  |
 *  |
 * 2|   abs(7, 2) -> rel (0, 0)
 * 1|
 *  |--------------------------> x
 *   1234567
 *
 *
 *  Egy elemnek abszolút pozíciója a térben egyértelmű pozíciója,
 *  míg a relatív pozíció a szülőosztályhoz képest igazodik.
 *  Példányosításnál a relatív pozíciót adhatjuk meg,
 *  viszont külön beállíthatunk abszolút pozíciót is.
 *
 *  Pl:
 *  Panel(1, 1, 10, 10)->Gomb(10/2, 10/2, 3, 2)
 *
 *  ConsoleGraphicsPanel panel1 = new ConsoleGraphicsPanel(3, 4, 20, 30);
 *  ConsoleGraphicsPanel panel2 = new ConsoleGraphicsPanel(3, 4, 20, 30);
 *
 * 
 *  Alapvetően nem tűnik logikusnak mindkettőt megadni,
 *  viszont van olyan helyzet amikor ugyan a panelhez tartozik a gomb,
 *  de a panelen kívül akarjuk elhelyezni.
 *
 *  Ez a megoldás meglehetősen kezdetleges, ezt a későbbiekben kiválthatjuk
 *  egy layout-manager bevezetésével ami automatikusan kezeli az elrendezést.
 *
 *                             
 *
 *
 * */
public abstract class ConsoleGraphicsComponent : IConsoleComponent
{
    private int _width;
    private int _height;
    private Position2D? _absolutePosition;
    private Position2D? _relativePosition;
    
    public Dimension2D Size
    {
        get
        {
            return new Dimension2D(_width, _height);
        }
        set
        {
            _width = value.Width;
            _height = value.Height;
        }
    }
    
    public Position2D AbsolutePosition
    {
        get { return _absolutePosition ?? new Position2D(0, 0); }
        init => _absolutePosition = value;
    }

    public Position2D RelativePosition
    {
        get => _relativePosition ?? new Position2D(0, 0);
        set => _relativePosition = value;
    }

    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public ConsoleColor BorderColor { get; set; }
    
    
    public List<ConsoleGraphicsComponent> Children { get; } = new();
    
    public IConsoleComponent Parent { get; set; }

    public void AddChild(ConsoleGraphicsComponent child)
    {
        Children.Add(child);
        child.Parent = this;
    }
    public void RemoveChild(ConsoleGraphicsComponent child) => Children.Remove(child);

    
    public virtual bool Visible { get; set; } = true;

    public Dimension2D WorldSize
    {
        get
        {
            return new Dimension2D(Console.WindowWidth, Console.WindowHeight);
        }
    }

    public virtual void Render(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;

        foreach (var child in Children)
        {
            child.Render(renderer);
        }
    }

    public Position2D ComputeAbsolutePosition()
    {
        if (Parent is ConsoleGraphicsComponent parent)
        {
            return parent.ComputeAbsolutePosition() + RelativePosition;
        }

        return _absolutePosition;
    }

    public abstract void Update();
    
    public void Dispose()
    {
        Children.Clear();
    }
}