using System;
using System.Text;
using System.Threading;
using ConsoleGameEngine.Engine;
using SimpleDoomDemo.Gameplay;
using SimpleDoomDemo.Gameplay.Scenes;
using NLog;

namespace SimpleDoomDemo;

class Program
{
    static void ConvertMap(string path)
    {
        Mapper mapper  = new Mapper();
        mapper.LoadFromLegacy(path);
        mapper.SaveMap("arena.dcmf");
    }
    static void Main(string[] args)
    {
        string DEFAULT_MAP_PATH = "arenad.dcmf";//Path.Combine("assets", "maps", "pmp_arena.txt");
        LogManager.Setup().LoadConfigurationFromFile("nlog.xml");
        DoomGameManager manager = new DoomGameManager(DEFAULT_MAP_PATH);
        manager.StartGame();
    }
}