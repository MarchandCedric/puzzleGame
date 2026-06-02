using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    [SerializeField] private float phoneAspect = 9f / 16f;
    [SerializeField] private float tabletAspect = 3f / 4f;

    [SerializeField] private float phoneZ = -750f;
    [SerializeField] private float tabletZ = -640f;

    [SerializeField] private RectTransform rectTransform;

    private void Start()
    {
        ApplyZ();
    }

    private void ApplyZ()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        float aspectRange = tabletAspect - phoneAspect;
        if (Mathf.Approximately(aspectRange, 0f))
        {
            Debug.LogWarning("[CameraFollowZ] phoneAspect and tabletAspect are equal. Using phoneZ.", this);

            Vector3 fallbackPos = rectTransform.localPosition;
            fallbackPos.z = phoneZ;
            rectTransform.localPosition = fallbackPos;
            return;
        }

        float t = (currentAspect - phoneAspect) / aspectRange;
        float z = Mathf.LerpUnclamped(phoneZ, tabletZ, t);

        Vector3 pos = rectTransform.localPosition;
        pos.z = z;
        rectTransform.localPosition = pos;
    }
}
