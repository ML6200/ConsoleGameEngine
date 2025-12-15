using System;
using System.Numerics;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;

namespace ConsoleGameEngine.Engine.Renderer;

/*
 * WorldMatrix:
 * 000000100000000000...
 * 000000111000000000...
 * 000000100000000000...
 * 000000000000000000...
 * .
 * .
 * .
 *
 *
 * Camera:
 * 00010000
 * 00011100
 * 00010000
 *
 */
public class ConsoleCamera
{
    public Point2D CameraStartPoint { get; set; }
    public Point2D CameraEndPoint { get; private set; }
    public Dimension2D CameraSize { get; set; }
    public Dimension2D WorldSize { get; set; }
    
    private GraphicsComponent? _followedComponent;
    
    private float _cameraX;
    private float _cameraY;
    private const float CameraSmoothSpeed = 10.0f;
    

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

    private void InitializeCamera(ConsoleEngine engine)
    {
        // Setup world size and camera
        int worldWidth = engine.RootPanel().ScreenSize.Width * 3;
        int worldHeight = engine.RootPanel().ScreenSize.Height * 3;

        WorldSize = new Dimension2D(worldWidth, worldHeight);
    }

    public Point2D TransformPoint(Point2D worldPoint)
    {
        // Transform from world space to screen space
        int screenX = worldPoint.X - CameraStartPoint.X;
        int screenY = worldPoint.Y - CameraStartPoint.Y;

        // Check if point is within camera viewport (culling)
        if (screenX < 0 || screenY < 0 ||
            screenX >= CameraSize.Width ||
            screenY >= CameraSize.Height)
        {
            return Point2D.OutsideScreenPoint;
        }

        return new Point2D(screenX, screenY);
    }

    public void SetCameraPosition(Point2D cameraPosition)
    {
        Point2D target = cameraPosition.Clamp(0, WorldSize.Width - CameraSize.Width, 
            0,
            WorldSize.Height - CameraSize.Height);
        
        CameraStartPoint = target;
    }

    public void FollowObject(GraphicsComponent followedComponent)
    {
        _followedComponent = followedComponent;
        
        // Initialize camera position centered on player
        // this prevents lerping from wrong position on scene start
        int initialCameraX = _followedComponent.WorldPosition.X - CameraSize.Width / 2;
        int initialCameraY = _followedComponent.WorldPosition.Y - CameraSize.Height / 2;
        _cameraX = initialCameraX;
        _cameraY = initialCameraY;
        SetCameraPosition(new Point2D(initialCameraX, initialCameraY));
    }
    
    public void Follow(double deltaTime)
    {
        if (_followedComponent == null) return;
        
        // Smooth camera follow using lerp
        int px = _followedComponent.WorldPosition.X;
        int py = _followedComponent.WorldPosition.Y;
        int cw = CameraSize.Width;
        int ch = CameraSize.Height;
        
        float targetCameraX = px - cw / 4.0f;
        float targetCameraY = py - ch / 4.0f;
        
        // clamping value to avoid jitter
        float multiplier = Math.Min(CameraSmoothSpeed * (float)deltaTime, 1.0f);

        if (Math.Abs(targetCameraX - _cameraX) < 0.1f
            && Math.Abs(targetCameraY - _cameraY) < 0.1f)
        {
            _cameraX = targetCameraX;
            _cameraY = targetCameraY;
        }
        else
        {
            // Lerp camera position towards target (smoothing)
            _cameraX = AnimationTween.LerpForScalar(_cameraX, targetCameraX, multiplier);
            _cameraY = AnimationTween.LerpForScalar(_cameraY, targetCameraY, multiplier);
        }

        SetCameraPosition(new Point2D((int)_cameraX, (int)_cameraY));
    }
}
