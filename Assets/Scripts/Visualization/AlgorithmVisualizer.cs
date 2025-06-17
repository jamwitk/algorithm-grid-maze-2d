using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using PathfindingCore;

namespace Visualization
{
    public class AlgorithmVisualizer : MonoBehaviour, IAlgorithmVisualizer
    {
        [Header("Visualization Tiles")]
        [SerializeField] private Tile pathTile;
        [SerializeField] private Tile neighborTile;
        [SerializeField] private Tile currentNodeTile;
        [SerializeField] private Tile exploredTile;
        [SerializeField] private Tile defaultTile;

        [Header("Special Tiles")]
        [SerializeField] private Tile startTile;
        [SerializeField] private Tile endTile;
        [SerializeField] private Tile obstacleTile;

        [Header("Visualization Settings")]
        [SerializeField] private VisualizationSettings settings = new VisualizationSettings();

        private Tilemap tilemap;
        private readonly HashSet<Vector3Int> visualizedPositions = new HashSet<Vector3Int>();

        private void Awake()
        {
            tilemap = GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogError("AlgorithmVisualizer requires a Tilemap component!");
            }
        }

        public IEnumerator VisualizeAlgorithm(IEnumerator algorithmSteps)
        {
            ClearVisualization();

            while (algorithmSteps.MoveNext())
            {
                var step = algorithmSteps.Current as IVisualizationStep;
                if (step == null) continue;

                VisualizeStep(step);
                
                // Update UI with step info
                if (UIManager.instance != null)
                {
                    UIManager.instance.SetResultText(step.GetStepInfo());
                }

                yield return new WaitForSeconds(settings.StepDelay);
            }
        }

        public void ClearVisualization()
        {
            if (tilemap == null) return;

            foreach (var position in visualizedPositions)
            {
                if (!IsSpecialTile(position))
                {
                    tilemap.SetTile(position, defaultTile);
                    tilemap.SetColor(position, Color.white);
                }
            }
            visualizedPositions.Clear();
        }

        public void SetVisualizationSpeed(float speed)
        {
            settings.StepDelay = speed;
        }

        private void VisualizeStep(IVisualizationStep step)
        {
            // Clear previous non-special visualizations
            ClearNonSpecialTiles();

            // Visualize current path
            if (settings.ShowCurrentPath)
            {
                VisualizePath(step.GetCurrentPath(), pathTile);
            }

            // Visualize neighbors
            if (settings.ShowNeighbors)
            {
                VisualizePositions(step.GetNeighbors(), neighborTile);
            }

            // Visualize current node
            var currentNode = step.GetCurrentNode();
            if (currentNode != Vector3Int.zero && !IsSpecialTile(currentNode))
            {
                VisualizePosition(currentNode, currentNodeTile);
            }
        }

        private void VisualizePath(List<Vector3Int> path, Tile tileToUse)
        {
            if (path == null) return;

            foreach (var position in path)
            {
                VisualizePosition(position, tileToUse);
            }
        }

        private void VisualizePositions(List<Vector3Int> positions, Tile tileToUse)
        {
            if (positions == null) return;

            foreach (var position in positions)
            {
                VisualizePosition(position, tileToUse);
            }
        }

        private void VisualizePosition(Vector3Int position, Tile tileToUse)
        {
            if (IsSpecialTile(position)) return;

            tilemap.SetTile(position, tileToUse);
            visualizedPositions.Add(position);
        }

        private bool IsSpecialTile(Vector3Int position)
        {
            var currentTile = tilemap.GetTile(position);
            return currentTile == startTile || 
                   currentTile == endTile || 
                   currentTile == obstacleTile;
        }

        private void ClearNonSpecialTiles()
        {
            var positionsToRemove = new List<Vector3Int>();

            foreach (var position in visualizedPositions)
            {
                if (!IsSpecialTile(position))
                {
                    tilemap.SetTile(position, defaultTile);
                    tilemap.SetColor(position, Color.white);
                    positionsToRemove.Add(position);
                }
            }

            foreach (var position in positionsToRemove)
            {
                visualizedPositions.Remove(position);
            }
        }

        public void DrawFinalPath(List<Vector3Int> finalPath)
        {
            if (finalPath == null || finalPath.Count == 0) return;

            StartCoroutine(DrawFinalPathCoroutine(finalPath));
        }

        private IEnumerator DrawFinalPathCoroutine(List<Vector3Int> finalPath)
        {
            foreach (var position in finalPath)
            {
                if (!IsSpecialTile(position))
                {
                    tilemap.SetTile(position, pathTile);
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
    }
} 