namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Base interface for all game systems.
/// Systems contain game logic and operate on entities/components.
/// </summary>
public interface IGameSystem
{
    /// <summary>
    /// Update the system logic.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in milliseconds</param>
    void Update(long deltaTime);
}
