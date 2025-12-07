using System.Drawing;
using System.Numerics;

namespace ConsoleGameEngine.Engine.Renderer.Geometry;

/*
 * This class handles the 2D transformation of the objects in space
 */
public class Transform2D
{
    /*
     * Coordinate System
     * Position
     * Translate
     * 
     * Position1->Position2
     */

    private Point2D _relativePosition;
    private Point2D _worldPosition;

    public Point2D RelativePosition => _relativePosition;
    public Point2D WorldPosition => _worldPosition;
    

    public Transform2D()
    {
    }

    public Transform2D(Point2D _relativePosition, Point2D relativePosition)
    {
        _relativePosition = relativePosition;
        _relativePosition = relativePosition;
    }

    public bool IsTransformPossible()
    {
        if (_worldPosition == null) return false;
        return true;
    }

    public Point2D TransformToWorld()
    {
        return _worldPosition + _relativePosition;
    }
}