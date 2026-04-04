using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GridMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard board;

    [Header("Grid Settings")]
    [SerializeField] private float moveDuration = 0.12f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private bool snapToGridOnStart = true;

    private Vector3Int gridPosition = Vector3Int.zero;
    private bool isMoving = false;

    private void Awake()
    {
        if (board == null)
            board = FindAnyObjectByType<GridBoard>();
    }

    private void Start()
    {
        gridPosition = ResolveCurrentGridPosition();

        if (snapToGridOnStart)
            transform.position = GridToWorld(gridPosition);
    }

    private void Update()
    {
        if (isMoving)
            return;

        Vector3Int direction = ReadMoveInput();
        if (direction == Vector3Int.zero)
            return;

        Vector3Int targetGridPosition = gridPosition + direction;
        if (!CanMoveTo(targetGridPosition))
            return;

        StartCoroutine(MoveToCell(targetGridPosition));
    }

    private Vector3Int ReadMoveInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector3Int.zero;

        if (WasPressedThisFrame(keyboard.upArrowKey, keyboard.wKey, keyboard.zKey))
            return new Vector3Int(1, 0, 0);

        if (WasPressedThisFrame(keyboard.downArrowKey, keyboard.sKey))
            return new Vector3Int(-1, 0, 0);

        if (WasPressedThisFrame(keyboard.leftArrowKey, keyboard.aKey, keyboard.qKey))
            return new Vector3Int(0, 0, 1);

        if (WasPressedThisFrame(keyboard.rightArrowKey, keyboard.dKey))
            return new Vector3Int(0, 0, -1);

        return Vector3Int.zero;
    }

    private bool CanMoveTo(Vector3Int targetGridPosition)
    {
        if (board != null)
            return board.IsWalkable(targetGridPosition);

        return true;
    }

    private IEnumerator MoveToCell(Vector3Int targetGridPosition)
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

    private Vector3 GridToWorld(Vector3Int cell)
    {
        if (board != null)
            return board.GridToWorld(cell, heightOffset);

        return new Vector3(cell.x, cell.y + heightOffset, cell.z);
    }

    private Vector3Int ResolveCurrentGridPosition()
    {
        if (board != null)
            return board.WorldToGrid(transform.position, heightOffset);

        return Vector3Int.RoundToInt(new Vector3(transform.position.x, transform.position.y - heightOffset, transform.position.z));
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
