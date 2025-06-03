using System.Collections.Generic;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    public Tilemap tilemap;
    public Tile defaultTile;
    public Tile tilePrefab;
    public Tile obstacleTile;
    public Tile startTile;
    public Tile endTile;
    public Tile neighborTile;
    public int startX = -5;
    public int maxX = 4;
    public int startY = 0;
    public int maxY = -10;
    public Vector3Int startTilePosition;
    public Vector3Int endTilePosition;
    public Level[] levels;
    public int currentLevel = 0;

    public int currentLevelSolved = 0;
    public void Awake(){
        instance = this;
    }
    public void Start(){
        tilemap.CompressBounds();
        SetLevel();
    }
    private void SetLevel(){
        
        for(int i = 0; i < levels[currentLevel].grid.Count; i++){
            for(int j = 0; j < levels[currentLevel].grid[i].row.Count; j++){
                var xValue = startX + j;
                var yValue = startY - i;
                if(levels[currentLevel].grid[i].row[j] == 0){
                    SetTile(xValue, yValue, defaultTile);
                }
                else if(levels[currentLevel].grid[i].row[j] == 1){
                    SetTile(xValue, yValue, obstacleTile);
                }
                else if(levels[currentLevel].grid[i].row[j] == 2){
                    SetTile(xValue, yValue, startTile);
                    startTilePosition = new Vector3Int(xValue, yValue, 0);
                }
                else if(levels[currentLevel].grid[i].row[j] == 3){
                    SetTile(xValue, yValue, endTile);
                    endTilePosition = new Vector3Int(xValue, yValue, 0);
                }
            }
        }
        SetUI();
    }
    private void SetUI(){
        UIManager.instance.SetStartEndTile(startTilePosition, endTilePosition);
    }
    public void SetTile(int j, int i, Tile tile){
        Vector3Int cellPosition = tilemap.WorldToCell(new Vector3(j, i, 0));
        tilemap.SetTile(cellPosition, tile);
    }
    
   
    public IEnumerator DrawPath(List<Vector3Int> path){
        Debug.Log("Drawing path");
        Debug.Log("Path: " + path.Count);
        if(path != null){
            foreach(Vector3Int cell in path){
                if(tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile){
                    tilemap.SetTile(cell, tilePrefab);
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        currentLevelSolved++;    
    }

    public IEnumerator VisualizeAlgorithmSteps(IEnumerator algorithmSteps)
    {
        while (algorithmSteps.MoveNext())
        {
            var step = algorithmSteps.Current;
            if (step == null) continue;

            // Clear previous visualization
            foreach (Vector3Int cell in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile && tilemap.GetTile(cell) != obstacleTile)
                {
                    tilemap.SetTile(cell, defaultTile);
                    tilemap.SetColor(cell, Color.white); // reset color
                }
            }

            // Handle both Astar.StepData and Geneticv2.StepData
            if (step is Astar.StepData astarStep)
            {
                // Draw current path
                foreach (Vector3Int cell in astarStep.currentPath)
                {
                    if (tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile)
                    {
                        tilemap.SetTile(cell, tilePrefab);
                    }
                }

                // Draw neighbors
                foreach (Vector3Int cell in astarStep.neighbors)
                {
                    if (tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile && tilemap.GetTile(cell) != obstacleTile)
                    {
                        tilemap.SetTile(cell, neighborTile);
                    }
                }
            }
            else if (step is Geneticv2.StepData geneticStep)
            {
                // Draw best current path
                foreach (Vector3Int cell in geneticStep.currentPath)
                {
                    if (tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile)
                    {
                        tilemap.SetTile(cell, tilePrefab);
                    }
                }

                // Draw explored neighbors of best path
                foreach (Vector3Int cell in geneticStep.exploredNeighbors)
                {
                    if (tilemap.GetTile(cell) != startTile && tilemap.GetTile(cell) != endTile && tilemap.GetTile(cell) != obstacleTile)
                    {
                        tilemap.SetTile(cell, neighborTile);
                    }
                }

                // Log generation info
                Debug.Log($"Generation {geneticStep.generation + 1} | Best Fitness: {geneticStep.bestFitness} | Reached Target: {geneticStep.reachedTarget}");
            }

            yield return new WaitForSeconds(0.5f);
        }
    }
        
    public bool IsWalkable(Vector3Int position){
         // ensure its not out of current tilemap bounds
        return tilemap.GetTile(position) != null && tilemap.GetTile(position) != obstacleTile;
    }
    public void Reset(){
        startTilePosition = Vector3Int.zero;
        endTilePosition = Vector3Int.zero;
        foreach(Vector3Int cell in tilemap.cellBounds.allPositionsWithin){
            tilemap.SetTile(cell, defaultTile);
        }
        SetLevel();
        UIManager.instance.SetStartEndTile(startTilePosition, endTilePosition);
    }
    public void NextLevel(){
        Reset();
        currentLevel++;
        currentLevelSolved = 0;
        SetLevel();
        UIManager.instance.SetStartEndTile(startTilePosition, endTilePosition);
    }
}