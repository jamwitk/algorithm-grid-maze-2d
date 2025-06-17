using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathfindingCore;
using Algorithms;
using Managers;
using Visualization;

namespace Controllers
{
    public class PathfindingController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private AlgorithmVisualizer visualizer;

        [Header("Algorithm Selection")]
        [SerializeField] private AlgorithmType selectedAlgorithm = AlgorithmType.AStar;

        private readonly Dictionary<AlgorithmType, IPathfindingAlgorithm> algorithms = 
            new Dictionary<AlgorithmType, IPathfindingAlgorithm>();

        private bool isRunning = false;
        private Coroutine currentPathfindingCoroutine;

        public static event Action<PathfindingResult> OnPathfindingComplete;
        public static event Action<AlgorithmType> OnAlgorithmChanged;

        private void Awake()
        {
            InitializeAlgorithms();
            ValidateComponents();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeAlgorithms()
        {
            algorithms[AlgorithmType.AStar] = new AStarAlgorithm();
            // Add other algorithms here when they're refactored
            // algorithms[AlgorithmType.Dijkstra] = new DijkstraAlgorithm();
            // algorithms[AlgorithmType.Genetic] = new GeneticAlgorithm();
        }

        private void ValidateComponents()
        {
            if (levelManager == null)
            {
                levelManager = FindObjectOfType<LevelManager>();
                if (levelManager == null)
                {
                    Debug.LogError("PathfindingController requires a LevelManager!");
                }
            }

            if (visualizer == null)
            {
                visualizer = FindObjectOfType<AlgorithmVisualizer>();
                if (visualizer == null)
                {
                    Debug.LogError("PathfindingController requires an AlgorithmVisualizer!");
                }
            }
        }

        private void SubscribeToEvents()
        {
            LevelManager.OnStartEndPositionsChanged += OnStartEndPositionsChanged;
        }

        private void UnsubscribeFromEvents()
        {
            LevelManager.OnStartEndPositionsChanged -= OnStartEndPositionsChanged;
        }

        private void OnStartEndPositionsChanged(Vector3Int start, Vector3Int end)
        {
            if (isRunning)
            {
                StopPathfinding();
            }
        }

        public void SetAlgorithm(int algorithmIndex)
        {
            if (Enum.IsDefined(typeof(AlgorithmType), algorithmIndex))
            {
                var newAlgorithm = (AlgorithmType)algorithmIndex;
                if (selectedAlgorithm != newAlgorithm)
                {
                    selectedAlgorithm = newAlgorithm;
                    OnAlgorithmChanged?.Invoke(selectedAlgorithm);
                    Debug.Log($"Algorithm changed to: {selectedAlgorithm}");
                }
            }
            else
            {
                Debug.LogError($"Invalid algorithm index: {algorithmIndex}");
            }
        }

        public void StartPathfinding()
        {
            if (isRunning)
            {
                Debug.LogWarning("Pathfinding is already running!");
                return;
            }

            if (!algorithms.ContainsKey(selectedAlgorithm))
            {
                Debug.LogError($"Algorithm {selectedAlgorithm} is not implemented yet!");
                return;
            }

            var grid = levelManager.CreateGrid();
            if (grid == null)
            {
                Debug.LogError("Failed to create grid for pathfinding!");
                return;
            }

            var start = levelManager.GetStartPosition();
            var end = levelManager.GetEndPosition();

            if (start == Vector3Int.zero || end == Vector3Int.zero)
            {
                Debug.LogError("Start and end positions must be set before pathfinding!");
                return;
            }

            Debug.Log($"Starting {selectedAlgorithm} pathfinding from {start} to {end}");
            currentPathfindingCoroutine = StartCoroutine(RunPathfinding(start, end, grid));
        }

        public void StopPathfinding()
        {
            if (currentPathfindingCoroutine != null)
            {
                StopCoroutine(currentPathfindingCoroutine);
                currentPathfindingCoroutine = null;
            }

            isRunning = false;
            visualizer.ClearVisualization();
        }

        public void ResetLevel()
        {
            StopPathfinding();
            levelManager.ResetLevel();
        }

        public void NextLevel()
        {
            StopPathfinding();
            levelManager.NextLevel();
        }

        public void SetVisualizationSpeed(float speed)
        {
            visualizer.SetVisualizationSpeed(speed);
        }

        private IEnumerator RunPathfinding(Vector3Int start, Vector3Int end, IGrid grid)
        {
            isRunning = true;
            
            try
            {
                var algorithm = algorithms[selectedAlgorithm];
                var pathfindingSteps = algorithm.FindPath(start, end, grid);
                
                yield return visualizer.VisualizeAlgorithm(pathfindingSteps);
                
                Debug.Log($"{selectedAlgorithm} pathfinding completed!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during pathfinding: {e.Message}");
            }
            finally
            {
                isRunning = false;
                currentPathfindingCoroutine = null;
            }
        }

        // Public getters for UI and other systems
        public AlgorithmType GetSelectedAlgorithm() => selectedAlgorithm;
        public bool IsPathfindingRunning() => isRunning;
        public IReadOnlyDictionary<AlgorithmType, IPathfindingAlgorithm> GetAvailableAlgorithms() => algorithms;
    }
} 