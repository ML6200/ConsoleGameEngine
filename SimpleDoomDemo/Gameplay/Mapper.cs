using System;
using System.Collections.Generic;
using System.IO;
using ConsoleGameEngine.Engine.Renderer.Geometry;
using ConsoleGameEngine.Engine.Renderer.Graphics;
using ConsoleGameEngine.Engine.System;
using NLog;
using SimpleDoomDemo.Gameplay.Actors.Demons;
using SimpleDoomEngine.Gameplay.Actors;
using SimpleDoomEngine.Gameplay.Items;

namespace SimpleDoomDemo.Gameplay;

public class Mapper
{
    private Logger _logger = LogManager.GetCurrentClassLogger();

    public Mapper(string path)
    {
        LoadFromDcmfFile(path);
    }

    public Mapper()
    {
    }
    
    // Doom CSV Map format
    /*
     * PosX;PosY;Type;Val
     * 12;21;GI;A
     */
    public record struct DcmFormat
    {
        public Point2D Position;
        public DcmType Type;
        public DcmEntity Entity;
        public GraphicsComponent Value;
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

    public List<DcmFormat> DcmList = new();

    public void LoadFromDcmfFile(string path)
    {
        if (FileUtil.FileHasExtension(path, ".dcmf"))
        {
            using StreamReader reader = new StreamReader(path);

            string? line;
            try
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) continue;
                    
                    string[] columns = line.Split(";");
                    if (columns.Length != 4) throw new Exception("Invalid map file format");

                    Point2D pos = new Point2D(int.Parse(columns[0]), int.Parse(columns[1]));
                    DcmType type = ParseDcmfType(columns[2]);
                    DcmEntity entity =  ParseDcmEntity(columns[3]);
                    
                    AddObject(pos, type, entity);
                }
                _logger.Info("Dcmf file loaded");
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
            }
            finally
            {
                reader.Close();
            }
        }
        else
        {
            _logger.Warn("File does not have a propper extension." +
                         "The extension should be *.dcmf'! " + path);
        }
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

    public void SaveMap(string path)
    {
        try
        {
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
            throw new Exception("Error saving map", e);
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
    
    static Logger _staticLogger = LogManager.GetCurrentClassLogger();
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
            _staticLogger.Error(e.Message);
            throw;
        }
    }
}