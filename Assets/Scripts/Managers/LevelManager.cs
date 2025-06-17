using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using PathfindingCore;

namespace Managers
{
    public class LevelManager : MonoBehaviour, ILevelManager
    {
        [Header("Level Configuration")]
        [SerializeField] private LevelData[] levels;
        [SerializeField] private int currentLevelIndex = 0;
        
        [Header("Grid Configuration")]
        [SerializeField] private int startX = -5;
        [SerializeField] private int startY = 0;
        
        [Header("Tile References")]
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Tile defaultTile;
        [SerializeField] private Tile obstacleTile;
        [SerializeField] private Tile startTile;
        [SerializeField] private Tile endTile;

        private Vector3Int currentStartPosition;
        private Vector3Int currentEndPosition;
        private bool isLevelLoaded = false;

        public static event Action<int> OnLevelChanged;
        public static event Action<Vector3Int, Vector3Int> OnStartEndPositionsChanged;

        private void Start()
        {
            LoadCurrentLevel();
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levels.Length)
            {
                Debug.LogError($"Level index {levelIndex} is out of range. Available levels: 0-{levels.Length - 1}");
                return;
            }

            currentLevelIndex = levelIndex;
            LoadCurrentLevel();
        }

        public void NextLevel()
        {
            if (currentLevelIndex < levels.Length - 1)
            {
                currentLevelIndex++;
                LoadCurrentLevel();
            }
            else
            {
                Debug.Log("Already at the last level!");
            }
        }

        public void ResetLevel()
        {
            if (isLevelLoaded)
            {
                LoadCurrentLevel();
            }
        }

        public int GetCurrentLevel() => currentLevelIndex;

        public bool IsLevelComplete() => isLevelLoaded;

        public Vector3Int GetStartPosition() => currentStartPosition;

        public Vector3Int GetEndPosition() => currentEndPosition;

        private void LoadCurrentLevel()
        {
            if (levels == null || levels.Length == 0)
            {
                Debug.LogError("No levels configured!");
                return;
            }

            var level = levels[currentLevelIndex];
            ClearGrid();
            SetupLevelGrid(level);
            
            isLevelLoaded = true;
            OnLevelChanged?.Invoke(currentLevelIndex);
            OnStartEndPositionsChanged?.Invoke(currentStartPosition, currentEndPosition);
            
        }

        private void SetupLevelGrid(LevelData level)
        {
            for (int row = 0; row < level.grid.Count; row++)
            {
                for (int col = 0; col < level.grid[row].row.Count; col++)
                {
                    var worldX = startX + col;
                    var worldY = startY - row;
                    var cellPosition = new Vector3Int(worldX, worldY, 0);
                    var tileType = (TileType)level.grid[row].row[col];

                    SetTileAtPosition(cellPosition, tileType);
                }
            }
        }

        private void SetTileAtPosition(Vector3Int position, TileType tileType)
        {
            var worldPosition = tilemap.WorldToCell(new Vector3(position.x, position.y, 0));
            
            switch (tileType)
            {
                case TileType.Empty:
                    tilemap.SetTile(worldPosition, defaultTile);
                    break;
                case TileType.Obstacle:
                    tilemap.SetTile(worldPosition, obstacleTile);
                    break;
                case TileType.Start:
                    tilemap.SetTile(worldPosition, startTile);
                    currentStartPosition = worldPosition;
                    break;
                case TileType.End:
                    tilemap.SetTile(worldPosition, endTile);
                    currentEndPosition = worldPosition;
                    break;
                default:
                    Debug.LogWarning($"Unknown tile type: {tileType}");
                    tilemap.SetTile(worldPosition, defaultTile);
                    break;
            }
        }

        private void ClearGrid()
        {
            if (tilemap != null)
            {
                foreach (var position in tilemap.cellBounds.allPositionsWithin)
                {
                    tilemap.SetTile(position, defaultTile);
                    tilemap.SetColor(position, Color.white);
                }
            }
        }

        public Grid CreateGrid()
        {
            if (!isLevelLoaded)
            {
                Debug.LogError("Cannot create grid - no level loaded!");
                return null;
            }

            return new Grid(tilemap, obstacleTile, currentStartPosition, currentEndPosition);
        }
    }
} 