using System;
using UnityEngine;

[Serializable]
public sealed class LevelCatalogEntry
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private string levelId = "";
    [SerializeField] private string title = "";
    [SerializeField] private string sceneName = "";
    [SerializeField] private int world = 1;
    [SerializeField] private int level = 1;
    [SerializeField] private int threeStarMoves = 12;
    [SerializeField] private int twoStarMoves = 16;
    [SerializeField] private int oneStarMoves = 22;

    public int LevelNumber => levelNumber;
    public string LevelId => levelId;
    public string Title => !string.IsNullOrWhiteSpace(title) ? title : $"Level {level}";
    public string SceneName => sceneName;
    public int World => world;
    public int Level => level;
    public int ThreeStarMoves => threeStarMoves;
    public int TwoStarMoves => twoStarMoves;
    public int OneStarMoves => oneStarMoves;

    public bool IsValid => levelNumber > 0 && !string.IsNullOrWhiteSpace(levelId) && !string.IsNullOrWhiteSpace(sceneName);

    public int EvaluateStars(int moveCount)
    {
        if (moveCount <= 0)
            return 0;
        if (moveCount <= threeStarMoves)
            return 3;
        if (moveCount <= twoStarMoves)
            return 2;
        if (moveCount <= oneStarMoves)
            return 1;
        return 0;
    }
}
