using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleDoomDemo.Gameplay;

namespace ConsoleGameEngine.Tests.Gameplay;

[TestClass]
[TestSubject(typeof(MapParser))]
public class MapParserTest
{

    [TestMethod]
    public void TestLoadAndSaveMap()
    {
        MapParser mapParser = new();
        mapParser.LoadFromLegacy("pmp_arena.txt");
        mapParser.SaveMap("pmp_arenax.dcmf");
        Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "pmp_arenax.dcmf")));
    }
    
    [TestMethod]
    public void TestLoadMap()
    {
        MapParser mapParser = new("pmp_arenax.dcmf");
        foreach (var v in mapParser.CollectDemons())
        {
            Console.WriteLine(v.State);
        }
    }
}