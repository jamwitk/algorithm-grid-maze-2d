using UnityEngine;


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
                    gridManager.FindPath();
                    break;
                case Algorithm.Dijkstra:
                    // TODO: Implement Dijkstra's algorithm
                    Debug.Log("Dijkstra's algorithm not implemented yet");
                    break;
                case Algorithm.GeneticAlgorithm:
                    // TODO: Implement Genetic Algorithm
                    Debug.Log("Genetic Algorithm not implemented yet");
                    break;
                case Algorithm.BruteForce:
                    // TODO: Implement Brute Force
                    Debug.Log("Brute Force algorithm not implemented yet");
                    break;
                default:
                    Debug.LogWarning("Unknown algorithm selected!");
                    break;
            }
        }
    }

