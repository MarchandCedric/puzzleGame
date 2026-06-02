using UnityEngine;

public class LevelSceneMetadata : MonoBehaviour
{
    [SerializeField] private int world = 1;
    [SerializeField] private int level = 1;
    [SerializeField] private int requiredCollectibles = -1;
    [SerializeField] private int threeStarMoves = 12;
    [SerializeField] private int twoStarMoves = 16;
    [SerializeField] private int oneStarMoves = 22;

    public int World => world;
    public int Level => level;
    public int RequiredCollectibles => requiredCollectibles;
    public int ThreeStarMoves => threeStarMoves;
    public int TwoStarMoves => twoStarMoves;
    public int OneStarMoves => oneStarMoves;
}
