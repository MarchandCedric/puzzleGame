using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCatalogPlayButton : MonoBehaviour
{
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private LocalLevelAttemptGate attemptGate;
    [SerializeField] private LevelSelectionManager levelSelectionManager;

    private LevelAttemptService fallbackAttemptService;

    public void LoadFirstLevel()
    {
        if (levelCatalog == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because no catalog is assigned.");
            return;
        }

        LevelCatalogEntry firstLevel = levelCatalog.GetFirstLevel();
        if (firstLevel == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because the catalog is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(firstLevel.SceneName))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because its scene name is empty.");
            return;
        }

        if (!TryConsumeLevelAttempt())
            return;

        SceneManager.LoadScene(firstLevel.SceneName);
    }

    public void LoadNextLevel()
    {
        if (levelCatalog == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load next level because no catalog is assigned.");
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();
        if (!levelCatalog.TryGetNextLevel(currentScene.name, out LevelCatalogEntry nextLevel))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load next level because '{currentScene.name}' has no next catalog entry.");
            return;
        }

        if (string.IsNullOrWhiteSpace(nextLevel.SceneName))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load next level because its scene name is empty.");
            return;
        }

        if (!TryConsumeLevelAttempt())
            return;

        SceneManager.LoadScene(nextLevel.SceneName);
    }

    public void LoadSelectedLevel()
    {
        if (levelSelectionManager == null)
            levelSelectionManager = FindAnyObjectByType<LevelSelectionManager>();

        if (levelSelectionManager == null || !levelSelectionManager.HasSelectedLevel)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load selected level because no level is selected.");
            return;
        }

        LoadLevelByNumber(levelSelectionManager.SelectedLevelNumber);
    }

    public void LoadLevelByNumber(int levelNumber)
    {
        if (levelCatalog == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load level {levelNumber} because no catalog is assigned.");
            return;
        }

        if (!levelCatalog.TryGetByLevelNumber(levelNumber, out LevelCatalogEntry selectedLevel))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load level {levelNumber} because it is not in the catalog.");
            return;
        }

        if (!levelCatalog.IsLevelUnlocked(levelNumber))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load level {levelNumber} because it is locked.");
            return;
        }

        if (string.IsNullOrWhiteSpace(selectedLevel.SceneName))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load level {levelNumber} because its scene name is empty.");
            return;
        }

        if (!TryConsumeLevelAttempt())
            return;

        SceneManager.LoadScene(selectedLevel.SceneName);
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

        Debug.LogWarning($"Level start blocked. Try again in {FormatRemainingCooldown(result)}.");
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
