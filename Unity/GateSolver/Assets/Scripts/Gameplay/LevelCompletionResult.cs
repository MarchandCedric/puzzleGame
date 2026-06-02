public readonly struct LevelCompletionResult
{
    public LevelCompletionResult(int world, int level, int moveCount, int stars)
    {
        World = world;
        Level = level;
        MoveCount = moveCount;
        Stars = stars;
    }

    public int World { get; }
    public int Level { get; }
    public int MoveCount { get; }
    public int Stars { get; }
}
