using System;
using System.Numerics;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

/// <summary>
/// Camera system for rendering a viewport into a larger world space.
/// Provides viewport culling, smooth camera following, and world-to-screen coordinate transformation.
/// </summary>
/// <remarks>
/// <para>
/// The camera defines a rectangular viewport that views a portion of the world:
/// </para>
/// <code>
///   World Space(3xScreen):
///   000000100000000000...
///   000000111000000000...
///   000000100000000000...
///
///   Camera Viewport (inside the world):
///   00010000
///   00011100
///   00010000
/// </code>
/// <para>
/// The camera can smoothly follow a component with lerp-based movement for natural tracking.
/// </para>
/// </remarks>
public class ConsoleCamera
{
    /// <summary>
    /// Gets or sets the top-left corner position of the camera viewport in world space.
    /// </summary>
    public Point2D CameraStartPoint { get; set; }

    /// <summary>
    /// Gets the bottom-right corner position of the camera viewport in world space.
    /// </summary>
    public Point2D CameraEndPoint { get; private set; }

    /// <summary>
    /// Gets or sets the size of the camera viewport in characters.
    /// </summary>
    public Dimension2D CameraSize { get; set; }

    /// <summary>
    /// Gets or sets the total size of the world space in characters.
    /// </summary>
    public Dimension2D WorldSize { get; set; }

    private GraphicsComponent? _followedComponent;

    private float _cameraX;
    private float _cameraY;
    private const float CameraSmoothSpeed = 10.0f;


    // ========CONSTRUCTOR========
    /// <summary>
    /// Initializes a new camera with the specified world and viewport settings.
    /// </summary>
    /// <param name="engine">The engine instance used to determine world dimensions.</param>
    /// <param name="worldSize">The total size of the world space.</param>
    /// <param name="cameraStartPoint">The initial top-left position of the camera viewport.</param>
    /// <param name="cameraSize">The size of the camera viewport.</param>
    /// <param name="followedComponent">Optional component for the camera to follow.</param>
    public ConsoleCamera(ConsoleEngine engine,
        Dimension2D worldSize,
        Point2D cameraStartPoint,
        Dimension2D cameraSize,
        GraphicsComponent? followedComponent = null)
    {
        WorldSize = worldSize;
        CameraStartPoint = cameraStartPoint;
        CameraSize = cameraSize;
       
        int endX = cameraStartPoint.X + cameraSize.Width;
        int endY = cameraStartPoint.Y + cameraSize.Height;
        CameraEndPoint = new Point2D(endX, endY);
        
        _followedComponent = followedComponent;
        InitializeCamera(engine);
    }
    // ========CONSTRUCTOR-END========

    // ========INITIALIZATION========
    /// <summary>
    /// Initializes the camera by calculating world dimensions based on screen size.
    /// </summary>
    /// <param name="engine">The engine instance used to determine world dimensions.</param>
    private void InitializeCamera(ConsoleEngine engine)
    {
        // World is 3x the screen size in each dimension
        int worldWidth = engine.RootPanel().ScreenSize.Width * 3;
        int worldHeight = engine.RootPanel().ScreenSize.Height * 3;

        WorldSize = new Dimension2D(worldWidth, worldHeight);
    }
    // ========INITIALIZATION-END========

    // ========COORDINATE-TRANSFORMATION========
    /// <summary>
    /// Transforms a point from world space to screen space coordinates.
    /// </summary>
    /// <param name="worldPoint">The point in world space to transform.</param>
    /// <returns>
    /// The transformed point in screen space, or <see cref="Point2D.OutsideScreenPoint"/> if outside the viewport.
    /// </returns>
    /// <remarks>
    /// This method performs viewport culling, returning a special value for points outside the camera view.
    /// </remarks>
    public Point2D TransformPoint(Point2D worldPoint)
    {
        // Convert world coordinates to screen coordinates relative to camera position
        int screenX = worldPoint.X - CameraStartPoint.X;
        int screenY = worldPoint.Y - CameraStartPoint.Y;

        // Cull points outside the viewport
        if (screenX < 0 || screenY < 0 ||
            screenX >= CameraSize.Width ||
            screenY >= CameraSize.Height)
        {
            return Point2D.OutsideScreenPoint;
        }

        return new Point2D(screenX, screenY);
    }
    // ========COORDINATE-TRANSFORMATION-END========

