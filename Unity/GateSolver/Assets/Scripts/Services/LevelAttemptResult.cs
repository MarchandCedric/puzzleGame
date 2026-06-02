using System;

public readonly struct LevelAttemptResult
{
    public LevelAttemptResult(bool isAllowed, int attemptsUsed, int maxAttempts, DateTimeOffset cooldownEndsAtUtc)
    {
        IsAllowed = isAllowed;
        AttemptsUsed = attemptsUsed;
        MaxAttempts = maxAttempts;
        CooldownEndsAtUtc = cooldownEndsAtUtc;
    }

    public bool IsAllowed { get; }
    public int AttemptsUsed { get; }
    public int MaxAttempts { get; }
    public DateTimeOffset CooldownEndsAtUtc { get; }
    public bool IsBlocked => !IsAllowed;

    public TimeSpan GetRemainingCooldown(DateTimeOffset nowUtc)
    {
        if (CooldownEndsAtUtc <= nowUtc)
            return TimeSpan.Zero;

        return CooldownEndsAtUtc - nowUtc;
    }
}
