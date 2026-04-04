using UnityEngine;
using System.Collections.Generic;

public class GridGroundTile : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private bool computeFootprintFromScale = true;
    [SerializeField] private Vector2Int sizeInCells = Vector2Int.one;

    public Vector3Int GridPosition => gridPosition;
    public Vector2Int SizeInCells => sizeInCells;

    private void OnValidate()
    {
        if (board == null)
            board = GetComponentInParent<GridBoard>();

        if (board == null)
            board = FindAnyObjectByType<GridBoard>();

        if (board == null)
            return;

        if (computeGridPositionFromTransform)
            gridPosition = board.WorldToGrid(transform.position, heightOffset);

        if (computeFootprintFromScale)
        {
            int sizeX = Mathf.Max(1, Mathf.RoundToInt(transform.lossyScale.x / board.CellSize));
            int sizeZ = Mathf.Max(1, Mathf.RoundToInt(transform.lossyScale.z / board.CellSize));
            sizeInCells = new Vector2Int(sizeX, sizeZ);
        }

        if (!snapToGridOnValidate)
            return;

        Vector3Int anchorCell = GetAnchorCell();
        Vector3 anchorWorld = board.GridToWorld(anchorCell, heightOffset);
        float centeredOffsetX = (sizeInCells.x - 1) * board.CellSize * 0.5f;
        float centeredOffsetZ = (sizeInCells.y - 1) * board.CellSize * 0.5f;

        transform.position = anchorWorld + new Vector3(centeredOffsetX, 0f, centeredOffsetZ);
    }

    public IEnumerable<Vector3Int> GetCoveredCells()
    {
        Vector3Int anchorCell = GetAnchorCell();

        for (int z = 0; z < sizeInCells.y; z++)
        {
            for (int x = 0; x < sizeInCells.x; x++)
                yield return new Vector3Int(anchorCell.x + x, anchorCell.y, anchorCell.z + z);
        }
    }

    private Vector3Int GetAnchorCell()
    {
        int offsetX = Mathf.FloorToInt((sizeInCells.x - 1) * 0.5f);
        int offsetZ = Mathf.FloorToInt((sizeInCells.y - 1) * 0.5f);
        return new Vector3Int(gridPosition.x - offsetX, gridPosition.y, gridPosition.z - offsetZ);
    }
}
