using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Algorithms
{
    public static class Dijkstra
    {
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

        public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos)
        {
            Node startNode = new Node(startPos) { Distance = 0 };
            Node endNode = new Node(endPos);

            List<Node> unvisited = new List<Node>();
            HashSet<Node> visited = new HashSet<Node>();
            unvisited.Add(startNode);

            while (unvisited.Count > 0)
            {
                Node current = unvisited[0];
                for (int i = 1; i < unvisited.Count; i++)
                {
                    if (unvisited[i].Distance < current.Distance)
                        current = unvisited[i];
                }

                if (current.Position.Equals(endNode.Position))
                    return RetracePath(startNode, current);

                unvisited.Remove(current);
                visited.Add(current);

                foreach (Node neighbor in GetNeighbors(current))
                {
                    if (visited.Contains(neighbor))
                        continue;

                    int newDistance = current.Distance + 1;
                    if (newDistance < neighbor.Distance)
                    {
                        neighbor.Distance = newDistance;
                        neighbor.Parent = current;
                        if (!unvisited.Contains(neighbor))
                            unvisited.Add(neighbor);
                    }
                }
            }

            return null;
        }

        private static List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            Vector3Int[] directions = new Vector3Int[]
            {
                new Vector3Int(1, 0, 0),  // right
                new Vector3Int(-1, 0, 0), // left
                new Vector3Int(0, 1, 0),  // up
                new Vector3Int(0, -1, 0)  // down
            };

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighborPos = node.Position + dir;
                if (IsValid(neighborPos))
                {
                    neighbors.Add(new Node(neighborPos));
                }
            }

            return neighbors;
        }

        private static bool IsValid(Vector3Int position){
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