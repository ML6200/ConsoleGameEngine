using System;

namespace SimpleDoomDemo.Gameplay.Scenes.Exceptions;

public class PlayerNotFoundException(string message) : Exception(message);