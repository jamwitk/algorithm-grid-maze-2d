using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Level))]
public class LevelConfigEditor : Editor
{
    
    
    public override void OnInspectorGUI()
    {
        // Get the target ScriptableObject
        Level levelConfig = (Level)target;

        // Draw the default inspector fields for gridWidth and gridHeight
        EditorGUI.BeginChangeCheck();
        int newWidth = EditorGUILayout.IntField("Grid Width", levelConfig.gridWidth);
        int newHeight = EditorGUILayout.IntField("Grid Height", levelConfig.gridHeight);

        // If width or height changed, resize the grid
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(levelConfig, "Resize Grid"); // For undo functionality
            levelConfig.gridWidth = newWidth > 0 ? newWidth : 1; // Ensure positive dimensions
            levelConfig.gridHeight = newHeight > 0 ? newHeight : 1;
            levelConfig.ResizeGrid();
            EditorUtility.SetDirty(levelConfig); // Mark the object as dirty to ensure changes are saved
        }

        // Ensure grid is initialized if it's null or empty, especially after creation
        if (levelConfig.grid == null || levelConfig.grid.Count != levelConfig.gridHeight ||
            (levelConfig.grid.Count > 0 && levelConfig.grid[0].row.Count != levelConfig.gridWidth))
        {
            levelConfig.InitializeGrid();
            EditorUtility.SetDirty(levelConfig);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Grid", EditorStyles.boldLabel);

        // Draw the grid
        for (int y = 0; y < levelConfig.gridHeight; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < levelConfig.gridWidth; x++)
            {
                // Ensure the row and column exist
                if (y < levelConfig.grid.Count && x < levelConfig.grid[y].row.Count)
                {
                    EditorGUI.BeginChangeCheck();
                    // Display an integer field for each cell. You can change this to other types.
                    int cellValue = EditorGUILayout.IntField(levelConfig.grid[y].row[x], GUILayout.Width(30)); // Adjust width as needed
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(levelConfig, "Edit Grid Cell");
                        levelConfig.grid[y].row[x] = cellValue;
                        EditorUtility.SetDirty(levelConfig);
                    }
                }
                else
                {
                    // This should ideally not be reached if InitializeGrid and ResizeGrid work correctly
                    EditorGUILayout.LabelField("ERR", GUILayout.Width(30));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        // Apply changes to the serialized object
        // While SetDirty is often enough for ScriptableObjects,
        // serializedObject.ApplyModifiedProperties() is good practice if you were using SerializedProperty.
        // In this direct manipulation case, SetDirty is key.
    }
}