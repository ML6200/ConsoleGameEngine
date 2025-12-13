using System;

namespace SimpleDoomDemo.Gameplay.Scenes.Exceptions;

public class InvalidMapFormatException(string message) : Exception(message);