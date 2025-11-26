using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Engine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomEngine;

public class GameLogic
{
    private readonly Game _game;
    private Random RNG = new Random();

    public GameLogic(Game game)
    {
        _game = game;
    }

    private List<GameItem> GetGameItemsWithinDistance(Position2D position, double distanceTreshold)
    {
        List<GameItem> closeItems = new List<GameItem>();

        for (int i = 0; i < _game._items.Count; i++)
        {
            double distance = Position2D.Distance(position, _game._items[i].AbsolutePosition);

            if (distance <= distanceTreshold)
            {
                closeItems.Add(_game._items[i]);
            }
        }

        return closeItems;
    }

    private double GetTotalFillingRatio(Position2D position)
    {
        List<GameItem> items = GetGameItemsWithinDistance(position, 0);
        List<Demon> dems = GetDemonsWithinDistance(position, 0);
        
        double sum = 0;
        
        foreach (var t in items)
        {
            sum += t.FillingRatio;
        }
        
        for (int i = 0; i < dems.Count; i++)
        {
            sum += dems[i].FillingRatio;
        }
        
        return sum;
    }
    
    
    public List<Demon> GetDemonsWithinDistance(Position2D position, double distanceTreshold)
    {
        List<Demon> closeItems = new List<Demon>();

        for (int i = 0; i < _game.Demons.Count; i++)
        {
            double distance = Position2D.Distance(position, _game._demons[i].AbsolutePosition);

            if (distance <= distanceTreshold)
            {
                closeItems.Add(_game.Demons[i]);
            }
        }

        return closeItems;
    }

    public void Move(Player player, Position2D position)
    {
        double totalFillingRatio = GetTotalFillingRatio(position) + _game.FillingRatio;
        
        if (totalFillingRatio < 1.0)
        {
            player.RelativePosition = position;
        }
    }

    private void Move(Demon demon, Position2D position, long deltaTime)
    {
        double pMove = (demon.Speed / 100.0) * (deltaTime / 1000.0);
        double randomValue = RNG.NextDouble();
        
        if (randomValue < pMove)
        {
            double totalFillingRatio = GetTotalFillingRatio(position) + demon.FillingRatio;

            if (totalFillingRatio < 1.0)
            {
                demon.RelativePosition = position;
            }
        }
    }

    private void DemonMoveLogic(Demon demon, long deltaTime)
    {
        int x = demon.AbsolutePosition.X + RNG.Next(-1, 2);
        int y = demon.AbsolutePosition.Y + RNG.Next(-1, 2);
        Position2D targetPosition = new Position2D(x, y);

        if (ConsoleRenderer.IsPointWithinBounds(targetPosition)) Move(demon, targetPosition, deltaTime);
    }

    private void UpdateDemons(long deltaTime)
    {
        foreach (Demon demon in _game.Demons)
        {
            demon.UpdateState(_game.Player);
            if (demon.State == DemonState.Move)
            {
                DemonMoveLogic(demon, deltaTime);
            } else if (demon.State == DemonState.Attack)
            {
                DemonAttackLogic(demon);
            }

            DemonIndirectInteraction(demon);
        }
    }

    

    public void PlayerAttackLogic()
    {
        if (_game.Player.Ammo > 0)
        {
            _game.Player.Shoot();
            _game.PlaySoundEffect(SoundEffectType.Shotgun);

            List<Demon> nearbyDemons = GetDemonsWithinDistance(_game.Player.AbsolutePosition, _game.Player.SightRange);

            foreach (Demon demon in nearbyDemons)
            {
                int u = RNG.Next(35, 106);
                int distance = (int)Position2D.Distance(demon.AbsolutePosition, _game.Player.AbsolutePosition);
                int damage = 2 * u / (1 + distance);

                demon.TakeDamage(damage);

                if (!demon.Alive)
                {
                    _game.Player.AddCombatPoints(demon.GetCombatPoints());
                }
            }
        }
    }

    public void PlayerBFGAttackLogic()
    {
        if (_game.Player.BFGCells > 0)
        {
            _game.Player.ShootBFG();
            _game.PlaySoundEffect(SoundEffectType.BFG);

            List<Demon> nearbyDemons = GetDemonsWithinDistance(_game.Player.AbsolutePosition, _game.Player.SightRange);

            foreach (Demon demon in nearbyDemons)
            {
                int damage = RNG.Next(100, 801);
                demon.TakeDamage(damage);

                if (!demon.Alive)
                {
                    _game.Player.AddCombatPoints(demon.GetCombatPoints());
                }
            }
        }
    }

    public void DemonAttackLogic(Demon demon)
    {
        int min, max;
        demon.GetAttackDamageRange(out min, out max);
        int u = RNG.Next(min, max);

        int distance = (int)Position2D.Distance(demon.AbsolutePosition, _game.Player.AbsolutePosition);
        int damage = 2 * u / (1 + distance);

        _game.Player.TakeDamage(damage);
        _game.PlaySoundEffect(SoundEffectType.Pain);
    }
    
    private void CleanUpGameItems()
    {
        foreach (GameItem item in _game.Items.ToList())
        {
            if (!item.Available)
            {
                _game.Items.Remove(item);
            }
        }
    }
    
    private void CleanupDemons()
    {
        foreach (Demon d in _game.Demons.ToList())
        {
            if(!d.Alive) _game.Demons.Remove(d);
        }
    }

    public void PlayerDirectInteractionLogic()
    {
        List<GameItem> items = GetGameItemsWithinDistance(_game.Player.AbsolutePosition, 1);
        foreach (GameItem item in items)
        {
            switch (item.Type)
            {
                case ItemType.DOOR:
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.Door);
                    break;
                case ItemType.LEVELEXIT:
                {
                    item.Interact();
                    _game.Exited = true;
                    _game.PlaySoundEffect(SoundEffectType.LevelExit);
                }
                    break;
            }
        }
    }

    public void PlayerIndirectInteractionLogic()
    {
        List<GameItem> items = GetGameItemsWithinDistance(_game.Player.AbsolutePosition, 0);
        foreach (GameItem item in items)
        {
            switch (item.Type)
            {
                case ItemType.AMMO:
                    _game.Player.PickUpAmmo(5);
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.ItemPickup);
                    break;
                case ItemType.BFGCELL:
                    if (_game.Player.BFGCells < _game.Player.MaxBFGCells)
                    {
                        _game.Player.PickUpBFGCell(1);
                        item.Interact();
                        _game.PlaySoundEffect(SoundEffectType.ItemPickup);
                    }
                    break;
                case ItemType.MEDKIT:
                    _game.Player.PickUpHealth(25);
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.ItemPickup);
                    break;
                case ItemType.TOXICWASTE:
                    _game.Player.TakeDamage(5);
                    break;
            }
        }
    }

    private void DemonIndirectInteraction(Demon demon)
    {
        List<GameItem> items = GetGameItemsWithinDistance(demon.AbsolutePosition, 0);
        foreach (GameItem item in items)
        {
            if (item.Type == ItemType.TOXICWASTE) demon.TakeDamage(5);
        }
    }

    public void UpdateGameState(long deltaTime)
    {
        UpdateDemons(deltaTime);
        CleanUpGameItems();
        CleanupDemons();
    }
}