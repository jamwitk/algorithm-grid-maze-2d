# Pathfinding System Architecture

## Overview
This is a refactored pathfinding system for Unity that follows SOLID principles and provides a clean, modular architecture for implementing and visualizing various pathfinding algorithms.

## Architecture Components

### Core Interfaces (`Assets/Scripts/Core/Interfaces.cs`)
- **IPathfindingAlgorithm**: Interface for all pathfinding algorithms
- **IGrid**: Interface for grid operations and queries
- **IVisualizationStep**: Interface for algorithm visualization steps
- **ILevelManager**: Interface for level management operations
- **IAlgorithmVisualizer**: Interface for algorithm visualization

### Data Structures (`Assets/Scripts/Core/DataStructures.cs`)
- **AlgorithmType**: Enum for different algorithm types
- **TileType**: Enum for different tile types
- **LevelData**: Serializable class for level configuration
- **PathfindingResult**: Class containing pathfinding results and metrics
- **VisualizationSettings**: Configuration for visualization behavior

### Grid System (`Assets/Scripts/Core/Grid.cs`)
- Implements `IGrid` interface
- Handles walkability checks, bounds validation, and neighbor finding
- Provides distance calculation methods
- Decoupled from Unity-specific tilemap implementation

### Managers

#### LevelManager (`Assets/Scripts/Managers/LevelManager.cs`)
- Manages level loading, progression, and reset
- Handles tile placement based on level data
- Provides grid creation for pathfinding algorithms
- Fires events for level state changes

### Visualization

#### AlgorithmVisualizer (`Assets/Scripts/Visualization/AlgorithmVisualizer.cs`)
- Visualizes pathfinding algorithm steps in real-time
- Handles different tile types for visualization
- Configurable visualization settings
- Clears and manages visualization state

### Algorithms

#### AStarAlgorithm (`Assets/Scripts/Algorithms/AStarAlgorithm.cs`)
- Implements `IPathfindingAlgorithm` interface
- Provides step-by-step visualization data
- Includes performance metrics and logging
- Follows A* algorithm best practices

### Controllers

#### PathfindingController (`Assets/Scripts/Controllers/PathfindingController.cs`)
- Main controller that orchestrates the pathfinding process
- Manages algorithm selection and execution
- Coordinates between level manager and visualizer
- Provides public API for UI interactions

## Key Improvements

### 1. Separation of Concerns
- Grid logic separated from Unity tilemap
- Algorithm logic decoupled from visualization
- Level management isolated from pathfinding

### 2. Interface-Based Design
- All major components implement interfaces
- Easy to add new algorithms and visualizers
- Improved testability and maintainability

### 3. Event-Driven Architecture
- Components communicate through events
- Loose coupling between systems
- Easy to add new features without modifying existing code

### 4. Performance Optimization
- Efficient neighbor finding
- Proper memory management
- Configurable visualization speed

### 5. Error Handling
- Comprehensive validation
- Graceful error recovery
- Detailed logging and debugging

## Usage

### Setting Up a New Algorithm
1. Implement `IPathfindingAlgorithm` interface
2. Create a visualization step class inheriting from `BaseVisualizationStep`
3. Add algorithm to `PathfindingController.InitializeAlgorithms()`
4. Update `AlgorithmType` enum

### Adding New Visualization Features
1. Implement `IAlgorithmVisualizer` or extend `AlgorithmVisualizer`
2. Add new visualization types to `VisualizationType` enum
3. Configure visualization settings as needed

### Creating New Levels
1. Create `LevelData` objects with grid configuration
2. Assign to `LevelManager.levels` array
3. Use `TileType` enum values for grid cells

## Migration from Old System

### What Was Changed
- `GridManager` split into `LevelManager` and `AlgorithmVisualizer`
- `GameController` replaced with `PathfindingController`
- Algorithms now implement common interface
- Level data structure improved
- Visualization logic centralized

### Benefits
- More maintainable code
- Easier to add new features
- Better performance
- Improved debugging capabilities
- More testable architecture

## Example Usage

```csharp
// Get the pathfinding controller
var controller = FindObjectOfType<PathfindingController>();

// Set algorithm
controller.SetAlgorithm((int)AlgorithmType.AStar);

// Start pathfinding
controller.StartPathfinding();

// Change visualization speed
controller.SetVisualizationSpeed(0.2f);

// Reset level
controller.ResetLevel();
```

## Future Enhancements
- Add more pathfinding algorithms (Dijkstra, BFS, DFS)
- Implement different heuristics for A*
- Add pathfinding metrics dashboard
- Support for diagonal movement
- Save/load level configurations 