using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSceneFlowController : MonoBehaviour
{
    [SerializeField] private LevelSceneMetadata metadata;
    [SerializeField] private GridMover mover;
    [SerializeField] private LevelExit levelExit;
    [SerializeField] private PortalPulse portalVisual;
    [SerializeField] private GameObject completionUiRoot = null;
    [SerializeField] private bool disableMoverOnComplete = true;
    [Header("Completion Presentation")]
    [SerializeField] private float completionRevealDelay = 0.75f;
    [SerializeField] private PortalCompletionEffect completionEffect = null;
    [SerializeField] private Animator completionAnimation = null;
    [SerializeField] private string completionAnimationTrigger = "Play";
    [SerializeField] private bool fitCompletionAnimationToDelay = true;
    [SerializeField] private bool searchWholeSceneForCollectibles = true;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = string.Empty;

    private bool hasCompleted;
    private Coroutine completionSequence;
    private GridCollectible[] collectibles = Array.Empty<GridCollectible>();
    private LevelObjectiveState objectiveState;
    private LevelCompletionResult lastCompletionResult;

    public int RequiredCollectibles => objectiveState?.RequiredCollectibles ?? 0;
    public int CollectedCollectibles => objectiveState?.CollectedCollectibles ?? 0;
    public bool IsPortalActive => objectiveState?.IsPortalActive ?? false;
    public bool HasCompleted => hasCompleted;
    public bool HasNextLevel => !string.IsNullOrWhiteSpace(nextLevelSceneName);
    public LevelCompletionResult LastCompletionResult => lastCompletionResult;
    public event Action<int, int> CollectiblesChanged;
    public event Action PortalActivated;
    public event Action<LevelCompletionResult> LevelCompleted;

    private void Awake()
    {
        if (metadata == null)
            metadata = FindAnyObjectByType<LevelSceneMetadata>();

        if (mover == null)
            mover = FindAnyObjectByType<GridMover>();

        if (levelExit == null)
            levelExit = FindAnyObjectByType<LevelExit>();

        if (portalVisual == null)
            portalVisual = FindAnyObjectByType<PortalPulse>();

        if (completionEffect == null)
            completionEffect = FindAnyObjectByType<PortalCompletionEffect>();

        collectibles = FindCollectibles();
        int requiredCollectibles = ResolveRequiredCollectibles();
        objectiveState = new LevelObjectiveState(requiredCollectibles);
        objectiveState.Changed += HandleObjectiveChanged;
        objectiveState.PortalActivated += HandlePortalActivated;
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

        if (objectiveState != null)
        {
            objectiveState.Changed -= HandleObjectiveChanged;
            objectiveState.PortalActivated -= HandlePortalActivated;
        }
    }

    private void Start()
    {
        if (completionUiRoot != null)
            completionUiRoot.SetActive(false);

        ApplyPortalState();
        RaiseCollectiblesChanged();
    }

    private void HandleMoveResolved(Vector3Int cell, int moveCount)
    {
        if (hasCompleted)
            return;

        ResolveCollectibleAt(cell);

        if (levelExit == null || cell != levelExit.GridPosition || !IsPortalActive)
            return;

        BeginCompleteLevel(moveCount);
    }

    private void ResolveCollectibleAt(Vector3Int cell)
    {
        GridCollectible collectible = FindCollectibleAt(cell);
        if (collectible == null)
            return;

        if (objectiveState == null || !objectiveState.CollectOne())
            return;

        collectible.Collect();
    }

    private int ResolveRequiredCollectibles()
    {
        if (metadata != null && metadata.RequiredCollectibles >= 0)
            return metadata.RequiredCollectibles;

        int count = 0;
        foreach (GridCollectible collectible in collectibles)
        {
            if (collectible != null && !collectible.IsCollected)
                count++;
        }

        return count;
    }

    private GridCollectible FindCollectibleAt(Vector3Int cell)
    {
        foreach (GridCollectible collectible in collectibles)
        {
            if (collectible != null && !collectible.IsCollected && collectible.GridPosition == cell)
                return collectible;
        }

        return null;
    }

    private GridCollectible[] FindCollectibles()
    {
        if (searchWholeSceneForCollectibles)
            return FindObjectsByType<GridCollectible>(FindObjectsInactive.Include);

        return GetComponentsInChildren<GridCollectible>(true);
    }

    private void HandleObjectiveChanged(LevelObjectiveState state)
    {
        RaiseCollectiblesChanged();
    }

    private void HandlePortalActivated()
    {
        ApplyPortalState();
        PortalActivated?.Invoke();
    }

    private void ApplyPortalState()
    {
        if (portalVisual != null)
            portalVisual.SetPortalActive(IsPortalActive);

        if (levelExit != null)
            levelExit.SetReady(IsPortalActive);
    }

    private void RaiseCollectiblesChanged()
    {
        CollectiblesChanged?.Invoke(CollectedCollectibles, RequiredCollectibles);
    }

    private void BeginCompleteLevel(int moveCount)
    {
        if (hasCompleted || metadata == null)
            return;

        hasCompleted = true;
        int stars = EvaluateStars(moveCount);
        lastCompletionResult = new LevelCompletionResult(metadata.World, metadata.Level, moveCount, stars);
        SaveResult(moveCount, stars);

        if (disableMoverOnComplete && mover != null)
            mover.enabled = false;

        PlayCompletionAnimation();
        PlayCompletionEffect();

        if (completionSequence != null)
            StopCoroutine(completionSequence);

        completionSequence = StartCoroutine(ShowCompletionAfterDelay());
    }

    private IEnumerator ShowCompletionAfterDelay()
    {
        if (completionRevealDelay > 0f)
            yield return new WaitForSeconds(completionRevealDelay);

        if (completionAnimation != null && fitCompletionAnimationToDelay)
            completionAnimation.speed = 1f;

        if (completionUiRoot != null)
            completionUiRoot.SetActive(true);

        LevelCompleted?.Invoke(lastCompletionResult);
        completionSequence = null;
    }

    private void PlayCompletionAnimation()
    {
        if (completionAnimation == null)
            return;

        if (fitCompletionAnimationToDelay && completionRevealDelay > 0f)
        {
            float clipLength = GetFirstAnimationClipLength(completionAnimation);
            if (clipLength > 0f)
                completionAnimation.speed = clipLength / completionRevealDelay;
        }

        if (!string.IsNullOrWhiteSpace(completionAnimationTrigger))
            completionAnimation.SetTrigger(completionAnimationTrigger);
    }

    private void PlayCompletionEffect()
    {
        if (completionEffect == null || mover == null)
            return;

        completionEffect.Play(mover.transform, completionRevealDelay);
    }

    private static float GetFirstAnimationClipLength(Animator animator)
    {
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null || controller.animationClips == null || controller.animationClips.Length == 0)
            return 0f;

        AnimationClip clip = controller.animationClips[0];
        return clip != null ? clip.length : 0f;
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

    public void LoadNextLevel()
    {
        if (!string.IsNullOrWhiteSpace(nextLevelSceneName))
            SceneManager.LoadScene(nextLevelSceneName);
    }

    public void ReturnToMenu()
    {
        if (!string.IsNullOrWhiteSpace(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }
}
