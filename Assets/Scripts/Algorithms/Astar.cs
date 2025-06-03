using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class Astar{
    private class Node
    {
        public Vector3Int Position { get; }
        public int GCost { get; set; } // Cost from start to this node
        public int HCost { get; set; } // Heuristic cost from this node to end
        public int FCost => GCost + HCost; // Total cost
        public Node Parent { get; set; }
        public Node(Vector3Int position)
        {
            Position = position;
        }
        public override bool Equals(object obj) => obj is Node other && Position.Equals(other.Position);
        public override int GetHashCode() => Position.GetHashCode();
    }

    public static List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int endPos)
    {
        Node startNode = new Node(startPos);
        Node endNode = new Node(endPos);

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || 
                    (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode.Position.Equals(endNode.Position))
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);
                if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
                {
                    neighbor.GCost = newMovementCostToNeighbor;
                    neighbor.HCost = GetDistance(neighbor, endNode);
                    neighbor.Parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null; // No path found
    }

    private static List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(1, 0, 0),  // right
            new Vector3Int(-1, 0, 0), // left
            new Vector3Int(0, 1, 0),  // up
            new Vector3Int(0, -1, 0), // down
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = node.Position + dir;
            if (IsWalkable(neighborPos))
            {
                neighbors.Add(new Node(neighborPos));
            }
        }

        return neighbors;
    }

    private static bool IsWalkable(Vector3Int pos)
    {
        return GridManager.instance.IsWalkable(pos);
    }

    private static int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.Position.x - nodeB.Position.x);
        int distY = Mathf.Abs(nodeA.Position.y - nodeB.Position.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        return 14 * distX + 10 * (distY - distX);
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
