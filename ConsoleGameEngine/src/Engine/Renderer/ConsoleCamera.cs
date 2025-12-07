using System.Numerics;
using ConsoleGameEngine.Engine.Renderer.Geometry;

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

    public ConsoleCamera(Dimension2D worldSize, Point2D cameraStartPoint, Dimension2D cameraSize)
    {
        WorldSize = worldSize;
        CameraStartPoint = cameraStartPoint;
        CameraSize = cameraSize;
       
        int endX = cameraStartPoint.X + cameraSize.Width;
        int endY = cameraStartPoint.Y + cameraSize.Height;
        CameraEndPoint = new Point2D(endX, endY);
    }

    public Point2D? TransformPoint(Point2D worldPoint)
    {
        // Transform from world space to screen space
        int screenX = worldPoint.X - CameraStartPoint.X;
        int screenY = worldPoint.Y - CameraStartPoint.Y;

        // Check if point is within camera viewport (culling)
        if (screenX < 0 || screenY < 0 ||
            screenX >= CameraSize.Width ||
            screenY >= CameraSize.Height)
        {
            return null;
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
}