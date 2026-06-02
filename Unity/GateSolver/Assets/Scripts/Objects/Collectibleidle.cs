using UnityEngine;

public class CollectibleIdle : MonoBehaviour
{
    public float bobAmplitude = 0.12f;
    public float bobFrequency = 2f;

    public float tiltAngle = 8f;
    public float tiltFrequency = 1.5f;

    private Vector3 _startPos;
    private Quaternion _startRot;

    void Start()
    {
        _startPos = transform.localPosition;
        _startRot = transform.localRotation;
    }

    void Update()
    {
        float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        float tilt = Mathf.Sin(Time.time * tiltFrequency) * tiltAngle;

        transform.localPosition = _startPos + new Vector3(0, bob, 0);
        transform.localRotation = _startRot * Quaternion.Euler(0, 0, tilt);
    }
}