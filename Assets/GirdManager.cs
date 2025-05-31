using System.Collections.Generic;
using Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{

    public Tilemap tilemap;
    public Tile defaultTile;
    public Tile tilePrefab;
    public Tile startTile;
    public Tile endTile;
    public int startX = -5;
    public int maxX = 4;
    public int startY = 0;
    public int maxY = -10;
    public Vector3Int startTilePosition;
    public Vector3Int endTilePosition;
        
    public void Start(){
        tilemap.CompressBounds();
    }

    public void Update(){
        if(Input.GetMouseButtonDown(0) && startTilePosition == Vector3Int.zero){
            SetStartTile();
            Debug.Log("Start tile set");
        }
        if(Input.GetMouseButtonDown(0) && endTilePosition == Vector3Int.zero && startTilePosition != Vector3Int.zero){
            SetEndTile();
            Debug.Log("End tile set");
        }
        if(Input.GetMouseButtonDown(0) && startTilePosition != Vector3Int.zero && endTilePosition != Vector3Int.zero){
            FindPath();
        }
    }

    public void SetStartTile(){
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        if(cellPosition != endTilePosition){
            startTilePosition = cellPosition;
            tilemap.SetTile(cellPosition, startTile);
        }
    }

    public void SetEndTile(){
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3Int cellPosition = tilemap.WorldToCell(mousePosition);
        if(cellPosition != startTilePosition){
            endTilePosition = cellPosition;
            tilemap.SetTile(cellPosition, endTile);
        }
    }

    public void FindPath(){
        Debug.Log("Finding path");
        var path = new List<Vector3Int>();
        
        switch(GameController.instance.selectedAlgorithm){
            case Algorithm.Astar:
                path = Astar.FindPath(startTilePosition, endTilePosition, tilemap);
                Debug.Log("Path found: " + path.Count);
                Debug.Log(string.Join("-", path));  
                DrawPath(path);
                break;
            case Algorithm.Dijkstra:
                path = Dijkstra.FindPath(startTilePosition, endTilePosition, tilemap);
                Debug.Log("Dijkstra Path found: " + path.Count);
                Debug.Log(string.Join("-", path));
                DrawPath(path);
                break;
            case Algorithm.BruteForce:
                path = Astar.FindPath(startTilePosition, endTilePosition, tilemap);
                Debug.Log("Path found: " + path.Count);
                Debug.Log(string.Join("-", path));
                DrawPath(path);
                break;
            case Algorithm.GeneticAlgorithm:
                path = Geneticv2.FindPath(startTilePosition, endTilePosition, tilemap);
                Debug.Log("Genetic Algorithm Path found: " + path.Count);
                Debug.Log(string.Join("-", path));
                DrawPath(path);
                break;
        }
    }
    private void DrawPath(List<Vector3Int> path){
        foreach(Vector3Int cell in path){
            tilemap.SetTile(cell, tilePrefab);
        }
    }
        
    public void OnClickResetButton(){
        startTilePosition = Vector3Int.zero;
        endTilePosition = Vector3Int.zero;
        foreach(Vector3Int cell in tilemap.cellBounds.allPositionsWithin){
            tilemap.SetTile(cell, defaultTile);
        }
    }
}