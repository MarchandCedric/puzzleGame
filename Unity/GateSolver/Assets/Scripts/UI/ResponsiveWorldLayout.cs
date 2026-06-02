using UnityEngine;

public class ResponsiveWorldLayout : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Transform robot;
    [SerializeField] private SpriteRenderer background;

    [Header("Robot")]
    [SerializeField] private Vector2 robotViewportPos = new Vector2(0.5f, 0.28f);
    [SerializeField] private float baseRobotScale = 1f;
    [SerializeField] private float referenceAspect = 1080f / 1920f; // portrait ref
    [SerializeField] private float minScaleFactor = 0.85f;
    [SerializeField] private float maxScaleFactor = 1.2f;

    private int lastW, lastH;

    private void Start() => ApplyLayout();

    private void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
            ApplyLayout();
    }

    private void ApplyLayout()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        // 1) Robot position from viewport
        float dist = Mathf.Abs(cam.transform.position.z - robot.position.z);
        Vector3 wp = cam.ViewportToWorldPoint(new Vector3(robotViewportPos.x, robotViewportPos.y, dist));
        robot.position = new Vector3(wp.x, wp.y, robot.position.z);

        // 2) Robot scale from aspect
        float factor = Mathf.Clamp(cam.aspect / referenceAspect, minScaleFactor, maxScaleFactor);
        robot.localScale = Vector3.one * (baseRobotScale * factor);

        // 3) Background cover camera
        if (background != null && background.sprite != null && cam.orthographic)
        {
            float worldH = cam.orthographicSize * 2f;
            float worldW = worldH * cam.aspect;

            Vector2 s = background.sprite.bounds.size; // at scale 1
            float scale = Mathf.Max(worldW / s.x, worldH / s.y);

            background.transform.localScale = new Vector3(scale, scale, 1f);
            background.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, background.transform.position.z);
        }
    }
}
