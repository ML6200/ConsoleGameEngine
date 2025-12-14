using System;
using SimpleDoomDemo.Gameplay;
using NLog;

namespace SimpleDoomDemo;

class Program
{
    static void Main(string[] args)
    {
        LogManager.Setup().LoadConfigurationFromFile("nlog.xml");
        DoomGameManager manager = new DoomGameManager();
        manager.StartGame();
        Console.Clear();
    }
}