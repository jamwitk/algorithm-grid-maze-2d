using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Algorithms
{
    /// <summary>
    /// Brute-force (breadth-first) path-finding algorithm.
    /// Explores the grid level-by-level until the destination is reached.
    /// The implementation mirrors the visualisation interface of Dijkstra and A* (Astar.StepData).
    /// </summary>
    public static class Bruteforce
    {
        private class Node
        {
            public Vector3Int Position { get; }
            public Node Parent { get; set; }
            public Node(Vector3Int position) => Position = position;
            public override bool Equals(object obj) => obj is Node other && Position.Equals(other.Position);
            public override int GetHashCode() => Position.GetHashCode();
        }

        // 4-way movement (no diagonals)
        private static readonly Vector3Int[] Directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),   // right
            new Vector3Int(-1, 0, 0),  // left
            new Vector3Int(0, 1, 0),   // up
            new Vector3Int(0, -1, 0)   // down
        };

        /// <summary>
        /// Performs a breadth-first search starting at <paramref name="startPos"/> until <paramref name="endPos"/> is found.
        /// After each node expansion the routine yields a <see cref="Astar.StepData"/> instance so the caller can
        /// visualise the search (identical contract to other algorithms in this project).
        /// </summary>
        public static IEnumerator FindPath(Vector3Int startPos, Vector3Int endPos)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int iterations = 0;
            HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
            Queue<Node> queue = new Queue<Node>();

            Node startNode = new Node(startPos);
            queue.Enqueue(startNode);
            visited.Add(startPos);

            while (queue.Count > 0)
            {
                Node current = queue.Dequeue();
                iterations++;

                // Collect neighbours for visualisation BEFORE enqueuing them so the colours line up intuitively.
                List<Vector3Int> neighborPositions = new List<Vector3Int>();

                foreach (Vector3Int dir in Directions)
                {
                    Vector3Int neighborPos = current.Position + dir;
                    if (!IsValid(neighborPos) || visited.Contains(neighborPos))
                        continue;
                    neighborPositions.Add(neighborPos);
                }

                // Yield current exploration step
                yield return new Astar.StepData(RetracePath(startNode, current), neighborPositions, current.Position);

                // Enqueue neighbours for subsequent exploration
                foreach (Vector3Int neighborPos in neighborPositions)
                {
                    visited.Add(neighborPos);
                    Node neighbor = new Node(neighborPos) { Parent = current };
                    queue.Enqueue(neighbor);

                    // If we reached the destination, yield final path and terminate immediately after enqueuing.
                    if (neighborPos == endPos)
                    {
                        var path = RetracePath(startNode, neighbor);
                        yield return new Astar.StepData(path, new List<Vector3Int>(), neighborPos);

                        stopwatch.Stop();
                        AlgorithmAnalytics.Log(new AlgorithmAnalytics.ResultData("Bruteforce", path.Count, stopwatch.ElapsedMilliseconds, visited.Count, iterations, true));
                        yield break;
                    }
                }
            }

            // No path found – simply yield break so GridManager can finalise visualisation.
            stopwatch.Stop();
            AlgorithmAnalytics.Log(new AlgorithmAnalytics.ResultData("Bruteforce", 0, stopwatch.ElapsedMilliseconds, visited.Count, iterations, false));
            yield break;
        }

        private static bool IsValid(Vector3Int position)
        {
            return GridManager.instance.IsWalkable(position);
        }

        private static List<Vector3Int> RetracePath(Node startNode, Node endNode)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            Node currentNode = endNode;

            while (currentNode != startNode)
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
