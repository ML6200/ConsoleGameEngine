using System;
using System.Diagnostics;
using ConsoleGameEngine.Engine.Input;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleGameEngine.Tests.Engine.Input;

[TestClass]
[TestSubject(typeof(KeyBinding))]
public class KeyBindingTest
{

    [TestMethod]
    public void TestParse()
    {
        KeyBinding keyBinding = KeyBinding.Parse("ctrl+c");
        KeyBinding keyBinding2 = KeyBinding.Parse("ctrl+c");
        
        Assert.AreEqual(keyBinding.Modifiers, keyBinding2.Modifiers);
        Assert.AreEqual(ConsoleKey.C, keyBinding.Key);
        Assert.AreEqual("control+c", keyBinding.ToString());
    }
}