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
        if (board != null)
            return board.IsWalkable(targetGridPosition, keyRing);

        return true;
    }

    private bool TryResolveTargetCell(Vector3Int targetGridPosition)
    {
        if (board == null)
            return true;

        return board.TryUnlockDoor(targetGridPosition, keyRing);
    }

    private void TryStartMove(Vector3Int direction)
    {
        if (isMoving || direction == Vector3Int.zero)
            return;

        Vector3Int targetGridPosition = gridPosition + direction;
        if (!CanMoveTo(targetGridPosition))
            return;

        if (!TryResolveTargetCell(targetGridPosition))
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

public class LevelSceneMetadata : MonoBehaviour
{
    [SerializeField] private int world = 1;
    [SerializeField] private int level = 1;
    [SerializeField] private int threeStarMoves = 12;
    [SerializeField] private int twoStarMoves = 16;
    [SerializeField] private int oneStarMoves = 22;

    public int World => world;
    public int Level => level;
    public int ThreeStarMoves => threeStarMoves;
    public int TwoStarMoves => twoStarMoves;
    public int OneStarMoves => oneStarMoves;
}

public class LevelExit : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.35f;

    public Vector3Int GridPosition => gridPosition;

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

        if (snapToGridOnValidate)
            transform.position = board.GridToWorld(gridPosition, heightOffset);
    }
}

public class LevelSceneFlowController : MonoBehaviour
{
    [SerializeField] private LevelSceneMetadata metadata;
    [SerializeField] private GridMover mover;
    [SerializeField] private LevelExit levelExit;
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool hasCompleted;

    private void Awake()
    {
        if (metadata == null)
            metadata = FindAnyObjectByType<LevelSceneMetadata>();

        if (mover == null)
            mover = FindAnyObjectByType<GridMover>();

        if (levelExit == null)
            levelExit = FindAnyObjectByType<LevelExit>();
    }

    private void OnEnable()
    {
        if (mover != null)
            mover.MoveResolved += HandleMoveResolved;
    }

    private void OnDisable()
    {
        if (mover != null)
            mover.MoveResolved -= HandleMoveResolved;
    }

    private void HandleMoveResolved(Vector3Int cell, int moveCount)
    {
        if (hasCompleted || levelExit == null || cell != levelExit.GridPosition)
            return;

        CompleteLevel(moveCount);
    }

    private void CompleteLevel(int moveCount)
    {
        if (hasCompleted || metadata == null)
            return;

        hasCompleted = true;
        int stars = EvaluateStars(moveCount);
        SaveResult(moveCount, stars);
        StartCoroutine(ReturnToMenuAfterDelay(0.5f));
    }

    private int EvaluateStars(int moveCount)
    {
        if (moveCount <= metadata.ThreeStarMoves)
            return 3;
        if (moveCount <= metadata.TwoStarMoves)
            return 2;
        if (moveCount <= metadata.OneStarMoves)
            return 1;
        return 0;
    }

    private void SaveResult(int moveCount, int stars)
    {
        string levelId = $"{metadata.World}-{metadata.Level}";
        string completionKey = $"menu.level.{levelId}.completed";
        string starsKey = $"menu.level.{levelId}.stars";
        string movesKey = $"menu.level.{levelId}.moves";

        int existingStars = PlayerPrefs.GetInt(starsKey, 0);
        int existingMoves = PlayerPrefs.GetInt(movesKey, 0);
        int bestStars = Mathf.Max(existingStars, stars);
        int bestMoves = existingMoves > 0 ? Mathf.Min(existingMoves, moveCount) : moveCount;

        PlayerPrefs.SetInt(completionKey, 1);
        PlayerPrefs.SetInt(starsKey, bestStars);
        PlayerPrefs.SetInt(movesKey, bestMoves);
        PlayerPrefs.Save();
    }

    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnToMenu();
    }

    public void ReturnToMenu()
    {
        if (!string.IsNullOrWhiteSpace(menuSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }
}
