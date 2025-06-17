using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathfindingCore
{
    public interface IPathfindingAlgorithm
    {
        IEnumerator FindPath(Vector3Int start, Vector3Int end, IGrid grid);
        string GetAlgorithmName();
    }

    public interface IGrid
    {
        bool IsWalkable(Vector3Int position);
        bool IsInBounds(Vector3Int position);
        Vector3Int GetStartPosition();
        Vector3Int GetEndPosition();
        IEnumerable<Vector3Int> GetNeighbors(Vector3Int position);
    }

    public interface IVisualizationStep
    {
        List<Vector3Int> GetCurrentPath();
        List<Vector3Int> GetNeighbors();
        Vector3Int GetCurrentNode();
        string GetStepInfo();
    }

    public interface ILevelManager
    {
        void LoadLevel(int levelIndex);
        void NextLevel();
        void ResetLevel();
        int GetCurrentLevel();
        bool IsLevelComplete();
    }

    public interface IAlgorithmVisualizer
    {
        IEnumerator VisualizeAlgorithm(IEnumerator algorithmSteps);
        void ClearVisualization();
        void SetVisualizationSpeed(float speed);
    }
} 