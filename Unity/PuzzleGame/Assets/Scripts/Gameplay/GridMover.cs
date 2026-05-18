using System;
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
    private PlayerKeyRing keyRing;
    private IPlayerAnimationController animationController;

    public int MoveCount { get; private set; }
    public Vector3Int CurrentGridPosition => gridPosition;
    public event Action<Vector3Int, int> MoveResolved;

    private void Awake()
    {
        if (board == null)
            board = FindAnyObjectByType<GridBoard>();

        if (keyRing == null)
            keyRing = GetComponent<PlayerKeyRing>();
        
        animationController = FindAnimationController();
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

        TryStartMove(direction);
    }

    public void RequestMoveUp()
    {
        TryStartMove(new Vector3Int(1, 0, 0));
    }

    public void RequestMoveDown()
    {
        TryStartMove(new Vector3Int(-1, 0, 0));
    }

    public void RequestMoveLeft()
    {
        TryStartMove(new Vector3Int(0, 0, 1));
    }

    public void RequestMoveRight()
    {
        TryStartMove(new Vector3Int(0, 0, -1));
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
        EnsureDependenciesResolved();

        if (board != null)
            return board.IsWalkable(targetGridPosition, keyRing);

        return true;
    }

    private bool TryResolveTargetCell(Vector3Int targetGridPosition)
    {
        EnsureDependenciesResolved();

        if (board == null)
            return true;

        return board.TryUnlockDoor(targetGridPosition, keyRing);
    }

    private void TryStartMove(Vector3Int direction)
    {
        if (isMoving || direction == Vector3Int.zero)
            return;

        Vector3Int targetGridPosition = gridPosition + direction;
        if (!TryResolveTargetCell(targetGridPosition))
            return;

        if (!CanMoveTo(targetGridPosition))
            return;

        StartCoroutine(MoveToCell(targetGridPosition));
    }

    private IEnumerator MoveToCell(Vector3Int targetGridPosition)
    {
        isMoving = true;
        animationController?.BeginMove(ToAnimationDirection(targetGridPosition - gridPosition));

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
        MoveCount++;
        board?.ResolveArrival(gridPosition, keyRing);
        MoveResolved?.Invoke(gridPosition, MoveCount);
        animationController?.EndMove();
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

    private IPlayerAnimationController FindAnimationController()
    {
        MonoBehaviour[] behaviours = GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IPlayerAnimationController controller)
                return controller;
        }

        return null;
    }

    private void EnsureDependenciesResolved()
    {
        if (board == null)
            board = FindAnyObjectByType<GridBoard>();

        if (keyRing == null)
        {
            keyRing = GetComponent<PlayerKeyRing>();

            if (keyRing == null)
                keyRing = GetComponentInParent<PlayerKeyRing>();

            if (keyRing == null)
                keyRing = GetComponentInChildren<PlayerKeyRing>(true);
        }

        if (animationController == null)
            animationController = FindAnimationController();
    }

    private static MoveAnimationDirection ToAnimationDirection(Vector3Int movement)
    {
        if (movement == new Vector3Int(1, 0, 0))
            return MoveAnimationDirection.Up;

        if (movement == new Vector3Int(-1, 0, 0))
            return MoveAnimationDirection.Down;

        if (movement == new Vector3Int(0, 0, 1))
            return MoveAnimationDirection.Left;

        if (movement == new Vector3Int(0, 0, -1))
            return MoveAnimationDirection.Right;

        return MoveAnimationDirection.None;
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
