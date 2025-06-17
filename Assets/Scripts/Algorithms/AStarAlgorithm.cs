using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using PathfindingCore;

namespace Algorithms
{
    public class AStarAlgorithm : IPathfindingAlgorithm
    {
        private class Node
        {
            public Vector3Int Position { get; }
            public int GCost { get; set; }
            public int HCost { get; set; }
            public int FCost => GCost + HCost;
            public Node Parent { get; set; }

            public Node(Vector3Int position)
            {
                Position = position;
            }

            public override bool Equals(object obj) => obj is Node other && Position.Equals(other.Position);
            public override int GetHashCode() => Position.GetHashCode();
        }

        public class AStarVisualizationStep : BaseVisualizationStep
        {
            public int Iterations { get; }
            public int NodesExplored { get; }

            public AStarVisualizationStep(List<Vector3Int> path, List<Vector3Int> neighbors, 
                                        Vector3Int current, int iterations, int nodesExplored)
                : base(path, neighbors, current)
            {
                Iterations = iterations;
                NodesExplored = nodesExplored;
            }

            public override string GetStepInfo()
            {
                return $"A* | Path Length: {currentPath.Count} | Neighbors: {neighbors.Count} | Iterations: {Iterations} | Explored: {NodesExplored}";
            }
        }

        public string GetAlgorithmName() => "A* (A-Star)";

        public IEnumerator FindPath(Vector3Int start, Vector3Int end, IGrid grid)
        {
            var stopwatch = Stopwatch.StartNew();
            int iterations = 0;

            var startNode = new Node(start);
            var endNode = new Node(end);

            var openSet = new List<Node>();
            var closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = GetLowestFCostNode(openSet);

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                // Check if we reached the target
                if (currentNode.Position.Equals(endNode.Position))
                {
                    var finalPath = RetracePath(startNode, currentNode);
                    stopwatch.Stop();
                    
                    LogResult(finalPath, stopwatch.ElapsedMilliseconds, closedSet.Count, iterations, true);
                    
                    yield return new AStarVisualizationStep(finalPath, new List<Vector3Int>(), 
                                                          currentNode.Position, iterations, closedSet.Count);
                    yield break;
                }

                // Get neighbors and create visualization step
                var neighbors = GetNeighbors(currentNode, grid);
                var neighborPositions = new List<Vector3Int>();
                foreach (var neighbor in neighbors)
                {
                    neighborPositions.Add(neighbor.Position);
                }

                var currentPath = RetracePath(startNode, currentNode);
                yield return new AStarVisualizationStep(currentPath, neighborPositions, 
                                                      currentNode.Position, iterations, closedSet.Count);

                // Process neighbors
                foreach (var neighbor in neighbors)
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    int newGCost = currentNode.GCost + GetDistance(currentNode, neighbor, grid);
                    
                    if (newGCost < neighbor.GCost || !openSet.Contains(neighbor))
                    {
                        neighbor.GCost = newGCost;
                        neighbor.HCost = GetHeuristicDistance(neighbor, endNode, grid);
                        neighbor.Parent = currentNode;

                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);
                    }
                }

                iterations++;
            }

            // No path found
            stopwatch.Stop();
            LogResult(null, stopwatch.ElapsedMilliseconds, closedSet.Count, iterations, false);
            yield return null;
        }

        private Node GetLowestFCostNode(List<Node> nodes)
        {
            Node lowestNode = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].FCost < lowestNode.FCost || 
                    (nodes[i].FCost == lowestNode.FCost && nodes[i].HCost < lowestNode.HCost))
                {
                    lowestNode = nodes[i];
                }
            }
            return lowestNode;
        }

        private List<Node> GetNeighbors(Node node, IGrid grid)
        {
            var neighbors = new List<Node>();
            
            foreach (var neighborPosition in grid.GetNeighbors(node.Position))
            {
                neighbors.Add(new Node(neighborPosition));
            }

            return neighbors;
        }

        private int GetDistance(Node nodeA, Node nodeB, IGrid grid)
        {
            if (grid is Grid specificGrid)
            {
                return specificGrid.GetDistance(nodeA.Position, nodeB.Position);
            }
            
            // Fallback to Manhattan distance
            return Mathf.Abs(nodeA.Position.x - nodeB.Position.x) + 
                   Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
        }

        private int GetHeuristicDistance(Node nodeA, Node nodeB, IGrid grid)
        {
            if (grid is Grid specificGrid)
            {
                return specificGrid.GetHeuristicDistance(nodeA.Position, nodeB.Position);
            }
            
            // Fallback to Manhattan distance
            return Mathf.Abs(nodeA.Position.x - nodeB.Position.x) + 
                   Mathf.Abs(nodeA.Position.y - nodeB.Position.y);
        }

        private List<Vector3Int> RetracePath(Node startNode, Node endNode)
        {
            var path = new List<Vector3Int>();
            var currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.Position);
                currentNode = currentNode.Parent;
            }
            
            path.Add(startNode.Position);
            path.Reverse();
            return path;
        }

        private void LogResult(List<Vector3Int> path, long elapsedMs, int nodesExplored, int iterations, bool success)
        {
            var result = new PathfindingResult(
                path, 
                success, 
                elapsedMs, 
                nodesExplored, 
                iterations, 
                GetAlgorithmName()
            );

            UnityEngine.Debug.Log($"A* Result: Path Found: {success}, Length: {result.Path.Count}, " +
                                  $"Time: {elapsedMs}ms, Nodes Explored: {nodesExplored}, Iterations: {iterations}");
        }
    }
} 