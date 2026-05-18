using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.35f;
    [SerializeField] private GameObject[] objectsToActivateWhenReady = new GameObject[0];
    [SerializeField] private GameObject[] objectsToDeactivateWhenReady = new GameObject[0];

    public Vector3Int GridPosition => gridPosition;
    public bool IsReady { get; private set; }

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

    private void Awake()
    {
        SetReady(false);
    }

    public void SetReady(bool ready)
    {
        IsReady = ready;
        SetObjectsActive(objectsToActivateWhenReady, ready);
        SetObjectsActive(objectsToDeactivateWhenReady, !ready);
    }

    private static void SetObjectsActive(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (GameObject objectToSet in objects)
        {
            if (objectToSet != null)
                objectToSet.SetActive(active);
        }
    }
}
