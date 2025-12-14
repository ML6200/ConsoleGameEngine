using System;

namespace SimpleDoomDemo.Gameplay.Scenes.Exceptions;

public class LevelExitNotFoundException() : Exception("Level exit is not found in map file. " +
                                                      "You can add it via map editor");