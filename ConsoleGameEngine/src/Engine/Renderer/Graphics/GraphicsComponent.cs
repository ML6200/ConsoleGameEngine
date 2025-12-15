using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

public abstract class GraphicsComponent : IRenderable
{
    protected int Width;
    protected int Height;
    public virtual bool Visible { get; set; } = true;
    
    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public ConsoleColor BorderColor { get; set; }

    private List<Animation> Animations { get; } = new();
    public List<GraphicsComponent> Children { get; } = new();
    private IRenderable? Parent { get; set; }
    
    private GraphicsComponent[] _cachedChildren;
    private Point2D _relativePosition = new(0, 0);
    private Point2D _cachedWorldPosition = new(0, 0);
    private bool _isPositionDirty = true;
    private bool _childrenDirty = true;
    private readonly Lock _childrenLock = new();
    
    
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);


    // ====================CONSTRUCTORS====================
    public GraphicsComponent(int width, int height,
        Point2D? relativePosition,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor,
        ConsoleColor borderColor)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition ?? new Point2D(0, 0);
        BackgroundColor = backgroundColor;
        ForegroundColor = foregroundColor;
        BorderColor = borderColor;
    }

    public GraphicsComponent(int width, int height,
        Point2D? relativePosition)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition ?? new Point2D(0, 0);
    }

    public GraphicsComponent()
    {
        // _relativePosition already initialized to (0, 0) via field initializer
    }
    // ====================CONSTRUCTORS_END====================
    
    // ====================POSITIONING====================
    public Dimension2D Size
    {
        get => new(Width, Height);
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
    public Point2D WorldPosition
    {
        get
        {
            if (_isPositionDirty)
            {
                UpdateWorldPosition();
            }
            return _cachedWorldPosition;
        }
        set
        {
            if (value != _cachedWorldPosition)
            {
                SetWorldPosition(value);
            }
        }
    }

    private void SetWorldPosition(Point2D worldPosition)
    {
        Point2D newRelative;
        if (Parent is GraphicsComponent parent)
        {
            newRelative = worldPosition - parent.WorldPosition;
        } else newRelative = worldPosition;

        if (newRelative != _relativePosition)
        {
            _relativePosition = newRelative;
            _cachedWorldPosition = worldPosition;
            _isPositionDirty = false;
        }
    }

    private void UpdateWorldPosition()
    {
        if (Parent is GraphicsComponent parent)
        {
            _cachedWorldPosition = parent.WorldPosition + _relativePosition;
        }
        else _cachedWorldPosition = _relativePosition;
        
        _isPositionDirty = false;
    }

    public Point2D RelativePosition
    {
        get => _relativePosition;
        set
        {
            if (_relativePosition != value)
            {
                _relativePosition = value;
                MarkWorldPositionDirty();
            }
        }
    }

    private void MarkWorldPositionDirty()
    {
        _isPositionDirty = true;
        MarkChildrenDirty();
    }

    private void MarkChildrenDirty()
    {
        foreach (var child in Children)
        {
            child.MarkWorldPositionDirty();
        }
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
    public void AddChild(GraphicsComponent child)
    {
        lock (_childrenLock)
        {
            Children.Add(child);
            _childrenDirty = true;
            child.Parent = this;
            MarkWorldPositionDirty();
        }
    }

    public void RemoveChild(GraphicsComponent child)
    {
        lock (_childrenLock)
        {
            Children.Remove(child);
            _childrenDirty = true;
            child.Parent = null;
            child.MarkWorldPositionDirty();
        }
    }

    public void RemoveAllChildren()
    {
        lock (_childrenLock)
        {
            Children.Clear();

            foreach (var child in Children)
            {
                RemoveChild(child);
            }
        }
    }

    private GraphicsComponent[] GetChildrenSnapshot()
    {
        if (_childrenDirty)
        {
            lock (_childrenLock)
            {
                if (_childrenDirty)
                {
                    _cachedChildren =  Children.ToArray();
                    _childrenDirty = false;    
                }
            }
        }
        return _cachedChildren;
    }
    // ============================PARENTING-END====================

    // ============================RENDERING========================
    public void Compute(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
        if (!Visible) return;
        
        RenderSelf(renderer, camera);
        
        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
        {
            child.Compute(renderer,  camera);
        }
    }

    protected virtual void RenderSelf(ConsoleRenderer2D renderer, ConsoleCamera camera)
    {
    }

    public void Update(double deltaTime)
    {
        for (int i = Animations.Count - 1; i >= 0; i--)
        {
            Animations[i].OnUpdate(deltaTime);
            if (Animations[i].IsComplete)
            {
                Animations.Remove(Animations[i]);
            }
        }
        
        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
        {
            child.Update(deltaTime);
        }
    }
    // ============================RENDERING-END========================
}