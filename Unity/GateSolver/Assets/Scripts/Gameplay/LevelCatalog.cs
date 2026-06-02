using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "PuzzleGame/Level Catalog")]
public sealed class LevelCatalog : ScriptableObject
{
    [SerializeField] private LevelCatalogEntry[] levels = Array.Empty<LevelCatalogEntry>();

    public int Count => levels?.Length ?? 0;
    public LevelCatalogEntry[] Levels => levels;

    public bool TryGetByLevelId(string levelId, out LevelCatalogEntry entry)
    {
        entry = null;
        if (string.IsNullOrWhiteSpace(levelId) || levels == null)
            return false;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate != null && string.Equals(candidate.LevelId, levelId, StringComparison.OrdinalIgnoreCase))
            {
                entry = candidate;
                return true;
            }
        }

        return false;
    }

    public bool TryGetByLevelNumber(int levelNumber, out LevelCatalogEntry entry)
    {
        entry = null;
        if (levels == null)
            return false;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate != null && candidate.LevelNumber == levelNumber)
            {
                entry = candidate;
                return true;
            }
        }

        return false;
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        if (!TryGetByLevelNumber(levelNumber, out LevelCatalogEntry entry) || !entry.IsValid)
            return false;

        LevelCatalogEntry firstLevel = GetFirstLevel();
        if (firstLevel == null)
            return false;

        if (entry.LevelNumber == firstLevel.LevelNumber)
            return true;

        return TryGetPreviousLevel(entry.LevelNumber, out LevelCatalogEntry previousLevel) &&
            LevelProgressStore.GetBestStars(previousLevel.World, previousLevel.Level) >= 1;
    }

    public bool TryGetHighestUnlockedLevel(out LevelCatalogEntry highestUnlockedLevel)
    {
        highestUnlockedLevel = null;
        if (levels == null)
            return false;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate == null || !candidate.IsValid || !IsLevelUnlocked(candidate.LevelNumber))
                continue;

            if (highestUnlockedLevel == null || candidate.LevelNumber > highestUnlockedLevel.LevelNumber)
                highestUnlockedLevel = candidate;
        }

        return highestUnlockedLevel != null;
    }

    public LevelCatalogEntry GetFirstLevel()
    {
        LevelCatalogEntry first = null;
        if (levels == null)
            return null;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate == null || !candidate.IsValid)
                continue;

            if (first == null || candidate.LevelNumber < first.LevelNumber)
                first = candidate;
        }

        return first;
    }

    public bool TryGetNextLevel(string currentSceneName, out LevelCatalogEntry nextLevel)
    {
        nextLevel = null;
        if (string.IsNullOrWhiteSpace(currentSceneName) || levels == null)
            return false;

        LevelCatalogEntry currentLevel = null;
        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate == null || !candidate.IsValid)
                continue;

            if (string.Equals(candidate.SceneName, currentSceneName, StringComparison.OrdinalIgnoreCase))
            {
                currentLevel = candidate;
                break;
            }
        }

        if (currentLevel == null)
            return false;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate == null || !candidate.IsValid || candidate.LevelNumber <= currentLevel.LevelNumber)
                continue;

            if (nextLevel == null || candidate.LevelNumber < nextLevel.LevelNumber)
                nextLevel = candidate;
        }

        return nextLevel != null;
    }

    private bool TryGetPreviousLevel(int levelNumber, out LevelCatalogEntry previousLevel)
    {
        previousLevel = null;
        if (levels == null)
            return false;

        foreach (LevelCatalogEntry candidate in levels)
        {
            if (candidate == null || !candidate.IsValid || candidate.LevelNumber >= levelNumber)
                continue;

            if (previousLevel == null || candidate.LevelNumber > previousLevel.LevelNumber)
                previousLevel = candidate;
        }

        return previousLevel != null;
    }
}
