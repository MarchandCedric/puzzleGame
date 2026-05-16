using UnityEngine;

public class AnimatorPlayerAnimationController : MonoBehaviour, IPlayerAnimationController
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform visualRoot;

    [Header("General Parameters")]
    [SerializeField] private string movingBoolParameter = "Walk_Anim";

    [Header("Facing")]
    [SerializeField] private float upYaw = 45f;
    [SerializeField] private float downYaw = 225f;
    [SerializeField] private float leftYaw = 315f;
    [SerializeField] private float rightYaw = 135f;

    [Header("Directional Parameters")]
    [SerializeField] private string walkUpParameter = "";
    [SerializeField] private string walkDownParameter = "";
    [SerializeField] private string walkLeftParameter = "";
    [SerializeField] private string walkRightParameter = "";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualRoot == null)
            visualRoot = transform;
    }

    public void BeginMove(MoveAnimationDirection direction)
    {
        if (animator == null)
            return;

        FaceDirection(direction);
        SetBoolIfConfigured(movingBoolParameter, true);
        SetDirectionalState(direction, true);
    }

    public void EndMove()
    {
        if (animator == null)
            return;

        SetBoolIfConfigured(movingBoolParameter, false);
        ClearDirectionalState();
    }

    private void SetDirectionalState(MoveAnimationDirection direction, bool value)
    {
        ClearDirectionalState();

        switch (direction)
        {
            case MoveAnimationDirection.Up:
                SetBoolIfConfigured(walkUpParameter, value);
                break;
            case MoveAnimationDirection.Down:
                SetBoolIfConfigured(walkDownParameter, value);
                break;
            case MoveAnimationDirection.Left:
                SetBoolIfConfigured(walkLeftParameter, value);
                break;
            case MoveAnimationDirection.Right:
                SetBoolIfConfigured(walkRightParameter, value);
                break;
        }
    }

    private void ClearDirectionalState()
    {
        SetBoolIfConfigured(walkUpParameter, false);
        SetBoolIfConfigured(walkDownParameter, false);
        SetBoolIfConfigured(walkLeftParameter, false);
        SetBoolIfConfigured(walkRightParameter, false);
    }

    private void FaceDirection(MoveAnimationDirection direction)
    {
        if (visualRoot == null)
            return;

        float yaw;
        switch (direction)
        {
            case MoveAnimationDirection.Up:
                yaw = upYaw;
                break;
            case MoveAnimationDirection.Down:
                yaw = downYaw;
                break;
            case MoveAnimationDirection.Left:
                yaw = leftYaw;
                break;
            case MoveAnimationDirection.Right:
                yaw = rightYaw;
                break;
            default:
                return;
        }

        Vector3 eulerAngles = visualRoot.localEulerAngles;
        visualRoot.localEulerAngles = new Vector3(eulerAngles.x, yaw, eulerAngles.z);
    }

    private void SetBoolIfConfigured(string parameterName, bool value)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
            return;

        animator.SetBool(parameterName, value);
    }
}
