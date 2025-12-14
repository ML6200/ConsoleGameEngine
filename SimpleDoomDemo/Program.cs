using System;
using SimpleDoomDemo.Gameplay;
using NLog;

namespace SimpleDoomDemo;

class Program
{
    static void ConvertMap(string path)
    {
        MapParser mapParser  = new MapParser();
        mapParser.LoadFromLegacy(path);
        mapParser.SaveMap("arena.dcmf");
    }
    static void Main(string[] args)
    {
        string DEFAULT_MAP_PATH = "arena.dcmf";//Path.Combine("assets", "maps", "pmp_arena.txt");
        LogManager.Setup().LoadConfigurationFromFile("nlog.xml");
        DoomGameManager manager = new DoomGameManager(DEFAULT_MAP_PATH);
        manager.StartGame();
        Console.Clear();
    }
}