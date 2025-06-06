using UnityEngine;
using System.Collections.Generic;
using Algorithms;
using System.Runtime.CompilerServices;
using System.Collections;


public enum Algorithm
    {
        Genetic = 0,
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
        public Algorithm selectedAlgorithm = Algorithm.Genetic;

        public void SetAlgorithm(int index)
        {
            selectedAlgorithm = (Algorithm)index;
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
                    var astarSteps = Astar.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    StartCoroutine(gridManager.VisualizeAlgorithmSteps(astarSteps));
                    break;
                case Algorithm.Dijkstra:
                    var dijkstraSteps = Dijkstra.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    StartCoroutine(gridManager.VisualizeAlgorithmSteps(dijkstraSteps));
                    break;
                case Algorithm.Genetic:
                    var geneticSteps = Geneticv2.FindPath(gridManager.startTilePosition, gridManager.endTilePosition);
                    StartCoroutine(gridManager.VisualizeAlgorithmSteps(geneticSteps));
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

