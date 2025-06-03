using UnityEngine;
using System.Collections.Generic;
using Algorithms;
using System.Runtime.CompilerServices;


public enum Algorithm
    {
        GeneticAlgorithm = 0,
        Astar = 1,
        Dijkstra = 2, 
        BruteForce = 3
    }

    public class GameController : MonoBehaviour
    {
        public static GameController instance;
        public GridManager gridManager;

        public void Awake()
        {
            instance = this;
        }
        public Algorithm selectedAlgorithm = Algorithm.GeneticAlgorithm;

        public void SetAlgorithm(int index)
        {
            selectedAlgorithm = (Algorithm)index;
            Debug.Log("Algorithm selected: " + selectedAlgorithm);
        }

        public void SolvePuzzle()
        {
            if (gridManager.startTilePosition == Vector3Int.zero || gridManager.endTilePosition == Vector3Int.zero)
            {
                Debug.LogWarning("Start and end positions must be set before solving!");
                return;
            }

            switch (selectedAlgorithm)
            {
                case Algorithm.Astar:
                    List<Vector3Int> astarPath = Astar.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    if (astarPath == null)
                    {
                        Debug.LogWarning("No path found between start and end positions!");
                        Debug.Log($"Start position: {gridManager.startTilePosition}, End position: {gridManager.endTilePosition}");
                        return;
                    }
                    Debug.Log($"Path found with {astarPath.Count} steps");
                    StartCoroutine(gridManager.DrawPath(astarPath));
                    break;
                case Algorithm.Dijkstra:
                    var DijkstraPath = Dijkstra.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    Debug.Log("Current selected algorithm: " + selectedAlgorithm + " Path found: " + DijkstraPath.Count + " Path: " + string.Join(",", DijkstraPath));
                    StartCoroutine(gridManager.DrawPath(DijkstraPath));
                    break;
                case Algorithm.GeneticAlgorithm:
                    var GeneticPath = Geneticv2.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    Debug.Log("Current selected algorithm: " + selectedAlgorithm + " Path found: " + GeneticPath.Count + " Path: " + string.Join(",", GeneticPath));
                    StartCoroutine(gridManager.DrawPath(GeneticPath));
                    break;
                case Algorithm.BruteForce:
                    //path = BruteForce.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    //gridManager.DrawPath(path);
                    break;
                default:
                    Debug.LogWarning("Unknown algorithm selected!");
                    break;
            }
        }
        public void Reset()
        {
            gridManager.Reset();
        }
        
    }

