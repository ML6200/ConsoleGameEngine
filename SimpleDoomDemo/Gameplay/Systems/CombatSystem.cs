using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine;
using SimpleDoomEngine.Gameplay.Actors;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Handles all combat logic including player attacks, demon attacks,
/// damage calculations, and combat animations.
/// </summary>
public class CombatSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly Random _random = new Random();

    public CombatSystem(DoomGameScene game)
    {
        _game = game;
    }

    public void Update(double deltaTime)
    {
        // Combat updates happen on-demand (when player/demon attacks)
        // No continuous updates needed here
    }

    /// <summary>
    /// Player performs shotgun attack with animation.
    /// </summary>
    public void PlayerAttack()
    {
        if (_game.Player.Ammo <= 0)
            return;

        _game.Player.Shoot();
        _game.PlaySoundEffect(SoundEffectType.Shotgun);

        // Trigger attack animation on player
        var attackAnim = AnimationTween.Blink(_game.Player, 0.2, loop: false);
        _game.Player.AddAnimation(attackAnim);

        List<Demon> nearbyDemons = GetDemonsWithinRange(_game.Player.WorldPosition, _game.Player.SightRange);

        foreach (Demon demon in nearbyDemons)
        {
            int u = _random.Next(35, 106);
            int distance = (int)Position2D.Distance(demon.WorldPosition, _game.Player.WorldPosition);
            int damage = 2 * u / (1 + distance);

            DealDamageToDemon(demon, damage);
        }
    }

    /// <summary>
    /// Player performs BFG attack with animation.
    /// </summary>
    public void PlayerBFGAttack()
    {
        if (_game.Player.BFGCells <= 0)
            return;

        _game.Player.ShootBFG();
        _game.PlaySoundEffect(SoundEffectType.BFG);

        // Trigger BFG animation (longer blink)
        var bfgAnim = AnimationTween.Blink(_game.Player, 200, loop: false);
        _game.Player.AddAnimation(bfgAnim);

        List<Demon> nearbyDemons = GetDemonsWithinRange(_game.Player.WorldPosition, _game.Player.SightRange);

        foreach (Demon demon in nearbyDemons)
        {
            int damage = _random.Next(100, 801);
            DealDamageToDemon(demon, damage);

            // Trigger explosion animation on hit demons
            var explosionAnim = AnimationTween.Blink(demon, 150, loop: false);
            demon.AddAnimation(explosionAnim);
        }
    }

    /// <summary>
    /// Demon attacks player with animation.
    /// </summary>
    public void DemonAttack(Demon demon)
    {
        int min, max;
        demon.GetAttackDamageRange(out min, out max);
        int u = _random.Next(min, max);

        int distance = (int)Position2D.Distance(demon.WorldPosition, _game.Player.WorldPosition);
        int damage = u / (1 + distance);

        // Trigger attack animation on demon
        var attackAnim = AnimationTween.Blink(demon, 100, loop: false);
        demon.AddAnimation(attackAnim);

        DealDamageToPlayer(damage);
    }

    private void DealDamageToDemon(Demon demon, int damage)
    {
        demon.TakeDamage(damage);

        if (!demon.Alive)
        {
            _game.Player.AddCombatPoints(demon.GetCombatPoints());

            // Death animation - fade out or blink rapidly
            var deathAnim = AnimationTween.Blink(demon, 0.2, loop: false);
            demon.AddAnimation(deathAnim);
        }
        else
        {
            // Hit animation
            var hitAnim = AnimationTween.Blink(demon, 0.2, loop: false);
            demon.AddAnimation(hitAnim);
        }
    }

    private void DealDamageToPlayer(int damage)
    {
        _game.Player.TakeDamage(damage);
        _game.PlaySoundEffect(SoundEffectType.Pain);

        if (_game.Player.Alive)
        {
            // Pain animation
            var painAnim = AnimationTween.Blink(_game.Player, 0.3, loop: false);
            _game.Player.AddAnimation(painAnim);
        }
    }

    private List<Demon> GetDemonsWithinRange(Position2D position, double range)
    {
        List<Demon> result = new List<Demon>();

        foreach (Demon demon in _game.Demons)
        {
            double distance = Position2D.Distance(position, demon.WorldPosition);
            if (distance <= range)
            {
                result.Add(demon);
            }
        }

        return result;
    }
}
