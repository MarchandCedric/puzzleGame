using System;
using UnityEngine;

public sealed class LocalLevelAttemptGate : MonoBehaviour
{
    [SerializeField] private int maxAttemptsBeforeCooldown = 10;
    [SerializeField] private int cooldownMinutes = 10;

    private LevelAttemptService attemptService;

    public LevelAttemptResult TryConsumeAttempt()
    {
        EnsureInitialized();
        return attemptService.TryConsumeAttempt(DateTimeOffset.UtcNow);
    }

    public LevelAttemptResult Check()
    {
        EnsureInitialized();
        return attemptService.Check(DateTimeOffset.UtcNow);
    }

    public void SetSubscriptionActive(bool isActive)
    {
        EnsureInitialized();
        attemptService.SetSubscriptionActive(isActive);
    }

    public void ResetLocalLimit()
    {
        EnsureInitialized();
        attemptService.ResetLocalLimit();
    }

    private void Awake()
    {
        EnsureInitialized();
    }

    private void EnsureInitialized()
    {
        if (attemptService != null)
            return;

        attemptService = new LevelAttemptService(
            new PlayerPrefsLevelAttemptStore(),
            maxAttemptsBeforeCooldown,
            TimeSpan.FromMinutes(Mathf.Max(1, cooldownMinutes)));
    }
}
