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

    public CameraSystem(Player player, ConsoleGraphicsPanel viewport, int screenWidth, int screenHeight)
    {
        _player = player;
        _viewport = viewport;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    /// <summary>
    /// Update camera position to follow the player.
    /// Centers the player on screen by moving the viewport panel.
    /// </summary>
    public void UpdateCamera()
    {
        // Get player's absolute position in world coordinates
        Position2D playerWorldPos = _player.AbsolutePosition;

        // Calculate screen center
        int centerX = _screenWidth / 2;
        int centerY = _screenHeight / 2;

        // Calculate viewport offset to center player
        // Viewport position = Center of screen - Player position
        int viewportX = centerX - playerWorldPos.X;
        int viewportY = centerY - playerWorldPos.Y;

        // Update viewport position (this moves all children with it)
        _viewport.RelativePosition = new Position2D(viewportX, viewportY);
    }

    /// <summary>
    /// Get the camera bounds in world coordinates.
    /// Useful for culling entities outside the visible area.
    /// </summary>
    public (Position2D topLeft, Position2D bottomRight) GetCameraBounds()
    {
        Position2D playerPos = _player.AbsolutePosition;

        int halfWidth = _screenWidth / 2;
        int halfHeight = _screenHeight / 2;

        Position2D topLeft = new Position2D(
            playerPos.X - halfWidth,
            playerPos.Y - halfHeight
        );

        Position2D bottomRight = new Position2D(
            playerPos.X + halfWidth,
            playerPos.Y + halfHeight
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
