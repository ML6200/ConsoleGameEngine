using System;

namespace SimpleDoomDemo.Gameplay.Scenes.Exceptions;

public class PlayerNotFoundException() : Exception("Player is not found in map file. " +
                                                   "You can add it via map editor");