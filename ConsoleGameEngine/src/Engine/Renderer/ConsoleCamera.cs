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
    public Point2D CameraStartStartPoint { get; set; }
    public Point2D CameraEndPoint { get; private set; }
    public Dimension2D CameraSize { get; set; }
    public Dimension2D WorldSize { get; private set; }

    public ConsoleCamera(Dimension2D worldSize, Point2D cameraStartPoint, Dimension2D cameraSize)
    {
        WorldSize = worldSize;
        CameraStartStartPoint = cameraStartPoint;
        CameraSize = cameraSize;
       
        int endX = cameraStartPoint.X + cameraSize.Width;
        int endY = cameraStartPoint.Y + cameraSize.Height;
        CameraEndPoint = new Point2D(endX, endY);
    }

    public Point2D TransformPoint(Point2D sourcePoint)
    {
        return 
            new Point2D(sourcePoint.X - CameraStartStartPoint.X, 
                    sourcePoint.Y - CameraStartStartPoint.Y)
                .Clamp(CameraStartStartPoint, CameraEndPoint);
    }
}