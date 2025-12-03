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

    private Point2D? _relativePosition;
    public virtual bool Visible { get; set; } = true;
    
    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public ConsoleColor BorderColor { get; set; }

    private List<Animation> Animations { get; } = new();
    public List<ConsoleGraphicsComponent> Children { get; } = new();
    private IConsoleRenderable Parent { get; set; }
    
    private readonly object _childrenLock = new();
    
    
    public Dimension2D WorldSize => new(Console.WindowWidth, Console.WindowHeight);


    // ====================CONSTRUCTORS====================
    public ConsoleGraphicsComponent(int width, int height, 
        Point2D? relativePosition, 
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
        Point2D? relativePosition)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition;
    }

    public ConsoleGraphicsComponent()
    {
        
    }
    // ====================CONSTRUCTORS_END====================
    
    // ====================POSITIONING====================
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
    public Point2D? WorldPosition
    {
        get
        {
            if (Parent is ConsoleGraphicsComponent { WorldPosition: not null } parent)
            {
                if (_relativePosition != null)
                    return parent.WorldPosition + _relativePosition;
            }
            return _relativePosition;
        }
        set
        {
            SetAbsolutePosition(value);
        }
    }

    public void SetAbsolutePosition(Point2D absolutePoint)
    {
        if (Parent is ConsoleGraphicsComponent {WorldPosition: not null} parent)
        {
            _relativePosition = absolutePoint - parent.WorldPosition;
        } else 
        {
            _relativePosition = absolutePoint;
        }
    }
    
    public Point2D RelativePosition
    {
        get => _relativePosition ?? new Point2D(0, 0);
        set => _relativePosition = value;
    }
    // ======================END-POSITIONING=======================

    // ====================ANIMATION-MANAGEMENT====================
    public void AddAnimation(Animation animation)
    {
        Animations.Add(animation);
    }

    public void ClearAnimations()
    {
        Animations.Clear();
    }
    // ====================ANIMATION-MANAGEMENT-END====================
    
    
    // ============================PARENTING===========================
    public void AddChild(ConsoleGraphicsComponent child)
    {
        lock (_childrenLock)
        {
            Children.Add(child);
            child.Parent = this;
        }
    }

    public void RemoveChild(ConsoleGraphicsComponent child)
    {
        lock (_childrenLock)
        {
            Children.Remove(child);
        }
    }

    public List<ConsoleGraphicsComponent> GetChildrenSnapshot()
    {
        lock (_childrenLock)
        {
            return Children.ToList();
        }
    }
    // ============================PARENTING-END====================

    // ============================RENDERING========================
    public virtual void Compute(ConsoleRenderer2D renderer)
    {
        if (!Visible) return;
        
        var childrenSnapshot = Children.ToList();
        foreach (var child in childrenSnapshot)
        {
            child.Compute(renderer);
        }
    }

    public void Update(double deltaTime)
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
    // ============================RENDERING-END========================
}