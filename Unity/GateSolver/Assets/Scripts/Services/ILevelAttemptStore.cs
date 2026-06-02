public interface ILevelAttemptStore
{
    int AttemptsUsed { get; set; }
    long CooldownEndsAtUnixSeconds { get; set; }
    bool HasActiveSubscription { get; set; }
    void Save();
}
