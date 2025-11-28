using System.Collections.Generic;
using ConsoleGameEngine.Engine.Renderer.Animations;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay.Systems;

/// <summary>
/// Handles all entity interactions including item pickups,
/// door opening, level exits, and environmental hazards.
/// </summary>
public class InteractionSystem : IGameSystem
{
    private readonly DoomGameScene _game;

    public InteractionSystem(DoomGameScene game)
    {
        _game = game;
    }

    public void Update(long deltaTime)
    {
        // Process automatic (indirect) interactions
        ProcessPlayerIndirectInteractions();
        ProcessDemonIndirectInteractions();
    }

    /// <summary>
    /// Process direct interactions (e.g., opening doors, using exits).
    /// Called when player presses interaction key.
    /// </summary>
    public void ProcessPlayerDirectInteraction()
    {
        List<GameItem> nearbyItems = GetItemsWithinRange(_game.Player.AbsolutePosition, 1);

        foreach (GameItem item in nearbyItems)
        {
            switch (item.Type)
            {
                case ItemType.DOOR:
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.Door);

                    // Door animation - quick blink
                    var doorAnim = AnimationTween.Blink(item, 0.3, loop: false);
                    item.AddAnimation(doorAnim);
                    break;

                case ItemType.LEVELEXIT:
                    item.Interact();
                    _game.Exited = true;
                    _game.PlaySoundEffect(SoundEffectType.LevelExit);
                    break;
            }
        }
    }

    /// <summary>
    /// Process automatic interactions (pickups, toxic waste).
    /// </summary>
    private void ProcessPlayerIndirectInteractions()
    {
        List<GameItem> itemsAtPosition = GetItemsWithinRange(_game.Player.AbsolutePosition, 0);

        foreach (GameItem item in itemsAtPosition)
        {
            switch (item.Type)
            {
                case ItemType.AMMO:
                    _game.Player.PickUpAmmo(5);
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.ItemPickup);

                    // Pickup animation
                    var pickupAnim = AnimationTween.Blink(item, 0.3, loop: false);
                    item.AddAnimation(pickupAnim);
                    break;

                case ItemType.BFGCELL:
                    if (_game.Player.BFGCells < _game.Player.MaxBFGCells)
                    {
                        _game.Player.PickUpBFGCell(1);
                        item.Interact();
                        _game.PlaySoundEffect(SoundEffectType.ItemPickup);

                        var cellPickupAnim = AnimationTween.Blink(item, 0.3, loop: false);
                        item.AddAnimation(cellPickupAnim);
                    }
                    break;

                case ItemType.MEDKIT:
                    _game.Player.PickUpHealth(25);
                    item.Interact();
                    _game.PlaySoundEffect(SoundEffectType.ItemPickup);

                    var medkitAnim = AnimationTween.Blink(item, 0.5, false);
                    item.AddAnimation(medkitAnim);
                    break;

                case ItemType.TOXICWASTE:
                    _game.Player.TakeDamage(5);
                    _game.PlaySoundEffect(SoundEffectType.Pain);

                    var toxicAnim = AnimationTween.Blink(_game.Player, 0.5, false);
                    _game.Player.AddAnimation(toxicAnim);
                    break;
            }
        }
    }

    /// <summary>
    /// Process demon interactions with environment (e.g., toxic waste).
    /// </summary>
    private void ProcessDemonIndirectInteractions()
    {
        foreach (Demon demon in _game.Demons)
        {
            List<GameItem> itemsAtPosition = GetItemsWithinRange(demon.AbsolutePosition, 0);

            foreach (GameItem item in itemsAtPosition)
            {
                if (item.Type == ItemType.TOXICWASTE)
                {
                    var toxicAnim = AnimationTween.Blink(demon, 0.3);
                    demon.AddAnimation(toxicAnim);
                    
                    demon.TakeDamage(5);
                }
            }
        }
    }

    private List<GameItem> GetItemsWithinRange(Position2D position, double range)
    {
        List<GameItem> result = new List<GameItem>();

        foreach (GameItem item in _game.Items)
        {
            double distance = Position2D.Distance(position, item.AbsolutePosition);
            if (distance <= range)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
