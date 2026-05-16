public interface IPlayerAnimationController
{
    void BeginMove(MoveAnimationDirection direction);
    void EndMove();
}

public enum MoveAnimationDirection
{
    None = 0,
    Up = 1,
    Down = 2,
    Left = 3,
    Right = 4
}
