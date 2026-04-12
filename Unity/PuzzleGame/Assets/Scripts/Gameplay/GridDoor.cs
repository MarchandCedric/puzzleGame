using UnityEngine;

public class GridDoor : MonoBehaviour
{
    [SerializeField] private GridBoard board;
    [SerializeField] private DoorKeyType requiredKeyType = DoorKeyType.Red;
    [SerializeField] private Vector3Int gridPosition = Vector3Int.zero;
    [SerializeField] private bool computeGridPositionFromTransform = true;
    [SerializeField] private bool snapToGridOnValidate = true;
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private bool hideRenderersWhenOpened = true;
    [SerializeField] private bool disableCollidersWhenOpened = true;
    [SerializeField] private bool isOpen = false;

    public DoorKeyType RequiredKeyType => requiredKeyType;
    public Vector3Int GridPosition => gridPosition;
    public bool IsOpen => isOpen;

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

        ApplyOpenState();
    }

    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        ApplyOpenState();
    }

    private void Awake()
    {
        ApplyOpenState();
    }

    private void ApplyOpenState()
    {
        if (hideRenderersWhenOpened)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                renderer.enabled = !isOpen;
        }

        if (disableCollidersWhenOpened)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider collider in colliders)
                collider.enabled = !isOpen;
        }
    }
}
