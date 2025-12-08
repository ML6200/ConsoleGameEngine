using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleDoomDemo.Gameplay;

namespace ConsoleGameEngine.Tests.Gameplay;

[TestClass]
[TestSubject(typeof(Mapper))]
public class MapperTest
{

    [TestMethod]
    public void TestLoadAndSaveMap()
    {
        Mapper mapper = new();
        mapper.LoadFromLegacy("pmp_arena.txt");
        mapper.SaveMap("pmp_arenax.dcmf");
        Assert.IsTrue(File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "pmp_arenax.dcmf")));
    }
    
    [TestMethod]
    public void TestLoadMap()
    {
        Mapper mapper = new("pmp_arenax.dcmf");
        foreach (var v in mapper.CollectDemons())
        {
            Console.WriteLine(v.State);
        }
    }
}