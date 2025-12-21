using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;

namespace ConsoleGameEngine.Engine.Renderer.Graphics;

/// <summary>
/// The root viewport component that serves as the top-level entry point for the rendering hierarchy.
/// This class is sealed and cannot be inherited.
/// </summary>
/// <remarks>
/// <para>
/// RootComponent acts as a viewport container and cannot have a parent, representing the
/// absolute top of the component hierarchy. It wraps a <see cref="Canvas"/> and
/// delegates all rendering and update operations to it.
/// </para>
/// <para>
/// Unlike <see cref="Canvas"/>, RootComponent implements <see cref="IRenderable"/>
/// directly and uses composition rather than inheritance. This design enforces that the root
/// cannot participate in parent-child relationships that regular components use.
/// </para>
/// <para>
/// The root component automatically matches the console window size and serves as the
/// coordinate system origin (0, 0) for all child components.
/// </para>
/// </remarks>
public sealed class RootComponent : IRenderable
{
    private readonly GraphicsComponent _canvas;

    /// <summary>
    /// Gets the wrapped graphics component that this root delegates to.
    /// </summary>
    public GraphicsComponent Canvas => _canvas;

    /// <summary>
    /// Gets the current console window size in characters.
    /// This defines the viewport dimensions for rendering.
    /// </summary>
    public Dimension2D ScreenSize => new(Console.WindowWidth, Console.WindowHeight);

    /// <summary>
    /// Gets or sets whether this root component and all its children should be rendered.
    /// </summary>
    /// <remarks>
    /// When false, the entire component hierarchy is skipped during rendering and updates.
    /// </remarks>
    public bool Visible
    {
        get; set;
    }

    // ========CONSTRUCTOR========
    /// <summary>
    /// Initializes a new root component wrapping the specified graphics component.
    /// </summary>
    /// <param name="canvas">The graphics component to use as the root's content.</param>
    /// <param name="uiManager"></param>
    /// <remarks>
    /// The root component starts visible by default. The wrapped component becomes the
    /// top-level parent in the rendering hierarchy.
    /// </remarks>
    public RootComponent(GraphicsComponent canvas)
    {
         if (canvas.Parent != null) 
             throw new ArgumentException("Root component cannot wrap a component that already has a parent.");
         
         _canvas = canvas;
         Visible = true;
    }
    
    // ========CONSTRUCTOR-END========

    // ========RENDERING-AND-UPDATE========
    /// <summary>
    /// Renders this root component and all its children to the screen.
    /// </summary>
    /// <param name="renderer">The renderer to use for drawing.</param>
    /// <param name="camera">The camera defining the view.</param>
    /// <param name="screenPoint"></param>
    /// <remarks>
    /// If the root is not visible, the entire hierarchy is skipped.
    /// This method delegates to the wrapped component's Compute method.
    /// </remarks>
    public void Draw(ConsoleRenderer2D renderer, Point2D screenPoint)
    {
        _canvas.Draw(renderer, screenPoint);
    }

    /// <summary>
    /// Updates this root component and all its children.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    /// <remarks>
    /// If the root is not visible, the entire hierarchy is skipped.
    /// This method delegates to the wrapped component's Update method.
    /// </remarks>
    public void Update(double deltaTime)
    {
        if (!Visible) return;

        _canvas.Update(deltaTime);
    }
    // ========RENDERING-AND-UPDATE-END========
}