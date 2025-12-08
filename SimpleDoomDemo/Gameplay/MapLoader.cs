using System;
using System.Collections.Generic;
using System.IO;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay;

public class MapLoader
{
    // Doom CSV Map format
    struct DcmFormat
    {
        public Point2D Position;
        public DcmType Type;
        public string Value;
        
        public DcmFormat(Point2D position, DcmType type, string value)
        {
            this.Position = position;
            this.Type = type;
            this.Value = value;
        }
    }

    enum DcmType
    {
        GameItem, Demon, Player, Unknown
    }

    enum DcmEntity
    {
        Ammo, BFGCELL, DOOR, LEVELEXIT, WALL, TOXICWASTE, MEDKIT,
        Zombieman, Mancubus, Imp,
        Player
    }

    private List<DcmFormat> _dcmList = new();
    
    public void LoadFromDcmf(string path)
    {
        using StreamReader reader = new StreamReader(path);
        
        string? line;
        try
        {
            while ((line = reader.ReadLine()) != null)
            {
                string[] columns = line.Split(";");
                if (columns.Length != 3) throw new Exception("Invalid map file format");

                _dcmList.Add(new DcmFormat
                {
                    Position = new Point2D(int.Parse(columns[0]), int.Parse(columns[1])),
                    Type = ParseDcmfType(columns[2]),
                    Value = columns[3]
                });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            reader.Close();
        }
    }
    

    private DcmType ParseDcmfType(string type)
    {
        switch (type)
        {
            case "GI": return DcmType.GameItem; 
            case "P": return DcmType.Player;
            case "D": return DcmType.Demon;
            default: return DcmType.Unknown;
        }
    }
    
    public void LoadFromLegacyMap(string path, 
        out List<GameItem> Items, 
        out List<Demon> Demons, 
        out Player Player)
    {
        Items = null;
        Demons = null;
        Player = null;
        try
        {
            string[] lines = File.ReadAllLines(path);
            string[] firstLine = lines[0].Split(",");
            int y = int.Parse(firstLine[0]);
            int x = int.Parse(firstLine[1]);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                for (int j = 0; j < x; j++)
                {
                    switch (line[j])
                    {
                        // ITEMS
                        case 'A':
                            Items.Add(new GameItem(j, i - 1, ItemType.AMMO));
                            break;
                        case 'B':
                            Items.Add(new GameItem(j, i - 1, ItemType.BFGCELL));
                            break;
                        case 'D':
                            Items.Add(new GameItem(j, i - 1, ItemType.DOOR));
                            break;
                        case 'E':
                            Items.Add(new GameItem(j, i - 1, ItemType.LEVELEXIT));
                            break;
                        case 'M':
                            Items.Add(new GameItem(j, i - 1, ItemType.MEDKIT));
                            break;
                        case 'T':
                            Items.Add(new GameItem(j, i - 1, ItemType.TOXICWASTE));
                            break;
                        case 'W':
                            Items.Add(new GameItem(j, i - 1, ItemType.WALL));
                            break;

                        // DEMONS
                        case 'z':
                            Demons.Add(new Zombieman(j, i - 1));
                            break;
                        case 'i':
                            Demons.Add(new Imp(j, i - 1));
                            break;
                        case 'm':
                            Demons.Add(new Mancubus(j, i - 1));
                            break;

                        // PLAYER
                        case 'p':
                            Player.WorldPosition = new Point2D(j, i - 1);
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}