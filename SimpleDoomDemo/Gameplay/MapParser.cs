using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using ConsoleGameEngine.Engine.System;
using NLog;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomDemo.Gameplay.Scenes.Exceptions;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay;

public class MapParser
{
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public MapParser(string path)
    {
        LoadFromDcmfFile(path);
    }

    public MapParser()
    {
    }
    
    // Doom CSV Map format
    /*
     * PosX;PosY;Type;Val
     * 12;21;GI;A
     *
     * New Format idea for multiple levels:
     *
     * PosX;PosY;Type;Val;Param      <-header
     * 12;   21;  GI; E;   nextLevel.dcmf
     * ...
     * 
     */
    public struct DcmFormat : IEquatable<DcmFormat>
    {
        public Point2D Position;
        public DcmType Type;
        public DcmEntity Entity;
        public GraphicsComponent Value;


        public bool Equals(DcmFormat other)
        {
            return Position.Equals(other.Position) && Type == other.Type && Entity == other.Entity && Value.Equals(other.Value);
        }

        public override bool Equals(object? obj)
        {
            return obj is DcmFormat other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, (int)Type, (int)Entity, Value);
        }
    }

    public enum DcmType
    {
        GameItem, Demon, Player, Unknown
    }

    public enum DcmEntity
    {
        Ammo = 0, BfgCell = 1, Door = 2, LevelExit = 3, MedKit = 4,
        ToxicWaste = 5, Wall = 6, 
        
        Zombieman = 7, Mancubus = 8, Imp = 9,
        
        Player = 10,
        Unknown = 11
    }

    public readonly List<DcmFormat> DcmList = new();

    public void LoadFromDcmfFile(string path, bool ignoreMissing = false)
    {
        bool hasPlayer = false;
        bool hasExit = false;

        if (!HasCorrectExtension(path))
        {
            _logger.Warn("File does not have a propper extension." +
                         "The extension should be '*.dcmf'! or '*.map'");
            throw new Exception("The extension should be .dcmf or .map!");
        }

        StreamReader reader;
        string? line;
        try
        {
            using (reader = new StreamReader(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) continue;

                    string[] columns = line.Split(";");
                    if (columns.Length != 4)
                    {
                        _logger.Error("Invalid Dcmf file format: " + line);
                        throw new InvalidMapFormatException();
                    }

                    Point2D pos = new Point2D(int.Parse(columns[0]), int.Parse(columns[1]));
                    DcmType type = ParseDcmfType(columns[2]);
                    DcmEntity entity = ParseDcmEntity(columns[3]);

                    if (type == DcmType.Player) hasPlayer = true;
                    if (entity == DcmEntity.LevelExit) hasExit = true;

                    AddObject(pos, type, entity);
                }

                if (!ignoreMissing && !hasPlayer)
                {
                    _logger.Warn("No player found.");
                    throw new PlayerNotFoundException();
                }

                if (!ignoreMissing && !hasExit)
                {
                    _logger.Warn("No player found.");
                    throw new LevelExitNotFoundException();
                }

                _logger.Info("Dcmf file loaded");
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            throw new Exception("Dcmf file could not be loaded: \n" + e.Message, e);
        }
    }

    public static bool HasCorrectExtension(string path)
    {
        return FileUtil.FileHasExtension(path, ".dcmf") || 
               FileUtil.FileHasExtension(path, ".map");
    }

    private GraphicsComponent GetEntityComponentByType(Point2D pos, DcmType type, DcmEntity entity)
    {
        switch (type)
        {
            case DcmType.GameItem:
                return new GameItem(pos.X, pos.Y, Enum.Parse<ItemType>(entity.ToString()));
            
            case DcmType.Player:
                return new Player(pos.X, pos.Y);
            
            case DcmType.Demon:
                switch (entity)
                {
                    case DcmEntity.Imp:
                        return new Imp(pos.X, pos.Y);
                    case DcmEntity.Zombieman:
                        return new Zombieman(pos.X, pos.Y);
                    case DcmEntity.Mancubus:
                        return new Mancubus(pos.X, pos.Y);
                    default: return null;
                }
        }
        _logger.Warn("Unknown entity type: " + type);

        return null;
    }

    public void AddObject(Point2D position, DcmType type, DcmEntity entity)
    {
        DcmList.Add(new DcmFormat
        {
            Position = position,
            Type = type,
            Entity = entity,
            Value = GetEntityComponentByType(position, type, entity)
        });
    }

    public GraphicsComponent? RemoveObject(Point2D position)
    {
        GraphicsComponent? removed = null;
        foreach (DcmFormat format in DcmList.ToList())
        {
            if (format.Position == position)
            {
                removed = format.Value;
                DcmList.Remove(format);
            }
        }
        return removed;
    }

    public bool IsPositionAcquired(Point2D position)
    {
        foreach (DcmFormat format in DcmList.ToList())
        {
            if (format.Position == position)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveObject(DcmFormat dcmFormat)
    {
        DcmList.Remove(dcmFormat);
    }

    public void RemoveLastObject()
    {
        DcmList.Remove(DcmList[DcmList.Count - 1]);
    }

    public void ClearObjects()
    {
        DcmList.Clear();
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

    private DcmType ParseDcmTypeFromEntity(DcmEntity entity)
    {
        int val = (int) entity;
        if (val < 7) return DcmType.GameItem;
        if (val < 10) return DcmType.Demon;
        if (val == 10) return DcmType.Player;
        
        _logger.Warn("Unknown entity type: " + entity);
        return DcmType.Unknown;
    }

    private DcmEntity ParseDcmEntity(string entity)
    {
        switch (entity)
        {
            case "A": return DcmEntity.Ammo;
            case "B": return DcmEntity.BfgCell;
            case "D": return DcmEntity.Door;
            case "E": return DcmEntity.LevelExit;
            case "W": return DcmEntity.Wall;
            case "T": return DcmEntity.ToxicWaste;
            case "M": return DcmEntity.MedKit;
            case "z": return DcmEntity.Zombieman;
            case "m": return DcmEntity.Mancubus;
            case "i": return DcmEntity.Imp;
            case "p": return DcmEntity.Player;
        }

        _logger.Warn("Unknown entity type: " + entity);
        return DcmEntity.Unknown;
    }
    
    private string GetEntity(DcmEntity entity) => entity switch
    {
        DcmEntity.Ammo => "A",
        DcmEntity.BfgCell => "B",
        DcmEntity.Door => "D",
        DcmEntity.LevelExit => "E",
        DcmEntity.Wall => "W",
        DcmEntity.ToxicWaste => "T",
        DcmEntity.MedKit => "M",
        DcmEntity.Zombieman => "z",
        DcmEntity.Mancubus => "m",
        DcmEntity.Imp => "i",
        DcmEntity.Player => "p",
        _ => "?"
    };
    
    private string GetType(DcmType type) => type switch
    {
        DcmType.GameItem => "GI",
        DcmType.Player => "P",
        DcmType.Demon => "D",
        _ => "?"
    };

    public List<GameItem>? CollectItems()
    {
        if (DcmList.Count == 0)
        {
            _logger.Error("No entity in dcm list");
            return null;
        }
        
        List<GameItem>? result = new List<GameItem>();
        
        foreach (var item in DcmList)
        {
            if (item.Type == DcmType.GameItem)
            {
                result.Add((GameItem) item.Value);
            }
        }
        return result;
    }
    
    public List<Demon> CollectDemons()
    {
        if (CheckCount()) return null;
        
        List<Demon> result = new List<Demon>();
        
        foreach (var item in DcmList)
        {
            if (item.Type == DcmType.Demon)
            {
                result.Add((Demon) item.Value);
            }
        }
        return result;
    }

    private bool CheckCount()
    {
        if (DcmList.Count == 0)
        {
            _logger.Error("No entity was found in dcm list");
            return true;
        }

        return false;
    }

    public Player? GetPlayer()
    {
        if (DcmList.Count == 0)
        {
            _logger.Error("No entity in dcm list");
            return null;
        }

        foreach (var item in DcmList)
        {
            if (item.Type == DcmType.Player)
            {
                return (Player) item.Value;
            }
        }

        return null;
    }

    // Deduplication
    public int Optimize()
    {
        HashSet<Point2D> seen = new HashSet<Point2D>();
        List<DcmFormat> deduplicated = new List<DcmFormat>();

        foreach (var item in DcmList)
        {
            if (seen.Add(item.Position))
            {
                deduplicated.Add(item);
            }
        }
        int duplicateCount = DcmList.Count - deduplicated.Count;
        DcmList.Clear();
        DcmList.AddRange(deduplicated);
        
        return duplicateCount;
    }
    
    public void SaveMap(string path)
    {
        try
        {
            if (!HasCorrectExtension(path)) 
                throw new Exception("The extension should be '*.dcmf'! or '*.map'");
            
            using StreamWriter sw = new StreamWriter(path);
            sw.WriteLine("#PX;PY;Typ;Val");
            foreach (var item in DcmList)
            {
                sw.WriteLine($"{item.Position.X};" +
                             $"{item.Position.Y};" +
                             $"{GetType(item.Type)};" +
                             $"{GetEntity(item.Entity)}");
            }
        }
        catch (Exception e)
        {
            throw new Exception("Error saving map:.\n" + e.Message);
        }
    }
    
    public void LoadFromLegacy(string path)
    {
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
                    if (line[j] != '_')
                    {
                        int posX = j;
                        int posY = i - 1;

                        DcmEntity item = ParseDcmEntity(line[j].ToString());
                        DcmType itemType = ParseDcmTypeFromEntity(item);
                        AddObject(new Point2D(posX, posY), itemType, item);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.Error(e.Message);
            throw new Exception("Failed to load the map", e);
        }
    }
    
    [Obsolete("This method is deprecated, please use LoadFromLegacy (inner function) instead")]
    public static void LoadFromLegacyMap(string path, 
        List<GameItem> items, 
        List<Demon> demons, 
        Player player)
    {
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
                            items.Add(new GameItem(j, i - 1, ItemType.Ammo));
                            break;
                        case 'B':
                            items.Add(new GameItem(j, i - 1, ItemType.BfgCell));
                            break;
                        case 'D':
                            items.Add(new GameItem(j, i - 1, ItemType.Door));
                            break;
                        case 'E':
                            items.Add(new GameItem(j, i - 1, ItemType.LevelExit));
                            break;
                        case 'M':
                            items.Add(new GameItem(j, i - 1, ItemType.MedKit));
                            break;
                        case 'T':
                            items.Add(new GameItem(j, i - 1, ItemType.ToxicWaste));
                            break;
                        case 'W':
                            items.Add(new GameItem(j, i - 1, ItemType.Wall));
                            break;

                        // DEMONS
                        case 'z':
                            demons.Add(new Zombieman(j, i - 1));
                            break;
                        case 'i':
                            demons.Add(new Imp(j, i - 1));
                            break;
                        case 'm':
                            demons.Add(new Mancubus(j, i - 1));
                            break;

                        // PLAYER
                        case 'p':
                            player.WorldPosition = new Point2D(j, i - 1);
                            break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
}