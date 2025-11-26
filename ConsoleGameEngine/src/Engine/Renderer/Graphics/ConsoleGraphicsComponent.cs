using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Animations;

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
 */

public abstract class ConsoleGraphicsComponent : IConsoleRenderable
{
    protected int Width;
    protected int Height;

    private Position2D? _relativePosition;
    private List<Animation> Animations { get; } = new List<Animation>();

    public virtual void Update(double deltaTime)
    {
        foreach (var anim in Animations.ToList())
        {
            anim.OnUpdate(deltaTime);
            if (anim.IsComplete)
            {
                Animations.Remove(anim);
            }
        }
        
        var childrenSnapshot = Children.ToList();
        foreach (var child in childrenSnapshot)
        {
            child.Update(deltaTime);
        }
    }
    
    public void AddAnimation(Animation animation)
    {
        Animations.Add(animation);
    }

    public void ClearAnimations()
    {
        Animations.Clear();
    }
    
    public Dimension2D Size
    {
        get
        {
            return new Dimension2D(Width, Height);
        }
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    /*
     * A komponensek az újabb tervezetben csak a lokális(relatív) pozícíciót
     * tárolják ezzel csökkentve a komplexitást. Az előző változatban mind a
     * globális és a lokális pozíciót is követtük, mely eléggé logikátlan, mivel
     * dupla számolást jelent. Ezzel ellentétben ha a fa mentén bejárjuk a gyerek nodeok
     * felől és mindig az adott szülő a referencia pont ezzel megkaphatjuk az aktuális
     * pozíciót a rendereléshez.
     *
     * PL:
     *
     * [Parent:root] 
     *     -> lok(0, 0)
     *     -> glob(0, 0)
     *
     * [Child1]
     *  ->lok(1, 1)
     *  ->glob=Parent.glob + (1, 1) => (1, 1)
     * 
     * [Child2]
     *  ->lok(1, 1)
     *  ->glob=Child1.glob + (1, 1) => (2, 2)
     *
     *
     * !!!Megjegyzés+++
     * Ezt a rekurzív megoldást később kiválthatjuk egy külön layout manager
     * vagy Transform osztály bevezetésével.
     * 
     */
    public Position2D AbsolutePosition
    {
        get
        {
            if (Parent is ConsoleGraphicsComponent parent)
            {
                return parent.AbsolutePosition + _relativePosition;
            }
            return _relativePosition;
        }
    }

    public void SetAbsolutePosition(Position2D absolutePosition)
    {
        if (Parent is ConsoleGraphicsComponent parent)
        {
            _relativePosition = absolutePosition - parent.AbsolutePosition;
        } else 
        {
            _relativePosition = absolutePosition;
        }
    }
    
    public Position2D RelativePosition
    {
        get => _relativePosition ?? new Position2D(0, 0);
        set
        {
            _relativePosition = value;
        }
    }

    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public ConsoleColor BorderColor { get; set; }
    
    
    public List<ConsoleGraphicsComponent> Children { get; } = new();
    
    private IConsoleRenderable Parent { get; set; }

    public void AddChild(ConsoleGraphicsComponent child)
    {
        Children.Add(child);
        child.Parent = this;
    }
    public void RemoveChild(ConsoleGraphicsComponent child) => Children.Remove(child);
    public virtual bool Visible { get; set; } = true;


    public ConsoleGraphicsComponent(int width, int height, 
        Position2D? relativePosition, 
        ConsoleColor backgroundColor, 
        ConsoleColor foregroundColor, 
        ConsoleColor borderColor)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition;
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
        BorderColor = borderColor;
    }
    
    public ConsoleGraphicsComponent(int width, int height, 
        Position2D? relativePosition)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition;
    }

    public ConsoleGraphicsComponent()
    {
        
    }
    

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

        // Create snapshot to avoid concurrent modification during iteration
        var childrenSnapshot = Children.ToList();
        foreach (var child in childrenSnapshot)
        {
            child.Render(renderer);
        }
    }

    public void Dispose()
    {
        Children.Clear();
    }
}