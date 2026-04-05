using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    [SerializeField] private float phoneAspect = 9f / 16f;   // 0.5625
    [SerializeField] private float tabletAspect = 3f / 4f;   // 0.75

    [SerializeField] private float phoneZ = -750f;
    [SerializeField] private float tabletZ = -620f;

    private void LateUpdate()
    {
        float currentAspect = (float)Screen.width / (float)Screen.height;

        float t = Mathf.InverseLerp(phoneAspect, tabletAspect, currentAspect);
        float z = Mathf.Lerp(phoneZ, tabletZ, t);

        Vector3 position = transform.position;
        position.z = z;
        transform.position = position;
    }
}
