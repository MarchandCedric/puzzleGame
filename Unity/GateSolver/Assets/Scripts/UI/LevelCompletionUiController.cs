using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompletionUiController : MonoBehaviour
{
    [SerializeField] private LevelSceneFlowController levelFlow = null;
    [SerializeField] private GameObject root = null;
    [SerializeField] private TMP_Text titleText = null;
    [SerializeField] private ResultStarsController starsController = null;
    [Header("Result Messages")]
    [SerializeField] private string zeroStarTitle = "Completed!";
    [SerializeField] private string oneStarTitle = "Good Job!";
    [SerializeField] private string twoStarTitle = "Nice!";
    [SerializeField] private string threeStarTitle = "Perfect!";
    [Header("Title Reveal")]
    [SerializeField] private float titlePopDuration = 0.28f;
    [SerializeField] private float titlePopOvershoot = 1.15f;
    [SerializeField] private TMP_Text levelText = null;
    [SerializeField] private TMP_Text movesText = null;
    [SerializeField] private TMP_Text starsText = null;
    [SerializeField] private GameObject[] earnedStarObjects = new GameObject[0];
    [SerializeField] private Button nextLevelButton = null;
    [SerializeField] private Button menuButton = null;
    [SerializeField] private Button retryButton = null;

    private Coroutine titleRevealRoutine;

    private void Awake()
    {
        if (levelFlow == null)
            levelFlow = FindAnyObjectByType<LevelSceneFlowController>();

        if (root == null)
            root = gameObject;

        if (starsController == null)
            starsController = root.GetComponentInChildren<ResultStarsController>(true);

        ValidateTextBindings();

        if (levelFlow != null)
            levelFlow.LevelCompleted += HandleLevelCompleted;

        if (starsController != null)
            starsController.AnimationCompleted += HandleStarsAnimationCompleted;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLevel);

        if (levelFlow != null && levelFlow.HasCompleted)
            HandleLevelCompleted(levelFlow.LastCompletionResult);
        else
            root.SetActive(false);
    }

    private void ValidateTextBindings()
    {
        WarnIfSameText(titleText, levelText, nameof(titleText), nameof(levelText));
        WarnIfSameText(titleText, movesText, nameof(titleText), nameof(movesText));
        WarnIfSameText(titleText, starsText, nameof(titleText), nameof(starsText));
    }

    private void WarnIfSameText(TMP_Text first, TMP_Text second, string firstName, string secondName)
    {
        if (first == null || second == null || first != second)
            return;

        Debug.LogWarning(
            $"{nameof(LevelCompletionUiController)} on {name} has {firstName} and {secondName} bound to the same TMP_Text. " +
            "Use separate text objects so completion values do not overwrite the title.",
            this);
    }

    private void OnDestroy()
    {
        if (levelFlow != null)
            levelFlow.LevelCompleted -= HandleLevelCompleted;

        if (starsController != null)
            starsController.AnimationCompleted -= HandleStarsAnimationCompleted;

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(LoadNextLevel);

        if (menuButton != null)
            menuButton.onClick.RemoveListener(ReturnToMenu);

        if (retryButton != null)
            retryButton.onClick.RemoveListener(RetryLevel);
    }

    private void HandleLevelCompleted(LevelCompletionResult result)
    {
        root.SetActive(true);

        if (titleText != null)
        {
            titleText.text = ResolveTitle(result.Stars);
            HideTitle();
        }

        if (levelText != null)
            levelText.text = $"Level {result.World}-{result.Level}";

        if (movesText != null)
            movesText.text = $"Moves {result.MoveCount}";

        if (starsText != null)
            starsText.text = $"{result.Stars}/3";

        for (int i = 0; i < earnedStarObjects.Length; i++)
        {
            if (earnedStarObjects[i] != null)
                earnedStarObjects[i].SetActive(i < result.Stars);
        }

        if (nextLevelButton != null)
            nextLevelButton.interactable = levelFlow != null && levelFlow.HasNextLevel;

        if (starsController == null || starsController.HasCompletedAnimation)
            RevealTitle();
    }

    private void HandleStarsAnimationCompleted()
    {
        RevealTitle();
    }

    private void HideTitle()
    {
        titleText.alpha = 0f;
        titleText.transform.localScale = Vector3.zero;
    }

    private void RevealTitle()
    {
        if (titleText == null)
            return;

        if (titleRevealRoutine != null)
            StopCoroutine(titleRevealRoutine);

        titleRevealRoutine = StartCoroutine(AnimateTitlePop());
    }

    private IEnumerator AnimateTitlePop()
    {
        float duration = Mathf.Max(0.01f, titlePopDuration);
        float growDuration = duration * 0.65f;
        float settleDuration = duration - growDuration;

        titleText.alpha = 1f;

        float t = 0f;
        while (t < growDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / growDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            titleText.transform.localScale = Vector3.one * Mathf.Lerp(0f, titlePopOvershoot, eased);
            yield return null;
        }

        t = 0f;
        while (t < settleDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / settleDuration);
            float eased = 1f - Mathf.Pow(1f - p, 2f);

            titleText.transform.localScale = Vector3.one * Mathf.Lerp(titlePopOvershoot, 1f, eased);
            yield return null;
        }

        titleText.transform.localScale = Vector3.one;
        titleText.alpha = 1f;
        titleRevealRoutine = null;
    }

    private string ResolveTitle(int stars)
    {
        if (stars >= 3)
            return threeStarTitle;
        if (stars == 2)
            return twoStarTitle;
        if (stars == 1)
            return oneStarTitle;

        return zeroStarTitle;
    }

    private void LoadNextLevel()
    {
        if (levelFlow != null)
            levelFlow.LoadNextLevel();
    }

    private void ReturnToMenu()
    {
        if (levelFlow != null)
            levelFlow.ReturnToMenu();
    }

    private static void RetryLevel()
    {
        LevelRestartButton restartButton = FindAnyObjectByType<LevelRestartButton>();
        if (restartButton != null)
        {
            restartButton.RestartCurrentLevel();
            return;
        }

        var fallbackAttemptService = new LevelAttemptService(
            new PlayerPrefsLevelAttemptStore(),
            10,
            TimeSpan.FromMinutes(10));

        LevelAttemptResult result = fallbackAttemptService.TryConsumeAttempt(DateTimeOffset.UtcNow);
        if (!result.IsAllowed)
        {
            Debug.LogWarning($"Level retry blocked. Try again in {FormatRemainingCooldown(result)}.");
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private static string FormatRemainingCooldown(LevelAttemptResult result)
    {
        TimeSpan remaining = result.GetRemainingCooldown(DateTimeOffset.UtcNow);
        return $"{Mathf.CeilToInt((float)remaining.TotalMinutes)} minute(s)";
    }
}
