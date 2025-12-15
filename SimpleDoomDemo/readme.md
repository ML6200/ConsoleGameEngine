# SimpleDoomDemo

A DOOM-inspired console-based game built with the ConsoleGameEngine framework. This demo showcases a first-person shooter experience rendered entirely in the terminal with ASCII graphics, complete with enemies, items, combat, and sound effects.

## Overview

SimpleDoomDemo is a demonstration project that implements classic DOOM-style gameplay mechanics in a console environment. Players navigate through maps, battle demons, collect items, and progress through levels - all rendered using console characters and colors.

## Features

### Gameplay
- Player movement with collision detection
- Combat system with multiple weapon types (Shotgun and BFG)
- Ai enemies with multiple demon types (Imps, Zombiemen, Mancubi)
- Item collection system (health, ammo, BFG cells)
- Fog of war / visibility system
- Level progression with exit points
- Character progression (combat points affect max health and ammo)

### Technical
- 40 FPS game logic updates, 100 FPS rendering
- Camera system that follows the player
- Sound engine with background music and sound effects
- Scene management (Main Menu, Game, Settings, Map Editor, Game Over)
- Custom map format (.dcmf) with built-in map editor
- Persistent settings storage (JSON)
- NLog integration for debugging

## Requirements

- .NET 9.0 SDK
- Windows, macOS, or Linux with terminal support
- Audio playback capability for sound effects and music

## Installation

1. Clone the repository and navigate to the SimpleDoomDemo directory
2. Ensure the ConsoleGameEngine project is available (referenced as a project dependency)
3. Build the project:
   ```bash
   dotnet build
   ```

4. Run the game:
   ```bash
   dotnet run
   ```

## Controls

### Gameplay
- **Arrow Keys** - Move player (Up/Down/Left/Right)
- **A** - Attack with shotgun
- **S** - Attack with BFG9000 (requires BFG cells)
- **D** - Interact with objects (open doors, activate exits, pick up items)
- **Escape** - Pause/Exit menu

### Menu Navigation
- Use arrow keys to navigate menus
- Use Enter for pressing buttons

## Game Elements

### Player
- **Health**: Starts at 100, increases with combat points (max = combatPoints/10 + 100)
- **Ammo**: For shotgun attacks (max = combatPoints/50 + 10)
- **BFG Cells**: Special weapon ammunition
- **Combat Points**: Earned by defeating enemies, increases player capabilities
- **Sight Range**: Determines visibility radius (fog of war effect)

### Enemies
- **Imp**: Basic melee demon
- **Zombieman**: Standard enemy type
- **Mancubus**: Stronger demon variant

### Items
- **Health Kits**: Restore player health
- **Ammo Packs**: Restore shotgun ammunition
- **BFG Cells**: Ammunition for the BFG weapon
- **Combat Points**: Permanent upgrades

### Map Elements
- **Walls**: Impassable barriers (represented by '#')
- **Doors**: Interactive objects that can be opened (represented by 'D')
- **Exit Points**: Level completion triggers (represented by 'E')

## Configuration

Settings are stored in `settings.json` (auto-generated on first run):

```json
{
  "DefaultMap": "map.dcmf",
  "AudioAssetsPath": "assets/audio"
}
```

The default assets folder contains audio effects and music

You can modify these settings through the in-game Settings menu or by editing the JSON file directly.

## Creating Custom Maps

SimpleDoomDemo includes a built-in map editor accessible from the main menu. Maps are saved in the `.dcmf` (Doom CSV Map Format) format.

### Map Format
Maps are text-based files that use characters to represent different elements:
- `W` - Wall
- `P` - Player start position
- `E` - Exit point
- `D` - Door
- `I` - Imp spawn
- `Z` - Zombieman spawn
- `M` - Mancubus spawn
- `M` - Medkit
- `A` - Ammo pack
- `B` - BFG cell
##### Note that each map requires both a Player and a Level Exit!


### Using the Map Editor
1. Launch the game and select "Map Editor" from the main menu
2. Use the editor tools to design your level
3. Save the map with a `.dcmf` or `.map` extension
4. Load your custom map from the "Load Map" menu option

You can also load 'handwritten' maps like this by pressing `Shift+L` in the map editor:

```
8,14
WWWWWWWWWWWWWW
W_S_W___TTT__W
W___i____B___W
W_p_WWWW_____W
WWWWW__WT__A_W
W___W__WWWW__W
W_M_W____zz__W
WWWWWWWWWWWWWW
```

and convert them to `.dcmf` by saving the loaded file

## Dependencies

- **ConsoleGameEngine**: Custom game engine framework (project reference)
- **NLog**: Logging framework (v6.0.7)
- **.NET 9.0**: Target framework

## Sound Credits

Audio files should be placed in the `assets/audio` directory. The game expects the following files:
- `mark_lor-war_of_sirens.mp3` - Background music
- `gs_shotgun.mp3` - Shotgun fire
- `gs_bfg.mp3` - BFG fire
- `gs_pain.mp3` - Player damage
- `gs_death.mp3` - Player death
- `gs_door.mp3` - Door opening
- `gs_pickup.mp3` - Item pickup

## Troubleshooting

### Audio Not Playing
- Ensure audio files are present in the `assets/audio` directory
- Check that the `AudioAssetsPath` in `settings.json` is correct
- Verify your system has audio playback capability

### Console Display Issues
- Ensure your terminal supports UTF-8 encoding
- Try resizing your terminal window if graphics appear corrupted
- On Windows, use Windows Terminal for best results

### Performance Issues
- The game targets 40 UPS (updates per second) and 100 FPS (frames per second)
- Reduce terminal window size if experiencing lag
- Check NLog configuration if extensive logging is impacting performance
