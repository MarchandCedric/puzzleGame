using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMover : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float moveDuration = 0.12f;

    private Vector2Int gridPosition = Vector2Int.zero;
    private bool isMoving = false;

    private readonly HashSet<Vector2Int> blockedCells = new()
    {
        new Vector2Int(2, 0),
        new Vector2Int(2, 1),
        new Vector2Int(2, 2)
    };

    private void Start()
    {
        transform.position = GridToWorld(gridPosition);
    }

    private void Update()
    {
        if (isMoving)
            return;

        Vector2Int direction = Vector2Int.zero;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            direction = new Vector2Int(0, 1);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            direction = new Vector2Int(0, -1);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            direction = new Vector2Int(-1, 0);
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            direction = new Vector2Int(1, 0);

        if (direction == Vector2Int.zero)
            return;

        Vector2Int targetGridPosition = gridPosition + direction;

        if (blockedCells.Contains(targetGridPosition))
            return;

        StartCoroutine(MoveToCell(targetGridPosition));
    }

    private IEnumerator MoveToCell(Vector2Int targetGridPosition)
    {
        isMoving = true;

        Vector3 start = transform.position;
        Vector3 end = GridToWorld(targetGridPosition);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        gridPosition = targetGridPosition;
        isMoving = false;
    }

    private Vector3 GridToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, 1f, cell.y * cellSize);
    }
}