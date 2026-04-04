using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridBoard : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField] private int width = 6;
    [SerializeField] private int height = 6;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Blocked Cells")]
    [SerializeField] private List<Vector2Int> blockedCells = new()
    {
        new Vector2Int(2, 0),
        new Vector2Int(2, 1),
        new Vector2Int(2, 2)
    };

    [Header("Tile Visuals")]
    [SerializeField] private bool generateTilesInEditor = true;
    [SerializeField] private float tileHeight = 0.1f;
    [SerializeField] private float tileInset = 0.08f;
    [SerializeField] private Material tileMaterial = null;
    [SerializeField] private Color walkableTileColor = new Color(0.79f, 0.82f, 0.72f, 1f);
    [SerializeField] private Color blockedTileColor = new Color(0.55f, 0.45f, 0.42f, 1f);

    public float CellSize => cellSize;

    private void OnEnable()
    {
        if (generateTilesInEditor)
            RebuildTiles();
    }

    private void OnValidate()
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        cellSize = Mathf.Max(0.1f, cellSize);
        tileHeight = Mathf.Max(0.02f, tileHeight);
        tileInset = Mathf.Clamp(tileInset, 0f, cellSize * 0.45f);

        if (generateTilesInEditor)
            RebuildTiles();
    }

    public bool IsWalkable(Vector2Int cell)
    {
        return IsInsideBoard(cell) && !blockedCells.Contains(cell);
    }

    public Vector3 GridToWorld(Vector2Int cell, float worldHeight = 0f)
    {
        return origin + new Vector3(cell.x * cellSize, worldHeight, cell.y * cellSize);
    }

    [ContextMenu("Rebuild Tiles")]
    public void RebuildTiles()
    {
        Transform tilesRoot = GetOrCreateTilesRoot();
        ClearChildren(tilesRoot);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.SetParent(tilesRoot, false);
                tile.transform.position = GridToWorld(cell, tileHeight * 0.5f);

                float visualSize = cellSize - (tileInset * 2f);
                tile.transform.localScale = new Vector3(visualSize, tileHeight, visualSize);

                Collider tileCollider = tile.GetComponent<Collider>();
                if (tileCollider != null)
                {
                    if (Application.isPlaying)
                        Destroy(tileCollider);
                    else
                        DestroyImmediate(tileCollider);
                }

                Renderer tileRenderer = tile.GetComponent<Renderer>();
                if (tileRenderer != null)
                {
                    Material materialToUse = tileMaterial != null
                        ? new Material(tileMaterial)
                        : new Material(Shader.Find("Universal Render Pipeline/Lit"));

                    materialToUse.color = blockedCells.Contains(cell) ? blockedTileColor : walkableTileColor;
                    tileRenderer.sharedMaterial = materialToUse;
                }
            }
        }
    }

    private bool IsInsideBoard(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    private Transform GetOrCreateTilesRoot()
    {
        const string rootName = "GeneratedTiles";
        Transform tilesRoot = transform.Find(rootName);

        if (tilesRoot != null)
            return tilesRoot;

        GameObject root = new GameObject(rootName);
        root.transform.SetParent(transform, false);
        return root.transform;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int index = parent.childCount - 1; index >= 0; index--)
        {
            GameObject child = parent.GetChild(index).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }
}
