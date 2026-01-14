⚠️ This project is unfinished and currently under development.

# Console Game Engine

A modern 2D game engine for rendering games directly in the terminal using ANSI escape codes.

## Features

- **Console Rendering**: ANSI-based color rendering for cross-platform terminal support
- **Multi-threaded Architecture**: Separate timers for both update and render threads for smooth gameplay, and better control
- **Component System**: Hierarchical component-based architecture with parent-child relationships
- **Scene Management**: Clean lifecycle management with `Initialize`, `OnEnter`, `OnUpdate`, and `OnExit` callbacks
- **Optimized Rendering**: Utilized stdout with byte buffer for faster access and culling for optimized rendering
- **Input Handling**: Event-based input system for responsive controls
- **Audio Support**: Integrated SoundFlow library for cross-platform audio
- **Camera System**: Viewport and camera support (in development)

## Requirements

- .NET 9.0 SDK
- Terminal with ANSI color support (most modern terminals)

## Dependencies
- SoundFlow (NuGet)
- NLog package (NuGet)

## Getting Started

### Building the Project

```bash
dotnet build .
```

### Running the Demo

```bash
# Run the Doom-like demo
dotnet run --project SimpleDoomDemo
```

## Architecture Overview

### Threading Model

The engine uses a two-threaded architecture for optimal performance:

- **Update Thread** (60 UPS): Handles game logic, scene updates, and input processing
- **Render Thread** (60 FPS): Manages graphics buffer and console output

Thread-safe by design with proper synchronization between update and render cycles.

### Component Hierarchy

Components follow a parent-child relationship model:

- **ConsoleGraphicsComponent**: Base class for all renderable objects
- **Position System**:
  - `RelativePosition`: Local coordinates relative to parent
  - `WorldPosition`: Calculated absolute position in world space
- **Rendering**: Components recursively compute and render themselves and their children

## Project Structure

```
ConsoleGameEngine-Separate/
├── ConsoleGameEngine/          # Core engine library
│   └── src/
│       └── Engine/
│           ├── ConsoleEngine.cs           # Main engine orchestrator
│           └── Renderer/                  # Rendering subsystem
│               ├── ConsoleRenderer2D.cs   # 2D rendering with ANSI
│               ├── ConsoleCamera.cs       # Camera and viewport
│               ├── Geometry/              # Point2D, Dimension2D, etc.
│               └── Graphics/              # Components, panels, UI
├── ConsoleGameEngine.Demo/     # Basic demo project
├── ConsoleGameEngine.Tests/    # Unit tests
└── SimpleDoomDemo/             # Doom-like game demo
```

### Scene Management

Implement the `IGameScene` interface to create game scenes:

```csharp
public interface IGameScene
{
    void Initialize();
    void OnEnter();
    void OnUpdate(float deltaTime);
    void OnExit();
}
```

## Examples

The project includes one main demonstration:

1. **SimpleDoomDemo**: A Doom-like game showcasing:
   - Multiple game systems (Movement, Combat, AI, Interaction)
   - Visibility/fog-of-war mechanics (hiding unnecessary entities)
   - Advanced gameplay features (still in developmnet)

## Current Status

### Working Features
- Component hierarchy system
- Multi-threaded rendering
- ANSI color support
- Input handling
- Scene management
- 
### In Development
- Camera viewport system (works if implemented for an actor, better implementation in progress)
- More advanced audio system
- Better settings management
- Entity-Component-System (ECS) architecture support

## Development

### Running the Doom demo
```bash
dotnet run --project SimpleDoomDemo/SimpleDoomDemo.csproj
```

### Project Configuration

- **Target Framework**: .NET 9.0
- **Unsafe Blocks**: Enabled for future optimizations
- **Implicit Usings**: Disabled for explicit namespace control
- **Nullable**: Enabled for better null-safety


## Background

This project was originally inspired by a university assignment and
was later expanded into a standalone project for learning and experimentation.

Claude Sonnet 4.5 was partially used for brainstorming and acceleating develoment.

## License

### Code License (MIT)

The source code in this project is licensed under the MIT License.

**EXCEPTION:** The following are NOT covered by the MIT License:
- `assets/` directory and all its contents

### Assets License

All assets (ex. music, sounds) in the `assets/` directory are proprietary
and NOT licensed for redistribution.

See `SimpleDoomDemo/assets/ASSETS_LICENSE.txt` for full details.
```
