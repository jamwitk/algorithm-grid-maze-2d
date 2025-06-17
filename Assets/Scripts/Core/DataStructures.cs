using System.Collections.Generic;
using UnityEngine;

namespace PathfindingCore
{
    public enum AlgorithmType
    {
        Genetic = 0,
        AStar = 1,
        Dijkstra = 2,
        BruteForce = 3
    }

    public enum TileType
    {
        Empty = 0,
        Obstacle = 1,
        Start = 2,
        End = 3
    }

    public enum VisualizationType
    {
        Path,
        Neighbor,
        Current,
        Explored
    }

    [System.Serializable]
    public class LevelData
    {
        public List<GridRow> grid;
        public string levelName;
        public int levelIndex;
    }

    [System.Serializable]
    public class GridRow
    {
        public List<int> row;
    }

    public class PathfindingResult
    {
        public List<Vector3Int> Path { get; private set; }
        public bool IsPathFound { get; private set; }
        public long ElapsedMilliseconds { get; private set; }
        public int NodesExplored { get; private set; }
        public int Iterations { get; private set; }
        public string AlgorithmName { get; private set; }

        public PathfindingResult(List<Vector3Int> path, bool isPathFound, long elapsedMs, 
                               int nodesExplored, int iterations, string algorithmName)
        {
            Path = path ?? new List<Vector3Int>();
            IsPathFound = isPathFound;
            ElapsedMilliseconds = elapsedMs;
            NodesExplored = nodesExplored;
            Iterations = iterations;
            AlgorithmName = algorithmName;
        }
    }

    public class VisualizationSettings
    {
        public float StepDelay { get; set; } = 0.1f;
        public bool ShowNeighbors { get; set; } = true;
        public bool ShowCurrentPath { get; set; } = true;
        public bool ShowExploredNodes { get; set; } = true;
    }

    public abstract class BaseVisualizationStep : IVisualizationStep
    {
        protected List<Vector3Int> currentPath;
        protected List<Vector3Int> neighbors;
        protected Vector3Int currentNode;

        public BaseVisualizationStep(List<Vector3Int> path, List<Vector3Int> neighbors, Vector3Int current)
        {
            this.currentPath = path ?? new List<Vector3Int>();
            this.neighbors = neighbors ?? new List<Vector3Int>();
            this.currentNode = current;
        }

        public virtual List<Vector3Int> GetCurrentPath() => currentPath;
        public virtual List<Vector3Int> GetNeighbors() => neighbors;
        public virtual Vector3Int GetCurrentNode() => currentNode;
        public abstract string GetStepInfo();
    }
} 