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
}
