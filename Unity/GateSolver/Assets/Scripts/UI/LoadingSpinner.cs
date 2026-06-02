using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private float degreesPerSecond = -180f;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;
    }

    private void Update()
    {
        if (target == null)
            return;

        target.Rotate(0f, 0f, degreesPerSecond * Time.unscaledDeltaTime);
    }
}
