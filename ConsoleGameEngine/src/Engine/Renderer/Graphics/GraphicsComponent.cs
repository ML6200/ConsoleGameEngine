using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Animations;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

/// <summary>
/// Base class for all renderable graphics components in the engine.
/// Implements a hierarchical component system where each component can have one parent and multiple children.
/// </summary>
/// <remarks>
/// <para>
/// Position System:
/// Components store their position relative to their parent. World position is calculated on-demand
/// by traversing up the parent chain. This avoids duplicate tracking and keeps positions in sync.
/// </para>
/// <para>
/// Example hierarchy:
/// <code>
///   Root (0, 0)
///    |
///   Child1 (1, 1) -> renders at (0+1, 0+1) = (1, 1)
///    |
///   Child2 (1, 1) -> renders at (1+1, 1+1) = (2, 2)
/// </code>
/// </para>
/// <para>
/// Position caching with dirty flags prevents unnecessary recalculations.
/// When a parent moves, all children are marked dirty and recalculate on next access.
/// </para>
///
/// For further information see: <see cref="RootComponent"/>>
/// </remarks>
public abstract class GraphicsComponent : IRenderable
{
    protected int Width;
    protected int Height;

    /// <summary>
    /// Gets or sets whether this component and its children should be rendered.
    /// </summary>
    public virtual bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the background color for this component.
    /// </summary>
    public ConsoleColor BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets the foreground (text) color for this component.
    /// </summary>
    public ConsoleColor ForegroundColor { get; set; }

    /// <summary>
    /// Gets or sets the border color for this component.
    /// </summary>
    public ConsoleColor BorderColor { get; set; }

    /// <summary>
    /// The list of animations currently running on this component.
    /// </summary>
    private readonly List<Animation> _animations  = [];

    /// <summary>
    /// Gets the list of children of this component.
    /// </summary>
    public GraphicsComponent[] Children => GetChildrenSnapshot();
    
    /// <summary>
    /// Gets the parent of children of this component.
    /// </summary>
    public IRenderable? Parent { get; private set; }
    
    private readonly List<GraphicsComponent> _children = [];

    private GraphicsComponent[] _cachedChildren = [];
    private Point2D _relativePosition = new(0, 0);
    private Point2D _cachedWorldPosition = new(0, 0);
    private bool _isPositionDirty = true;
    private bool _childrenDirty = true;
    private readonly Lock _childrenLock = new();

