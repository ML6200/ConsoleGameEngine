using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Animations;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

/*
 * Graphics component hierarchy system:
 *
 * Tree structure example:
 *   Root (0, 0)
 *    |
 *   Child1 (1, 1) -> renders at (0+1, 0+1) = (1, 1)
 *    |
 *   Child2 (1, 1) -> renders at (1+1, 1+1) = (2, 2)
 *
 * Each component has one parent and can have multiple children.
 * Child components use positions relative to their parent.
 *
 * Position types:
 *   - RelativePosition: Position relative to parent
 *   - WorldPosition: Absolute position in screen space
 *
 * Example:
 *   Parent at (10, 10) with Child at relative (5, 5)
 *   -> Child renders at world position (15, 15)
 */

public abstract class GraphicsComponent : IRenderable
{
    protected int Width;
    protected int Height;
    public virtual bool Visible { get; set; } = true;
    
    public ConsoleColor BackgroundColor { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public ConsoleColor BorderColor { get; set; }

    public List<Animation> Animations { get; } = new();
    public List<GraphicsComponent> Children { get; } = new();
    private IRenderable? Parent { get; set; }

    private GraphicsComponent[] _cachedChildren = [];
    private Point2D _relativePosition = new(0, 0);
    private Point2D _cachedWorldPosition = new(0, 0);
    private bool _isPositionDirty = true;
    private bool _childrenDirty = true;
    private readonly Lock _childrenLock = new();
    
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);


    // ========CONSTRUCTORS========
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
        // _relativePosition defaults to (0, 0) via field initializer
    }
    // ========CONSTRUCTORS-END========
    
    // ========POSITION-AND-SIZE========
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
     * Position system design:
     *
     * Components only store their relative position to reduce complexity.
     * World position is calculated on-demand by traversing up the parent chain.
     * This avoids duplicate tracking and ensures positions stay in sync.
     *
     * Example:
     *   Root: local(0, 0) -> world(0, 0)
     *   Child1: local(1, 1) -> world = Parent.world + (1, 1) = (1, 1)
     *   Child2: local(1, 1) -> world = Child1.world + (1, 1) = (2, 2)
     *
     * Position caching with dirty flags prevents unnecessary recalculations.
     * When a parent moves, all children are marked dirty and recalculate on next access.
     *
     * Note: This could be refactored into a separate Transform or LayoutManager class.
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
    // ========POSITION-AND-SIZE-END========

    // ========ANIMATION-MANAGEMENT========
    public void AddAnimation(Animation animation)
    {
        Animations.Add(animation);
    }

    public void ClearAnimations()
    {
        Animations.Clear();
    }
    // ========ANIMATION-MANAGEMENT-END========
    
    
    // ========CHILD-HIERARCHY========
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
            // Create snapshot before clearing to properly update each child's parent reference
            var childrenSnapshot = Children.ToArray();
            Children.Clear();
            _childrenDirty = true;

            foreach (var child in childrenSnapshot)
            {
                child.Parent = null;
                child.MarkWorldPositionDirty();
            }
        }
    }

    private GraphicsComponent[] GetChildrenSnapshot()
    {
        // Double-checked locking pattern to minimize lock contention
        if (_childrenDirty)
        {
            lock (_childrenLock)
            {
                if (_childrenDirty)
                {
                    _cachedChildren = Children.ToArray();
                    _childrenDirty = false;
                }
            }
        }
        return _cachedChildren;
    }
    // ========CHILD-HIERARCHY-END========

    // ========RENDERING-AND-UPDATE========
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
            var animation = Animations[i];
            animation.OnUpdate(deltaTime);
            if (animation.IsComplete)
            {
                Animations.Remove(animation);
            }
        }
        
        UpdateSelf();
        
        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
        {
            child.Update(deltaTime);
        }
    }
    
    protected virtual void UpdateSelf()
    {
    }
    // ========RENDERING-AND-UPDATE-END========
}