using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridBoard : MonoBehaviour
{
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float layerHeight = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Blocked Cells")]
    [SerializeField] private List<Vector3Int> blockedCells = new()
    {
        new Vector3Int(2, 0, 0),
        new Vector3Int(2, 0, 1),
        new Vector3Int(2, 0, 2)
    };
    [SerializeField] private bool requireSceneGroundTiles = true;
    [SerializeField] private bool includeSceneObstacles = true;
    [SerializeField] private bool searchWholeSceneForObstacles = true;
    [SerializeField] private bool searchWholeSceneForGroundTiles = true;

    public float CellSize => cellSize;
    public float LayerHeight => layerHeight;
    public Vector3 Origin => origin;

    private void OnValidate()
    {
        cellSize = Mathf.Max(0.1f, cellSize);
        layerHeight = Mathf.Max(0.1f, layerHeight);
    }

    public bool IsWalkable(Vector3Int cell)
    {
        if (requireSceneGroundTiles)
        {
            if (!GetGroundCells().Contains(cell))
                return false;
        }
        else if (!IsInsideDerivedBoard(cell))
        {
            return false;
        }

        return !GetBlockedCells().Contains(cell);
    }

    public Vector3 GridToWorld(Vector3Int cell, float yOffset = 0f)
    {
        return origin + new Vector3(cell.x * cellSize, (cell.y * layerHeight) + yOffset, cell.z * cellSize);
    }

    public Vector3Int WorldToGrid(Vector3 worldPosition, float yOffset = 0f)
    {
        Vector3 localPosition = worldPosition - origin - new Vector3(0f, yOffset, 0f);
        int gridX = Mathf.RoundToInt(localPosition.x / cellSize);
        int gridY = Mathf.RoundToInt(localPosition.y / layerHeight);
        int gridZ = Mathf.RoundToInt(localPosition.z / cellSize);
        return new Vector3Int(gridX, gridY, gridZ);
    }

    public HashSet<Vector3Int> GetBlockedCells()
    {
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>(blockedCells);

        if (!includeSceneObstacles)
            return occupiedCells;

        GridObstacle[] obstacles = FindObstacles();
        foreach (GridObstacle obstacle in obstacles)
        {
            if (obstacle == null)
                continue;

            occupiedCells.Add(obstacle.GridPosition);
        }

        return occupiedCells;
    }

    public HashSet<Vector3Int> GetGroundCells()
    {
        HashSet<Vector3Int> groundCells = new HashSet<Vector3Int>();
        GridGroundTile[] groundTiles = FindGroundTiles();

        foreach (GridGroundTile groundTile in groundTiles)
        {
            if (groundTile == null)
                continue;

            foreach (Vector3Int cell in groundTile.GetCoveredCells())
                groundCells.Add(cell);
        }

        return groundCells;
    }

    public bool TryGetDerivedBounds(out GridBounds bounds)
    {
        HashSet<Vector3Int> allCells = new HashSet<Vector3Int>();

        foreach (Vector3Int cell in GetGroundCells())
            allCells.Add(cell);

        foreach (Vector3Int cell in GetBlockedCells())
            allCells.Add(cell);

        if (allCells.Count == 0)
        {
            bounds = default;
            return false;
        }

        bool first = true;
        int minX = 0;
        int maxX = 0;
        int minY = 0;
        int maxY = 0;
        int minZ = 0;
        int maxZ = 0;

        foreach (Vector3Int cell in allCells)
        {
            if (first)
            {
                minX = maxX = cell.x;
                minY = maxY = cell.y;
                minZ = maxZ = cell.z;
                first = false;
                continue;
            }

            minX = Mathf.Min(minX, cell.x);
            maxX = Mathf.Max(maxX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxY = Mathf.Max(maxY, cell.y);
            minZ = Mathf.Min(minZ, cell.z);
            maxZ = Mathf.Max(maxZ, cell.z);
        }

        bounds = new GridBounds(new Vector3Int(minX, minY, minZ), new Vector3Int(maxX, maxY, maxZ));
        return true;
    }

    private GridObstacle[] FindObstacles()
    {
        if (searchWholeSceneForObstacles)
            return FindObjectsByType<GridObstacle>(FindObjectsInactive.Include);

        return GetComponentsInChildren<GridObstacle>(true);
    }

    private GridGroundTile[] FindGroundTiles()
    {
        if (searchWholeSceneForGroundTiles)
            return FindObjectsByType<GridGroundTile>(FindObjectsInactive.Include);

        return GetComponentsInChildren<GridGroundTile>(true);
    }

    private bool IsInsideDerivedBoard(Vector3Int cell)
    {
        if (!TryGetDerivedBounds(out GridBounds bounds))
            return false;

        return cell.x >= bounds.Min.x && cell.x <= bounds.Max.x
            && cell.y >= bounds.Min.y && cell.y <= bounds.Max.y
            && cell.z >= bounds.Min.z && cell.z <= bounds.Max.z;
    }
}

public readonly struct GridBounds
{
    public GridBounds(Vector3Int min, Vector3Int max)
    {
        Min = min;
        Max = max;
    }

    public Vector3Int Min { get; }
    public Vector3Int Max { get; }
}
