using UnityEngine;

public class GridCollectible : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.35f;
    [SerializeField] private GameObject[] objectsToDisableOnCollect = new GameObject[0];

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
        DisableCollectedVisuals();
    }

    private void DisableCollectedVisuals()
    {
        if (objectsToDisableOnCollect == null || objectsToDisableOnCollect.Length == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        foreach (GameObject objectToDisable in objectsToDisableOnCollect)
        {
            if (objectToDisable != null)
                objectToDisable.SetActive(false);
        }
    }
}
