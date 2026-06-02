using UnityEngine;

public sealed class PlayerPrefsLevelAttemptStore : ILevelAttemptStore
{
    private const string AttemptsUsedKey = "level_attempts.used";
    private const string CooldownEndsAtKey = "level_attempts.cooldown_ends_at";
    private const string HasActiveSubscriptionKey = "level_attempts.has_active_subscription";

    public int AttemptsUsed
    {
        get => PlayerPrefs.GetInt(AttemptsUsedKey, 0);
        set => PlayerPrefs.SetInt(AttemptsUsedKey, Mathf.Max(0, value));
    }

    public long CooldownEndsAtUnixSeconds
    {
        get
        {
            string value = PlayerPrefs.GetString(CooldownEndsAtKey, "0");
            return long.TryParse(value, out long parsedValue) ? parsedValue : 0;
        }
        set => PlayerPrefs.SetString(CooldownEndsAtKey, value.ToString());
    }

    public bool HasActiveSubscription
    {
        get => PlayerPrefs.GetInt(HasActiveSubscriptionKey, 0) == 1;
        set => PlayerPrefs.SetInt(HasActiveSubscriptionKey, value ? 1 : 0);
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}
