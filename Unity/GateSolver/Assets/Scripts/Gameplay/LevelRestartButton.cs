using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRestartButton : MonoBehaviour
{
    [SerializeField] private LocalLevelAttemptGate attemptGate;

    private LevelAttemptService fallbackAttemptService;

    public void RestartCurrentLevel()
    {
        if (!TryConsumeLevelAttempt())
            return;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
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

        Debug.LogWarning($"Level restart blocked. Try again in {FormatRemainingCooldown(result)}.");
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
