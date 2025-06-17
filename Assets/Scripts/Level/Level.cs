using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Row
{
    public List<int> row = new List<int>(); // Initialize to avoid null issues

    // Constructor to create a row with a specific width and default values
    public Row(int width)
    {
        for (int i = 0; i < width; i++)
        {
            row.Add(0); // Add default values (e.g., 0) to fill the row
        }
    }

    // A default constructor is good practice for serialization, though Unity can often manage without it
    // if the parameterized one is used consistently during initialization.
 
}
[CreateAssetMenu(fileName = "Level", menuName = "Level")]
public class Level : ScriptableObject
{
    public string levelName;
    // Grid dimensions
    public int gridWidth = 5;  // Default to a reasonable size
    public int gridHeight = 5; // Default to a reasonable size

    // The grid data
    public List<Row> grid = new List<Row>();

    // This method is called when the scriptable object asset is created or when the game starts/scripts are reloaded.
    // It's a good place to ensure data is initialized.
    void OnEnable()
    {
        // Check if the grid needs initialization or resizing due to inconsistent data
        // This can happen if the asset was created, or if gridWidth/gridHeight were changed
        // and the grid structure wasn't updated.
        if (grid == null || grid.Count != gridHeight)
        {
            InitializeGrid();
        }
        else
        {
            // Further check if individual rows are correctly sized
            for (int i = 0; i < grid.Count; i++)
            {
                if (grid[i] == null || grid[i].row == null || grid[i].row.Count != gridWidth)
                {
                    // If any row is malformed, re-initialize the whole grid.
                    // A more complex ResizeGrid might try to preserve data.
                    InitializeGrid();
                    break; // Exit after re-initialization
                }
            }
        }
    }

    // Initializes the grid with new dimensions, populating with default values.
    public void InitializeGrid()
    {
        grid = new List<Row>(gridHeight); // Set initial capacity for the list of rows
        for (int i = 0; i < gridHeight; i++)
        {
            grid.Add(new Row(gridWidth)); // Add a new Row, which now correctly populates its inner list
        }
    }

    // Resizes the grid, attempting to preserve existing data where possible.
    public void ResizeGrid()
    {
        List<Row> oldGrid = new List<Row>(grid); // Keep a copy of the old grid

        grid = new List<Row>(gridHeight); // Create the new grid structure

        for (int y = 0; y < gridHeight; y++)
        {
            Row newRow = new Row(gridWidth); // Create a new row, correctly populated with defaults
            if (y < oldGrid.Count && oldGrid[y] != null && oldGrid[y].row != null)
            {
                // If there was an old row at this y-position, copy its data
                for (int x = 0; x < Mathf.Min(gridWidth, oldGrid[y].row.Count); x++)
                {
                    newRow.row[x] = oldGrid[y].row[x];
                }
            }
            grid.Add(newRow);
        }
    }
}

