using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Algorithms
{
    public static class Dijkstra
    {
         // Helper function to get valid, walkable, non-diagonal neighbors
    private static List<Vector3Int> GetNeighbors(Vector3Int currentPos, Tilemap tilemap, HashSet<Vector3Int> closedSet)
    {
        List<Vector3Int> neighbors = new List<Vector3Int>();
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborPos = currentPos + dir;

            // Check if the neighbor is walkable and not already fully processed
            if (IsWalkable(neighborPos, tilemap) && !closedSet.Contains(neighborPos))
            {
                neighbors.Add(neighborPos);
            }
        }
        return neighbors;
    }

    // Helper to check if a tile is walkable
    // Customize this based on your tile setup (e.g., specific obstacle tiles).
    private  static bool IsWalkable(Vector3Int position, Tilemap tilemap)
    {
        // Example: Assumes any tile present is walkable. Null means not walkable or out of bounds.
        // You might have layers, specific obstacle tiles, etc.
        // TileBase tile = tilemap.GetTile(position);
        // return tile != null && !IsObstacle(tile); // where IsObstacle checks tile properties

        return tilemap.GetTile(position) != null; // Simple check: tile exists
    }

    private  static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> totalPath = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current); // Insert at the beginning to build path in correct order
        }
        return totalPath;
    }

    public  static List<Vector3Int> FindPath(Vector3Int startTile, Vector3Int endTile, Tilemap tilemap)
    {
        // 1. Basic Sanity Checks
        if (!IsWalkable(startTile, tilemap))
        {
            Debug.LogError("Start tile is not walkable!");
            return null;
        }
        if (!IsWalkable(endTile, tilemap))
        {
            Debug.LogError("End tile is not walkable!");
            return null;
        }
        if (startTile == endTile)
        {
            return new List<Vector3Int> { startTile };
        }

        // 2. Initialization
        // Using a List for openSet and sorting it to simulate a Priority Queue.
        // For very large maps, a more efficient Priority Queue (e.g., a min-heap) would be better.
        List<Vector3Int> openSet = new List<Vector3Int>();
        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>(); // Tiles already evaluated

        // cameFrom[n] is the node immediately preceding n on the cheapest path from start to n currently known.
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        // gScore[n] is the cost of the cheapest path from start to n currently known.
        Dictionary<Vector3Int, int> gScore = new Dictionary<Vector3Int, int>();

        // Initialize gScore for all potential nodes to infinity (or a very large number)
        // For Dijkstra, we only need to initialize the start node. Others are added as discovered.
        // We can use a default value for dictionary lookups if a key is not found.
        
        openSet.Add(startTile);
        gScore[startTile] = 0;

        // 3. Main Loop
        while (openSet.Count > 0)
        {
            // Find the node in openSet having the lowest gScore value
            // This is where a real Priority Queue would be efficient.
            // With a List, we sort or iterate to find the minimum.
            Vector3Int current = openSet[0];
            foreach (var node in openSet)
            {
                if (gScore.GetValueOrDefault(node, int.MaxValue) < gScore.GetValueOrDefault(current, int.MaxValue))
                {
                    current = node;
                }
            }
            // --- Alternative way to get minimum if openSet is always sorted by gScore ---
            // openSet.Sort((a, b) => gScore.GetValueOrDefault(a, int.MaxValue).CompareTo(gScore.GetValueOrDefault(b, int.MaxValue)));
            // Vector3Int current = openSet[0];


            // If the current node is the target, we found the path
            if (current == endTile)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current); // Mark current as evaluated

            // Process neighbors
            foreach (Vector3Int neighbor in GetNeighbors(current, tilemap, closedSet))
            {
                // Tentative gScore for the neighbor through current
                // Cost to move from one tile to an adjacent one is 1
                int tentativeGScore = gScore.GetValueOrDefault(current, int.MaxValue) + 1;

                if (tentativeGScore < gScore.GetValueOrDefault(neighbor, int.MaxValue))
                {
                    // This path to neighbor is better than any previous one. Record it.
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        // 4. No Path Found
        Debug.LogWarning("Dijkstra: No path found from " + startTile + " to " + endTile);
        return null; // Open set is empty but goal was never reached
    }
    }
}