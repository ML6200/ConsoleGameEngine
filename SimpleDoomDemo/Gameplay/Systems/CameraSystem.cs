using System;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Camera system that follows the player and creates a scrolling viewport.
/// The camera centers the player on screen by offsetting all world entities.
/// </summary>
public class CameraSystem
{
    private readonly Player _player;
    private readonly ConsoleGraphicsPanel _viewport;
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private Position2D _lastCameraPosition;
    private const float SMOOTHING = 0.15f; // Lower = smoother but more lag

    public CameraSystem(Player player, ConsoleGraphicsPanel viewport, int screenWidth, int screenHeight)
    {
        _player = player;
        _viewport = viewport;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _lastCameraPosition = new Position2D(0, 0);
    }

    /// <summary>
    /// Update camera position to follow the player.
    /// Centers the player on screen by moving the viewport panel.
    /// Uses the player's RelativePosition (world coordinates) directly.
    /// </summary>
    public void UpdateCamera()
    {
        // Get player's world position (RelativePosition within viewport)
        Position2D playerWorldPos = _player.RelativePosition;

        // Calculate screen center
        int centerX = _screenWidth / 2;
        int centerY = _screenHeight / 2;

        // Calculate target viewport offset to center player
        // Viewport position = Center of screen - Player world position
        int targetX = centerX - playerWorldPos.X;
        int targetY = centerY - playerWorldPos.Y;

        // Smooth camera movement (interpolate between last and target position)
        int smoothX = (int)(_lastCameraPosition.X + (targetX - _lastCameraPosition.X) * SMOOTHING);
        int smoothY = (int)(_lastCameraPosition.Y + (targetY - _lastCameraPosition.Y) * SMOOTHING);

        // For instant snapping (no smoothing), use this instead:
        // int smoothX = targetX;
        // int smoothY = targetY;

        Position2D newCameraPos = new Position2D(smoothX, smoothY);
        _lastCameraPosition = newCameraPos;

        // Update viewport position (this moves all children with it)
        _viewport.RelativePosition = newCameraPos;
    }

    /// <summary>
    /// Snap camera to player immediately without smoothing.
    /// </summary>
    public void SnapToPlayer()
    {
        Position2D playerWorldPos = _player.RelativePosition;
        int centerX = _screenWidth / 2;
        int centerY = _screenHeight / 2;
        int targetX = centerX - playerWorldPos.X;
        int targetY = centerY - playerWorldPos.Y;

        Position2D snapPos = new Position2D(targetX, targetY);
        _lastCameraPosition = snapPos;
        _viewport.RelativePosition = snapPos;
    }

    /// <summary>
    /// Get the camera bounds in world coordinates.
    /// Useful for culling entities outside the visible area.
    /// </summary>
    public (Position2D topLeft, Position2D bottomRight) GetCameraBounds()
    {
        Position2D playerPos = _player.RelativePosition;

        //int halfWidth = _screenWidth / 2;
        //int halfHeight = _screenHeight / 2;
        int sightRange = _player.SightRange;

        Position2D topLeft = new Position2D(
            playerPos.X - sightRange,
            playerPos.Y - sightRange
        );

        Position2D bottomRight = new Position2D(
            playerPos.X + sightRange,
            playerPos.Y + sightRange
        );

        return (topLeft, bottomRight);
    }

    /// <summary>
    /// Check if a world position is visible on screen.
    /// </summary>
    public bool IsInView(Position2D worldPosition)
    {
        var (topLeft, bottomRight) = GetCameraBounds();

        return worldPosition.X >= topLeft.X &&
               worldPosition.X <= bottomRight.X &&
               worldPosition.Y >= topLeft.Y &&
               worldPosition.Y <= bottomRight.Y;
    }
}
