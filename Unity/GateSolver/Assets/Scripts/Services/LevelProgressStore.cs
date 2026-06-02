using UnityEngine;

public static class LevelProgressStore
{
    public static bool IsCompleted(int world, int level)
    {
        return PlayerPrefs.GetInt(GetCompletionKey(world, level), 0) == 1;
    }

    public static int GetBestStars(int world, int level)
    {
        return PlayerPrefs.GetInt(GetStarsKey(world, level), 0);
    }

    public static int GetBestMoves(int world, int level)
    {
        return PlayerPrefs.GetInt(GetMovesKey(world, level), 0);
    }

    public static int GetTotalAttempts(int world, int level)
    {
        return PlayerPrefs.GetInt(GetAttemptsKey(world, level), 0);
    }

    public static void RecordAttempt(int world, int level)
    {
        int attempts = GetTotalAttempts(world, level) + 1;
        PlayerPrefs.SetInt(GetAttemptsKey(world, level), attempts);
        PlayerPrefs.Save();
    }

    public static void SaveBestResult(int world, int level, int moveCount, int stars)
    {
        int existingStars = GetBestStars(world, level);
        int existingMoves = GetBestMoves(world, level);
        int bestStars = Mathf.Max(existingStars, stars);
        int bestMoves = existingMoves > 0 ? Mathf.Min(existingMoves, moveCount) : moveCount;

        PlayerPrefs.SetInt(GetCompletionKey(world, level), 1);
        PlayerPrefs.SetInt(GetStarsKey(world, level), bestStars);
        PlayerPrefs.SetInt(GetMovesKey(world, level), bestMoves);
        PlayerPrefs.Save();
    }

    private static string GetCompletionKey(int world, int level)
    {
        return $"menu.level.{world}-{level}.completed";
    }

    private static string GetStarsKey(int world, int level)
    {
        return $"menu.level.{world}-{level}.stars";
    }

    private static string GetMovesKey(int world, int level)
    {
        return $"menu.level.{world}-{level}.moves";
    }

    private static string GetAttemptsKey(int world, int level)
    {
        return $"menu.level.{world}-{level}.attempts";
    }
}
