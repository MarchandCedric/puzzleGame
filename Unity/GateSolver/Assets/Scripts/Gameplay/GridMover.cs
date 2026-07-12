using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class GridMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridBoard board;

    [Header("Grid Settings")]
    [SerializeField] private float moveDuration = 0.18f;
    [SerializeField] private float heightOffset = 1f;
    [SerializeField] private bool snapToGridOnStart = true;

    [Header("Movement Feel")]
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float hopHeight = 0.08f;
    [SerializeField, Min(1)] private int maxBufferedMoves = 12;

    private Vector3Int gridPosition = Vector3Int.zero;
    private bool isMoving = false;
    private Coroutine moveRoutine;
    private readonly Queue<Vector3Int> bufferedMoves = new Queue<Vector3Int>();
    private PlayerKeyRing keyRing;
    private IPlayerAnimationController animationController;

    public int MoveCount { get; private set; }
    public Vector3Int CurrentGridPosition => gridPosition;
    public int BufferedMoveCount => bufferedMoves.Count;
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
        Vector3Int direction = ReadMoveInput();
        if (direction == Vector3Int.zero)
            return;

        RequestMove(direction);
    }

    public void RequestMoveUp()
    {
        RequestMove(new Vector3Int(1, 0, 0));
    }

    public void RequestMoveDown()
    {
        RequestMove(new Vector3Int(-1, 0, 0));
    }

    public void RequestMoveLeft()
    {
        RequestMove(new Vector3Int(0, 0, 1));
    }

    public void RequestMoveRight()
    {
        RequestMove(new Vector3Int(0, 0, -1));
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

    private void RequestMove(Vector3Int direction)
    {
        if (direction == Vector3Int.zero)
            return;

        if (isMoving)
        {
            if (bufferedMoves.Count < maxBufferedMoves)
                bufferedMoves.Enqueue(direction);

            return;
        }

        bufferedMoves.Enqueue(direction);
        moveRoutine = StartCoroutine(ProcessBufferedMoves());
    }

    private IEnumerator ProcessBufferedMoves()
    {
        isMoving = true;

        while (bufferedMoves.Count > 0 && isActiveAndEnabled)
        {
            Vector3Int direction = bufferedMoves.Dequeue();
            Vector3Int targetGridPosition = gridPosition + direction;

            if (!TryResolveTargetCell(targetGridPosition))
                continue;

            if (!CanMoveTo(targetGridPosition))
                continue;

            yield return MoveToCell(targetGridPosition);
        }

        animationController?.EndMove();
        isMoving = false;
        moveRoutine = null;
    }

    private IEnumerator MoveToCell(Vector3Int targetGridPosition)
    {
        animationController?.BeginMove(ToAnimationDirection(targetGridPosition - gridPosition));

        Vector3 start = transform.position;
        Vector3 end = GridToWorld(targetGridPosition);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float easedT = moveCurve != null ? Mathf.Clamp01(moveCurve.Evaluate(t)) : SmoothStep(t);
            Vector3 position = Vector3.LerpUnclamped(start, end, easedT);

            if (hopHeight > 0f)
                position.y += Mathf.Sin(t * Mathf.PI) * hopHeight;

            transform.position = position;
            yield return null;
        }

        transform.position = end;
        gridPosition = targetGridPosition;
        MoveCount++;
        board?.ResolveArrival(gridPosition, keyRing);
        MoveResolved?.Invoke(gridPosition, MoveCount);
    }

    private void OnDisable()
    {
        bufferedMoves.Clear();

        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

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

    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }
}
