using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GridMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard board;

    [Header("Grid Settings")]
    [SerializeField] private Vector2Int startGridPosition = Vector2Int.zero;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float worldHeight = 1f;
    [SerializeField] private float moveDuration = 0.12f;

    private Vector2Int gridPosition = Vector2Int.zero;
    private bool isMoving = false;

    private void Awake()
    {
        if (board == null)
            board = FindAnyObjectByType<GridBoard>();

        if (board != null)
            cellSize = board.CellSize;
    }

    private void Start()
    {
        gridPosition = startGridPosition;
        transform.position = GridToWorld(gridPosition);
    }

    private void Update()
    {
        if (isMoving)
            return;

        Vector2Int direction = ReadMoveInput();
        if (direction == Vector2Int.zero)
            return;

        Vector2Int targetGridPosition = gridPosition + direction;
        if (!CanMoveTo(targetGridPosition))
            return;

        StartCoroutine(MoveToCell(targetGridPosition));
    }

    private Vector2Int ReadMoveInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector2Int.zero;

        if (WasPressedThisFrame(keyboard.upArrowKey, keyboard.wKey, keyboard.zKey))
            return Vector2Int.up;

        if (WasPressedThisFrame(keyboard.downArrowKey, keyboard.sKey))
            return Vector2Int.down;

        if (WasPressedThisFrame(keyboard.leftArrowKey, keyboard.aKey, keyboard.qKey))
            return Vector2Int.left;

        if (WasPressedThisFrame(keyboard.rightArrowKey, keyboard.dKey))
            return Vector2Int.right;

        return Vector2Int.zero;
    }

    private bool CanMoveTo(Vector2Int targetGridPosition)
    {
        if (board != null)
            return board.IsWalkable(targetGridPosition);

        return true;
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
        if (board != null)
            return board.GridToWorld(cell, worldHeight);

        return new Vector3(cell.x * cellSize, worldHeight, cell.y * cellSize);
    }

    private static bool WasPressedThisFrame(params KeyControl[] keys)
    {
        foreach (KeyControl key in keys)
        {
            if (key != null && key.wasPressedThisFrame)
                return true;
        }

        return false;
    }
}
