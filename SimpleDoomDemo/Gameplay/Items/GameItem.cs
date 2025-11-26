using System;
using SimpleDoomEngine.Engine;

namespace SimpleDoomEngine.Gameplay.Items;

public class GameItem
{
    // =============================FIELDS_PUBLIC==============================
    public Position Position { get; }
    public ConsoleActor Actor { get; private set; }
    public ItemType Type { get; }
    public double FillingRatio { get; set; }
    public bool Available { get; private set; }

    
    // =============================METHODS==============================
    private void SetInitialProperties()
    {
        Available = true;
        switch (Type)
        {
            case ItemType.AMMO:
            {
                FillingRatio = 0.0;
                Actor = new ConsoleActor(ConsoleColor.Red, ConsoleColor.Yellow, 'A');
            }
                break;

            case ItemType.BFGCELL:
            {
                FillingRatio = 0.0;
                Actor = new ConsoleActor(ConsoleColor.Green, ConsoleColor.White, 'B');
            }
                break;
            case ItemType.DOOR:
            {
                Actor = new ConsoleActor(ConsoleColor.Gray, ConsoleColor.Yellow, '/');
            }
                break;
            case ItemType.LEVELEXIT:
            {
                FillingRatio = 1.0;
                Actor = new ConsoleActor(ConsoleColor.Blue, ConsoleColor.Black, 'E');
            }
                break;
            case ItemType.MEDKIT:
            {
                FillingRatio = 0.0;
                Actor = new ConsoleActor(ConsoleColor.DarkGray, ConsoleColor.Red, '+');
            }
                break;

            case ItemType.TOXICWASTE:
            {
                FillingRatio = 0.0;
                Actor = new ConsoleActor(ConsoleColor.Green, ConsoleColor.DarkGray, ':');
            }
                break;

            case ItemType.WALL:
            {
                FillingRatio = 1.0;
                Actor = new ConsoleActor(ConsoleColor.Gray, ConsoleColor.Gray, ' ');
            } break;
        }
    }

    public GameItem(int x, int y, ItemType type)
    {
        this.Position = new Position(x, y);
        this.Type = type;
        SetInitialProperties();
    }

    public void Interact()
    {
        if (Type == ItemType.AMMO
            || Type == ItemType.BFGCELL
            || Type == ItemType.MEDKIT)
        {
            Available = false;
        }

        else if (Type == ItemType.DOOR)
        {
            if (FillingRatio.Equals(1.0))
            {
                FillingRatio = 0.0;
                Actor = new ConsoleActor(ConsoleColor.Gray, ConsoleColor.DarkYellow, '/');
            }
            else
            {
                Actor = new ConsoleActor(ConsoleColor.Gray, ConsoleColor.Yellow, '/');
                FillingRatio = 1.0;
            }
        }
    }
}