using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PathfindingCore
{
    public class Grid : IGrid
    {
        private readonly Tilemap tilemap;
        private readonly Tile obstacleTile;
        private readonly Vector3Int startPosition;
        private readonly Vector3Int endPosition;
        private readonly BoundsInt bounds;

        private static readonly Vector3Int[] DirectionalOffsets = {
            new Vector3Int(1, 0, 0),   // Right
            new Vector3Int(-1, 0, 0),  // Left
            new Vector3Int(0, 1, 0),   // Up
            new Vector3Int(0, -1, 0)   // Down
        };

        public Grid(Tilemap tilemap, Tile obstacleTile, Vector3Int start, Vector3Int end)
        {
            this.tilemap = tilemap ?? throw new System.ArgumentNullException(nameof(tilemap));
            this.obstacleTile = obstacleTile ?? throw new System.ArgumentNullException(nameof(obstacleTile));
            this.startPosition = start;
            this.endPosition = end;
            this.bounds = tilemap.cellBounds;
        }

        public bool IsWalkable(Vector3Int position)
        {
            if (!IsInBounds(position))
                return false;

            var tile = tilemap.GetTile(position);
            return tile != null && tile != obstacleTile;
        }

        public bool IsInBounds(Vector3Int position)
        {
            return bounds.Contains(position);
        }

        public Vector3Int GetStartPosition() => startPosition;

        public Vector3Int GetEndPosition() => endPosition;

        public IEnumerable<Vector3Int> GetNeighbors(Vector3Int position)
        {
            foreach (var offset in DirectionalOffsets)
            {
                var neighborPosition = position + offset;
                if (IsWalkable(neighborPosition))
                {
                    yield return neighborPosition;
                }
            }
        }

        public int GetDistance(Vector3Int from, Vector3Int to)
        {
            int deltaX = Mathf.Abs(from.x - to.x);
            int deltaY = Mathf.Abs(from.y - to.y);
            
            // Using Manhattan distance for grid-based movement
            return deltaX + deltaY;
        }

        public int GetHeuristicDistance(Vector3Int from, Vector3Int to)
        {
            // Manhattan distance heuristic for grid-based pathfinding
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }
    }
} 