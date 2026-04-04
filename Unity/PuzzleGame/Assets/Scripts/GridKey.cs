using UnityEngine;

public class GridKey : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private DoorKeyType keyType = DoorKeyType.Red;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.35f;

    public DoorKeyType KeyType => keyType;
    public Vector3Int GridPosition => gridPosition;
    public bool IsCollected { get; private set; }

    private void OnValidate()
    {
        if (board == null)
            board = GetComponentInParent<GridBoard>();

        if (board == null)
            board = FindAnyObjectByType<GridBoard>();

        if (board == null)
            return;

        if (computeGridPositionFromTransform)
            gridPosition = board.WorldToGrid(transform.position, heightOffset);

        if (snapToGridOnValidate)
            transform.position = board.GridToWorld(gridPosition, heightOffset);
    }

    public void Collect()
    {
        if (IsCollected)
            return;

        IsCollected = true;
        gameObject.SetActive(false);
    }
}
