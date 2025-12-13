using System;

namespace SimpleDoomDemo.Gameplay.Scenes.Exceptions;

public class LevelExitNotFoundException(string message) : Exception(message);