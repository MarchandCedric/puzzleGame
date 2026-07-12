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
    [SerializeField] private ResultStarsController starsController = null;
    [SerializeField] private bool disableMoverOnComplete = true;
    [Header("Completion Presentation")]
    [SerializeField] private float completionRevealDelay = 0.75f;
    [SerializeField] private float completionEffectDelay = 0.5f;

    [SerializeField] private float beforeCompletionAnimationDelay = 0.5f;
[SerializeField] private float afterCompletionAnimationDelay = 0.75f;
    [SerializeField] private PortalCompletionEffect completionEffect = null;
    [SerializeField] private Animator completionAnimation = null;
    [SerializeField] private string completionAnimationTrigger = "Play";
    [SerializeField] private bool fitCompletionAnimationToDelay = true;
    [SerializeField] private bool searchWholeSceneForCollectibles = true;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = string.Empty;
    [SerializeField] private LocalLevelAttemptGate attemptGate;

    private bool hasCompleted;
    private bool hasRecordedAttempt;
    private Coroutine completionSequence;
    private GridCollectible[] collectibles = Array.Empty<GridCollectible>();
    private LevelObjectiveState objectiveState;
    private LevelCompletionResult lastCompletionResult;
    private LevelAttemptService fallbackAttemptService;

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

        if (starsController == null && completionUiRoot != null)
            starsController = completionUiRoot.GetComponentInChildren<ResultStarsController>(true);

        if (starsController == null)
            starsController = FindAnyObjectByType<ResultStarsController>(FindObjectsInactive.Include);

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
        RecordLevelAttempt();

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

    if (completionSequence != null)
        StopCoroutine(completionSequence);

    completionSequence = StartCoroutine(CompleteLevelSequence());
}

    private IEnumerator CompleteLevelSequence()
{

    
    if (!string.IsNullOrWhiteSpace(completionAnimationTrigger))
                completionAnimation.SetTrigger(completionAnimationTrigger);

    if (beforeCompletionAnimationDelay > 0f)
        yield return new WaitForSeconds(beforeCompletionAnimationDelay);

    PlayCompletionAnimation();

    if (afterCompletionAnimationDelay > 0f)
        yield return new WaitForSeconds(afterCompletionAnimationDelay);

    PlayCompletionEffect();

    if (completionRevealDelay > 0f)
        yield return new WaitForSeconds(completionRevealDelay);

    if (completionUiRoot != null)
        completionUiRoot.SetActive(true);

    if (starsController != null)
        starsController.ShowStars(lastCompletionResult.Stars);

    LevelCompleted?.Invoke(lastCompletionResult);
    completionSequence = null;
}

    private IEnumerator ShowCompletionAfterDelay()
    {
        if (completionEffectDelay  > 0f)
            yield return new WaitForSeconds(completionEffectDelay );

        if (completionAnimation != null && fitCompletionAnimationToDelay)
            completionAnimation.speed = 1f;

        if (completionUiRoot != null)
            completionUiRoot.SetActive(true);

        if (starsController != null)
            starsController.ShowStars(lastCompletionResult.Stars);

        LevelCompleted?.Invoke(lastCompletionResult);
        completionSequence = null;
    }

    private void PlayCompletionAnimation()
    {
        if (completionAnimation == null)
            return;

        if (fitCompletionAnimationToDelay && completionEffectDelay  > 0f)
        {
            float clipLength = GetFirstAnimationClipLength(completionAnimation);
            if (clipLength > 0f)
                completionAnimation.speed = clipLength / completionEffectDelay ;
        }

    }

    private void PlayCompletionEffect()
    {
        if (completionEffect == null || mover == null)
            return;

        completionEffect.Play(mover.transform, completionEffectDelay );
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
        LevelProgressStore.SaveBestResult(metadata.World, metadata.Level, moveCount, stars);
    }

    private void RecordLevelAttempt()
    {
        if (hasRecordedAttempt || metadata == null)
            return;

        hasRecordedAttempt = true;
        LevelProgressStore.RecordAttempt(metadata.World, metadata.Level);
    }

    public void LoadNextLevel()
    {
        if (string.IsNullOrWhiteSpace(nextLevelSceneName))
            return;

        if (!TryConsumeLevelAttempt())
            return;

        SceneManager.LoadScene(nextLevelSceneName);
    }

    public void ReturnToMenu()
    {
        if (!string.IsNullOrWhiteSpace(menuSceneName))
            SceneManager.LoadScene(menuSceneName);
    }

    private bool TryConsumeLevelAttempt()
    {
        if (attemptGate == null)
            attemptGate = GetComponent<LocalLevelAttemptGate>();

        LevelAttemptResult result = attemptGate != null
            ? attemptGate.TryConsumeAttempt()
            : GetFallbackAttemptService().TryConsumeAttempt(DateTimeOffset.UtcNow);

        if (result.IsAllowed)
            return true;

        Debug.LogWarning($"Next level blocked. Try again in {FormatRemainingCooldown(result)}.");
        return false;
    }

    private static string FormatRemainingCooldown(LevelAttemptResult result)
    {
        TimeSpan remaining = result.GetRemainingCooldown(DateTimeOffset.UtcNow);
        return $"{Mathf.CeilToInt((float)remaining.TotalMinutes)} minute(s)";
    }

    private LevelAttemptService GetFallbackAttemptService()
    {
        if (fallbackAttemptService == null)
            fallbackAttemptService = new LevelAttemptService(
                new PlayerPrefsLevelAttemptStore(),
                10,
                TimeSpan.FromMinutes(10));

        return fallbackAttemptService;
    }
}
