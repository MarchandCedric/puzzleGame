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
    private IPlayerAnimationController animationController;

    private void Awake()
    {
        if (board == null)
            board = FindAnyObjectByType<GridBoard>();
        
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

public interface IPlayerAnimationController
{
    void BeginMove(MoveAnimationDirection direction);
    void EndMove();
}

public enum MoveAnimationDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}

public class AnimatorPlayerAnimationController : MonoBehaviour, IPlayerAnimationController
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("General Parameters")]
    [SerializeField] private string movingBoolParameter = "IsMoving";

    [Header("Directional Parameters")]
    [SerializeField] private string walkUpParameter = "";
    [SerializeField] private string walkDownParameter = "";
    [SerializeField] private string walkLeftParameter = "";
    [SerializeField] private string walkRightParameter = "";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void BeginMove(MoveAnimationDirection direction)
    {
        if (animator == null)
            return;

        SetBoolIfConfigured(movingBoolParameter, true);
        SetDirectionalState(direction, true);
    }

    public void EndMove()
    {
        if (animator == null)
            return;

        SetBoolIfConfigured(movingBoolParameter, false);
        ClearDirectionalState();
    }

    private void SetDirectionalState(MoveAnimationDirection direction, bool value)
    {
        ClearDirectionalState();

        switch (direction)
        {
            case MoveAnimationDirection.Up:
                SetBoolIfConfigured(walkUpParameter, value);
                break;
            case MoveAnimationDirection.Down:
                SetBoolIfConfigured(walkDownParameter, value);
                break;
            case MoveAnimationDirection.Left:
                SetBoolIfConfigured(walkLeftParameter, value);
                break;
            case MoveAnimationDirection.Right:
                SetBoolIfConfigured(walkRightParameter, value);
                break;
        }
    }

    private void ClearDirectionalState()
    {
        SetBoolIfConfigured(walkUpParameter, false);
        SetBoolIfConfigured(walkDownParameter, false);
        SetBoolIfConfigured(walkLeftParameter, false);
        SetBoolIfConfigured(walkRightParameter, false);
    }

    private void SetBoolIfConfigured(string parameterName, bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
            return;

        animator.SetBool(parameterName, value);
    }
}
