using System;
using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Scenes;
using SimpleDoomEngine;

namespace SimpleDoomDemo.Gameplay.Systems;

public class CombatSystem : IGameSystem
{
    private readonly DoomGameScene _game;
    private readonly Random _random = new();

    public CombatSystem(DoomGameScene game)
    {
        _game = game;
    }

    public void Update(double deltaTime)
    {
    }
    
    public void PlayerAttack()
    {
        if (_game.Player.Ammo <= 0)
            return;

        _game.Player.Shoot();
        _game.PlaySoundEffect(SoundEffectType.Shotgun);

        var attackAnim = Animation.Blink(_game.Player, 0.2, loop: false);
        _game.Player.AddAnimation(attackAnim);

        List<Demon> nearbyDemons = GetDemonsWithinRange(_game.Player.WorldPosition, _game.Player.SightRange);

        foreach (Demon demon in nearbyDemons)
        {
            int u = _random.Next(35, 106);
            int distance = Point2D.ChebyshevDistance(demon.WorldPosition, _game.Player.WorldPosition);
            int damage = 2 * u / (1 + distance);

            DealDamageToDemon(demon, damage);
        }
    }
    
    public void PlayerBFGAttack()
    {
        if (_game.Player.BfgCells <= 0)
            return;

        _game.Player.ShootBFG();
        _game.PlaySoundEffect(SoundEffectType.BFG);
        
        var bfgAnim = Animation.Blink(_game.Player, 0.2, loop: false);
        _game.Player.AddAnimation(bfgAnim);

        List<Demon> nearbyDemons = GetDemonsWithinRange(_game.Player.WorldPosition, _game.Player.SightRange);

        foreach (Demon demon in nearbyDemons)
        {
            int damage = _random.Next(100, 801);
            DealDamageToDemon(demon, damage);

            // Trigger explosion animation on hit demons
            var explosionAnim = Animation.Blink(demon, 150, loop: false);
            demon.AddAnimation(explosionAnim);
        }
    }
    
    public void DemonAttack(Demon demon)
    {
        int min, max;
        demon.GetAttackDamageRange(out min, out max);
        int u = _random.Next(min, max);

        int distance = (int) Point2D.ChebyshevDistance(demon.WorldPosition, _game.Player.WorldPosition);
        int damage = 2 * u / (1 + distance);

        DealDamageToPlayer(damage);
    }

    private void DealDamageToDemon(Demon demon, int damage)
    {
        demon.TakeDamage(damage);

        if (!demon.Alive)
        {
            _game.Player?.AddCombatPoints(demon.GetCombatPoints());
            
            var deathAnim = Animation.Blink(demon, 0.5, loop: false);
            demon.AddAnimation(deathAnim);
        }
        else
        {
            var hitAnim = Animation.Blink(demon, 0.2, loop: false);
            demon.AddAnimation(hitAnim);
        }
    }

    private void DealDamageToPlayer(int damage)
    {
        _game.Player.TakeDamage(damage);
        _game.PlaySoundEffect(SoundEffectType.Pain);

        if (_game.Player.Alive)
        {
            var painAnim = Animation.Blink(_game.Player, 0.3, loop: false);
            _game.Player.AddAnimation(painAnim);
        }
    }

    private List<Demon> GetDemonsWithinRange(Point2D point, double range)
    {
        List<Demon> result = new List<Demon>();

        foreach (Demon demon in _game.Demons)
        {
            double distance = Point2D.ChebyshevDistance(point, demon.WorldPosition);
            if (distance <= range)
            {
                result.Add(demon);
            }
        }

        return result;
    }
}
