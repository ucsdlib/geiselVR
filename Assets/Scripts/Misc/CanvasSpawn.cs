using System.Collections.Generic;
using UnityEngine;

public class CanvasSpawn : MonoBehaviour
{
    public Canvas CanvasTemplate;
    public int GridSize;
    private const float Spacing = 0.5f;

    private List<Canvas> all = new List<Canvas>();
    private int lastGridSize;

    private void Start()
    {
        lastGridSize = GridSize;
        GenerateGrid(GridSize, Spacing);
    }

    private void Update()
    {
        if (GridSize == lastGridSize) return;
            
        lastGridSize = GridSize;
        foreach (var canvas in all)
        {
            Destroy(canvas);
        }
        all.Clear();
        GenerateGrid(GridSize, Spacing);
    }

    private void GenerateGrid(int gridSize, float spacing)
    {
        for (var i = 0; i < gridSize; i++)
        {
            var offset = i * spacing * Vector3.right;
            for (var j = 0; j < gridSize; j++)
            {
                var position = offset + j * spacing * Vector3.forward;
                var canvas = Instantiate(CanvasTemplate);
                canvas.transform.position = position;
                all.Add(canvas);
            }
        }
    }
}