    // ========CAMERA-POSITIONING========
    /// <summary>
    /// Sets the camera position, clamping it within world boundaries.
    /// </summary>
    /// <param name="cameraPosition">The desired camera position in world space.</param>
    /// <remarks>
    /// The position is automatically clamped to prevent the camera from viewing outside the world bounds.
    /// </remarks>
    public void SetCameraPosition(Point2D cameraPosition)
    {
        // Clamp camera position to keep viewport within world bounds
        Point2D target = cameraPosition.Clamp(0, WorldSize.Width - CameraSize.Width,
            0,
            WorldSize.Height - CameraSize.Height);

        CameraStartPoint = target;

        // Update end point to match new camera bounds
        int endX = CameraStartPoint.X + CameraSize.Width;
        int endY = CameraStartPoint.Y + CameraSize.Height;
        CameraEndPoint = new Point2D(endX, endY);
    }
    // ========CAMERA-POSITIONING-END========

    // ========CAMERA-FOLLOWING========
    /// <summary>
    /// Sets a component for the camera to follow and initializes the camera position centered on it.
    /// </summary>
    /// <param name="followedComponent">The component to follow.</param>
    /// <remarks>
    /// The camera is immediately centered on the component to prevent lerping from an incorrect initial position.
    /// </remarks>
    public void FollowObject(GraphicsComponent followedComponent)
    {
        _followedComponent = followedComponent;

        // Initialize camera position centered on the followed component
        // Prevents lerping from (0,0) on first frame
        int initialCameraX = _followedComponent.WorldPosition.X - CameraSize.Width / 2;
        int initialCameraY = _followedComponent.WorldPosition.Y - CameraSize.Height / 2;
        _cameraX = initialCameraX;
        _cameraY = initialCameraY;
        SetCameraPosition(new Point2D(initialCameraX, initialCameraY));
    }

    /// <summary>
    /// Updates the camera position to smoothly follow the tracked component.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since the last update in seconds.</param>
    /// <remarks>
    /// Uses linear interpolation (lerp) for smooth camera movement.
    /// The camera stops interpolating when very close to the target to prevent micro-jitter.
    /// </remarks>
    public void Follow(double deltaTime)
    {
        if (_followedComponent == null) return;

        // Calculate target camera position based on followed component
        int px = _followedComponent.WorldPosition.X;
        int py = _followedComponent.WorldPosition.Y;
        int cw = CameraSize.Width;
        int ch = CameraSize.Height;

        // Offset camera to keep component slightly off-center (1/4 from edges)
        float targetCameraX = px - cw / 4.0f;
        float targetCameraY = py - ch / 4.0f;

        // Clamp multiplier to prevent overshooting on large delta times
        float multiplier = Math.Min(CameraSmoothSpeed * (float)deltaTime, 1.0f);

        // Snap to target when very close to prevent micro-jitter
        if (Math.Abs(targetCameraX - _cameraX) < 0.1f
            && Math.Abs(targetCameraY - _cameraY) < 0.1f)
        {
            _cameraX = targetCameraX;
            _cameraY = targetCameraY;
        }
        else
        {
            // Smoothly interpolate towards target position
            _cameraX = AnimationTween.LerpForScalar(_cameraX, targetCameraX, multiplier);
            _cameraY = AnimationTween.LerpForScalar(_cameraY, targetCameraY, multiplier);
        }

        SetCameraPosition(new Point2D((int)_cameraX, (int)_cameraY));
    }
    // ========CAMERA-FOLLOWING-END========
}
