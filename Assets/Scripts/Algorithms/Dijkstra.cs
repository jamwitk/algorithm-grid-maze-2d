using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
// using Analytics; // removed because we will reference fully qualified namespace
using System.Diagnostics;

namespace Algorithms
{
    public static class Dijkstra
    {
        public class StepData
        {
            public List<Vector3Int> currentPath;
            public List<Vector3Int> neighbors;
            public Vector3Int currentNode;
            public int iterations;

            public StepData(List<Vector3Int> path, List<Vector3Int> neighbors, Vector3Int current, int iterations)
            {
                this.currentPath = path;
                this.neighbors = neighbors;
                this.currentNode = current;
                this.iterations = iterations;
            }
        }
        private class Node
        {
            public Vector3Int Position { get; }
            public int Distance { get; set; }
            public Node Parent { get; set; }
            public Node(Vector3Int position)
            {
                Position = position;
                Distance = int.MaxValue;
            }
            public override bool Equals(object obj) => obj is Node other && Position.Equals(other.Position);
            public override int GetHashCode() => Position.GetHashCode();
        }

        // Direction vectors for the 4-way grid (no diagonals)
        private static readonly Vector3Int[] Directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),   // right
            new Vector3Int(-1, 0, 0),  // left
            new Vector3Int(0, 1, 0),   // up
            new Vector3Int(0, -1, 0)   // down
        };

        public static IEnumerator FindPath(Vector3Int startPos, Vector3Int endPos)
        {
            // BEGIN performance measurement
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Cache to ensure a unique Node instance per position
            Dictionary<Vector3Int, Node> nodeCache = new Dictionary<Vector3Int, Node>();
            int iterations = 0;
            Node GetNode(Vector3Int pos)
            {
                if (!nodeCache.TryGetValue(pos, out Node n))
                {
                    n = new Node(pos);
                    nodeCache[pos] = n;
                }
                return n;
            }

            Node startNode = GetNode(startPos);
            startNode.Distance = 0;

            Node endNode = GetNode(endPos);

            List<Node> openSet = new List<Node>();      // Nodes discovered but not finalised
            HashSet<Node> closedSet = new HashSet<Node>(); // Finalised nodes
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Select the open node with the smallest tentative distance
                Node current = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].Distance < current.Distance)
                        current = openSet[i];
                }
                iterations++;   
                // If destination reached – yield final path and terminate.
                if (current.Position.Equals(endNode.Position))
                {
                    var finalPath = RetracePath(startNode, current);
                    yield return new StepData(finalPath, new List<Vector3Int>(), current.Position, iterations);

                    // Stop timer and log analytics
                    stopwatch.Stop();
                    AlgorithmAnalytics.Log(
                        new AlgorithmAnalytics.ResultData(
                            "Dijkstra",
                            finalPath.Count,
                            stopwatch.ElapsedMilliseconds,
                            closedSet.Count,
                            iterations,
                            true));
                    yield break;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                // Gather valid neighbour positions first for visualisation
                List<Vector3Int> neighborPositions = new List<Vector3Int>();
                foreach (Vector3Int dir in Directions)
                {
                    Vector3Int neighborPos = current.Position + dir;
                    if (!IsValid(neighborPos)) continue;
                    neighborPositions.Add(neighborPos);

                    Node neighbor = GetNode(neighborPos);

                    if (closedSet.Contains(neighbor)) continue; // Already finalised

                    int tentativeDistance = current.Distance + 1; // Uniform edge cost
                    if (tentativeDistance < neighbor.Distance)
                    {
                        neighbor.Distance = tentativeDistance;
                        neighbor.Parent = current;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }

                // Yield current exploration step for visualisation (after relaxing edges so colours show up intuitively)
                yield return new StepData(RetracePath(startNode, current), neighborPositions, current.Position, iterations);
            }   

            // No path found – simply yield break (GridManager will finish visualisation loop)
            stopwatch.Stop();
            AlgorithmAnalytics.Log(
                new AlgorithmAnalytics.ResultData(
                    "Dijkstra",
                    0,
                    stopwatch.ElapsedMilliseconds,
                    closedSet.Count,
                    iterations,
                    false));
            yield break;
        }

        private static bool IsValid(Vector3Int position){
            return GridManager.instance.IsWalkable(position);
        }

        private static List<Vector3Int> RetracePath(Node startNode, Node endNode)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            Node currentNode = endNode;

            while (!Equals(currentNode, startNode))
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }
            path.Add(startNode.Position);
            path.Reverse();
            return path;
        }
    }
}