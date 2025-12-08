using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Handles AI behavior for demons including state updates and decision making.
/// Demons can be in Idle, Move, or Attack states based on distance to player.
/// </summary>
public class ControlSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly CombatSystem _combatSystem;

    public ControlSystem(DoomGameScene game, CombatSystem combatSystem)
    {
        _game = game;
        _combatSystem = combatSystem;
    }

    public void Update(double deltaTime)
    {
        // Convert deltaTime from seconds to milliseconds
        double deltaTimeMs = deltaTime * 1000.0;

        foreach (Demon demon in _game.Demons)
        {
            // Update demon state based on player position
            demon.LastDistanceToPlayer = Point2D.Distance(_game.Player.WorldPosition, demon.WorldPosition);
            demon.UpdateState(_game.Player);  // Use cached distance

            // Update attack cooldown timer
            demon.UpdateAttackCooldown(deltaTimeMs);

          
            if (demon.State == DemonState.Attack 
                && demon.CanAttack())
            {
                _combatSystem.DemonAttack(demon);
                demon.ResetAttackCooldown();
            }
        }
    }
}
