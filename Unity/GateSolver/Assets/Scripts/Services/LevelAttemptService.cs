using System;
using UnityEngine;

public sealed class LevelAttemptService
{
    private readonly ILevelAttemptStore store;
    private readonly int maxAttemptsBeforeCooldown;
    private readonly TimeSpan cooldownDuration;

    public LevelAttemptService(ILevelAttemptStore store, int maxAttemptsBeforeCooldown, TimeSpan cooldownDuration)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
        this.maxAttemptsBeforeCooldown = Mathf.Max(1, maxAttemptsBeforeCooldown);
        this.cooldownDuration = cooldownDuration <= TimeSpan.Zero ? TimeSpan.FromMinutes(10) : cooldownDuration;
    }

    public LevelAttemptResult TryConsumeAttempt(DateTimeOffset nowUtc)
    {
        Debug.LogWarning($"Trying to consume an attemps.");

        if (store.HasActiveSubscription)
        {
            ResetLocalLimit();
            return new LevelAttemptResult(true, 0, maxAttemptsBeforeCooldown, DateTimeOffset.MinValue);
        }

        DateTimeOffset cooldownEndsAt = ReadCooldownEndsAt();
        if (cooldownEndsAt > nowUtc)
            return new LevelAttemptResult(false, store.AttemptsUsed, maxAttemptsBeforeCooldown, cooldownEndsAt);

        if (cooldownEndsAt != DateTimeOffset.MinValue)
            ResetLocalLimit();

        int attemptsUsed = store.AttemptsUsed + 1;
        if (attemptsUsed >= maxAttemptsBeforeCooldown)
        {
            cooldownEndsAt = nowUtc.Add(cooldownDuration);
            store.AttemptsUsed = 0;
            store.CooldownEndsAtUnixSeconds = cooldownEndsAt.ToUnixTimeSeconds();
            store.Save();
            return new LevelAttemptResult(true, maxAttemptsBeforeCooldown, maxAttemptsBeforeCooldown, cooldownEndsAt);
        }

        store.AttemptsUsed = attemptsUsed;
        store.CooldownEndsAtUnixSeconds = 0;
        store.Save();
        return new LevelAttemptResult(true, attemptsUsed, maxAttemptsBeforeCooldown, DateTimeOffset.MinValue);
    }

    public LevelAttemptResult Check(DateTimeOffset nowUtc)
    {
        if (store.HasActiveSubscription)
            return new LevelAttemptResult(true, 0, maxAttemptsBeforeCooldown, DateTimeOffset.MinValue);

        DateTimeOffset cooldownEndsAt = ReadCooldownEndsAt();
        if (cooldownEndsAt > nowUtc)
            return new LevelAttemptResult(false, store.AttemptsUsed, maxAttemptsBeforeCooldown, cooldownEndsAt);

        return new LevelAttemptResult(true, store.AttemptsUsed, maxAttemptsBeforeCooldown, DateTimeOffset.MinValue);
    }

    public void SetSubscriptionActive(bool isActive)
    {
        store.HasActiveSubscription = isActive;

        if (isActive)
            ResetLocalLimit();
        else
            store.Save();
    }

    public void ResetLocalLimit()
    {
        store.AttemptsUsed = 0;
        store.CooldownEndsAtUnixSeconds = 0;
        store.Save();
    }

    private DateTimeOffset ReadCooldownEndsAt()
    {
        long unixSeconds = store.CooldownEndsAtUnixSeconds;
        if (unixSeconds <= 0)
            return DateTimeOffset.MinValue;

        return DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
    }
}
