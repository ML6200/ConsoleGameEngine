using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Handles AI behavior for demons including state updates and decision making.
/// Demons can be in Idle, Move, or Attack states based on distance to player.
/// </summary>
public class AISystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly CombatSystem _combatSystem;

    public AISystem(DoomGameScene game, CombatSystem combatSystem)
    {
        _game = game;
        _combatSystem = combatSystem;
    }

    public void Update(long deltaTime)
    {
        foreach (Demon demon in _game.Demons)
        {
            // Update demon state based on player position
            demon.UpdateState(_game.Player);

            // Update attack cooldown timer
            demon.UpdateAttackCooldown(deltaTime);

            // Execute behavior based on state
            switch (demon.State)
            {
                case DemonState.Idle:
                    // Do nothing, demon is too far from player
                    break;

                case DemonState.Move:
                    // Movement is handled by MovementSystem
                    // AI just needs to set the state
                    break;

                case DemonState.Attack:
                    // Demon is close enough to attack, but check cooldown
                    if (demon.CanAttack())
                    {
                        _combatSystem.DemonAttack(demon);
                        demon.ResetAttackCooldown();
                    }
                    break;
            }
        }
    }
}
