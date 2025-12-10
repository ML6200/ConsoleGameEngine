# Console Game Engine

A modern 2D game engine for rendering games directly in the terminal using ANSI escape codes.

## Features

- **Console Rendering**: ANSI-based color rendering for cross-platform terminal support
- **Multi-threaded Architecture**: Separate update (60 UPS) and render (60 FPS) threads for smooth gameplay
- **Component System**: Hierarchical component-based architecture with parent-child relationships
- **Scene Management**: Clean lifecycle management with `Initialize`, `OnEnter`, `OnUpdate`, and `OnExit` callbacks
- **Optimized Rendering**: Dirty-cell tracking to minimize unnecessary screen updates
- **Input Handling**: Event-based input system for responsive controls
- **Audio Support**: Integrated audio via NAudio library
- **Camera System**: Viewport and camera support (in development)

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

## Requirements

- .NET 9.0 SDK
- Terminal with ANSI color support (most modern terminals)
- NAudio package (will be replaced with native api calls later)

## Getting Started

### Building the Project

```bash
dotnet build ConsoleGameEngine.sln
```

### Running the Demo

```bash
# Run the basic demo
dotnet run --project ConsoleGameEngine.Demo

# Run the Doom-like demo
dotnet run --project SimpleDoomDemo
```

### Using the Engine in Your Project

1. Add a reference to the ConsoleGameEngine project:
   ```bash
   dotnet add reference ../ConsoleGameEngine/ConsoleGameEngine.csproj
   ```

2. Create a basic game:
   ```csharp
   using ConsoleGameEngine;

   var engine = new ConsoleEngine();
   engine.Run();
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

### In Development
- Camera viewport system (placeholder exists, implementation in progress)
- World-to-screen coordinate transformation
- Viewport culling for large worlds
- Camera following and tracking
- More advanced audio system
- Custom map editor
- Settings management
- Entity-Component-System (ECS) architecture

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

Note: Claude Sonnet 4.5 was partially used for brainstorming and acceleating develoment.