    /// <summary>
    /// Gets the current console window size in characters.
    /// </summary>
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);
    
        
    private IComponentObserver? ComponentObserver {get; set;}

    /// <summary>
    /// Sets the component observer for this component and all its descendants.
    /// This is typically called on the root component to wire up the UI manager.
    /// </summary>
    public void SetObserver(IComponentObserver? observer)
    {
        ComponentObserver = observer;

        // Notify for this component
        observer?.OnComponentAdded(this);

        // Propagate to all descendants
        PropagateObserver(this, observer);
    }
    
    // ========CONSTRUCTORS========
    /// <summary>
    /// Initializes a new graphics component with full customization.
    /// </summary>
    /// <param name="width">The width of the component in characters.</param>
    /// <param name="height">The height of the component in characters.</param>
    /// <param name="relativePosition">The position relative to the parent component. Defaults to (0, 0) if null.</param>
    /// <param name="backgroundColor">The background color for this component.</param>
    /// <param name="foregroundColor">The foreground (text) color for this component.</param>
    /// <param name="borderColor">The border color for this component.</param>
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

    /// <summary>
    /// Initializes a new graphics component with size and position.
    /// </summary>
    /// <param name="width">The width of the component in characters.</param>
    /// <param name="height">The height of the component in characters.</param>
    /// <param name="relativePosition">The position relative to the parent component. Defaults to (0, 0) if null.</param>
    protected GraphicsComponent(int width, int height,
        Point2D? relativePosition)
    {
        Width = width;
        Height = height;
        _relativePosition = relativePosition ?? new Point2D(0, 0);
    }

    /// <summary>
    /// Initializes a new graphics component with default values.
    /// Position defaults to (0, 0).
    /// </summary>
    protected GraphicsComponent()
    {
        // _relativePosition defaults to (0, 0) via field initializer
    }
    
    // ========CONSTRUCTORS-END========
    
    // ========POSITION-AND-SIZE========
    /// <summary>
    /// Gets or sets the size of this component in characters.
    /// </summary>
    public Dimension2D Size
    {
        get => new(Width, Height);
        set
        {
            Width = value.Width;
            Height = value.Height;
        }
    }

    /// <summary>
    /// Gets or sets the absolute position of this component in screen space.
    /// </summary>
    /// <remarks>
    /// World position is calculated by traversing up the parent chain and adding relative positions.
    /// Setting this property will update the relative position based on the parent's world position.
    /// Position values are cached and recalculated only when marked dirty.
    /// </remarks>
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

    private void PropagateObserver(GraphicsComponent component, IComponentObserver? observer)
    {
        foreach (var child in component.Children)
        {
            child.ComponentObserver = observer;
            observer?.OnComponentAdded(child);
            PropagateObserver(child, observer);
        }
    }

    /// <summary>
    /// Gets or sets the position of this component relative to its parent.
    /// </summary>
    /// <remarks>
    /// When set, this marks the world position as dirty, triggering recalculation on next access.
    /// All child components are also marked dirty to maintain position hierarchy consistency.
    /// </remarks>
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
        foreach (var child in _children)
        {
            child.MarkWorldPositionDirty();
        }
    }
    // ========POSITION-AND-SIZE-END========

    // ========ANIMATION-MANAGEMENT========
    /// <summary>
    /// Adds an animation to this component.
    /// </summary>
    /// <param name="animation">The animation to add.</param>
    public void AddAnimation(Animation animation)
    {
        _animations.Add(animation);
    }

    /// <summary>
    /// Removes all animations from this component.
    /// </summary>
    public void ClearAnimations()
    {
        _animations.Clear();
    }
    // ========ANIMATION-MANAGEMENT-END========
    
    
    // ========CHILD-HIERARCHY========
    /// <summary>
    /// Adds a child component to this component's hierarchy.
    /// </summary>
    /// <param name="child">The child component to add.</param>
    /// <remarks>
    /// This operation is thread-safe. The child's parent reference is automatically set,
    /// and all positions in the hierarchy are marked dirty for recalculation.
    /// </remarks>
    public void AddChild(GraphicsComponent child)
    {
        lock (_childrenLock)
        {
            _children.Add(child);
            _childrenDirty = true;
            child.Parent = this;
            MarkWorldPositionDirty();

            // Set observer on the child and propagate to its descendants
            child.ComponentObserver = ComponentObserver;
            ComponentObserver?.OnComponentAdded(child);
            PropagateObserver(child, ComponentObserver);
        }
    }

    /// <summary>
    /// Removes a child component from this component's hierarchy.
    /// </summary>
    /// <param name="child">The child component to remove.</param>
    /// <remarks>
    /// This operation is thread-safe. The child's parent reference is automatically cleared,
    /// and the child's position is marked dirty for recalculation.
    /// </remarks>
    public void RemoveChild(GraphicsComponent child)
    {
        lock (_childrenLock)
        {
            _children.Remove(child);
            _childrenDirty = true;
            child.Parent = null;
            child.MarkWorldPositionDirty();

            // Notify observer and clear from child tree
            ComponentObserver?.OnComponentRemoved(child);
            child.ComponentObserver = null;
            PropagateObserver(child, null);
        }
    }

    /// <summary>
    /// Removes all child components from this component's hierarchy.
    /// </summary>
    /// <remarks>
    /// This operation is thread-safe. All children's parent references are cleared,
    /// and their positions are marked dirty for recalculation.
    /// </remarks>
    public void RemoveAllChildren()
    {
        lock (_childrenLock)
        {
            // Create snapshot before clearing to properly update each child's parent reference
            var childrenSnapshot = _children.ToArray();
            _children.Clear();
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
                    _cachedChildren = _children.ToArray();
                    _childrenDirty = false;
                }
            }
        }
        return _cachedChildren;
    }
    // ========CHILD-HIERARCHY-END========
    

    /// <summary>
    /// Renders this specific component. Override this method to implement custom rendering logic.
    /// </summary>
    /// <param name="renderer">The renderer to use for drawing.</param>
    /// <param name="camera">The camera defining the view.</param>
    /// <param name="screenPoint"></param>
    public virtual void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
    }

    /// <summary>
    /// Updates this component and all its children.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    /// <remarks>
    /// Updates all animations first, removing completed ones.
    /// Then calls UpdateSelf() for component-specific logic.
    /// Finally updates all children using a thread-safe snapshot.
    /// </remarks>
    public void Update(double deltaTime)
    {
        for (int i = _animations.Count - 1; i >= 0; i--)
        {
            var animation = _animations[i];
            animation.OnUpdate(deltaTime);
            if (animation.IsComplete)
            {
                _animations.Remove(animation);
            }
        }
        
        UpdateSelf();
        
        var childrenSnapshot = GetChildrenSnapshot();
        foreach (var child in childrenSnapshot)
        {
            child.Update(deltaTime);
        }
    }

    /// <summary>
    /// Updates this specific component's logic. Override this method to implement custom update behavior.
    /// </summary>
    protected virtual void UpdateSelf()
    {
    }
    // ========RENDERING-AND-UPDATE-END========
}