using UnityEngine;

public class CollectibleMotion : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public enum MotionType { Translation, Rotation }

    [Header("Motion Settings")]
    public MotionType motionType = MotionType.Translation;
    public Axis axis = Axis.Y;

    public float amplitude = 0.15f;   // distance (translation) or angle (rotation)
    public float frequency = 2f;

    private Vector3 _startPos;
    private Quaternion _startRot;

    void Start()
    {
        _startPos = transform.localPosition;
        _startRot = transform.localRotation;
    }

    void Update()
    {
        float value = Mathf.Sin(Time.time * frequency) * amplitude;

        Vector3 axisVector = GetAxisVector();

        if (motionType == MotionType.Translation)
        {
            transform.localPosition = _startPos + axisVector * value;
        }
        else if (motionType == MotionType.Rotation)
        {
            transform.localRotation = _startRot * Quaternion.AngleAxis(value, axisVector);
        }
    }

    private Vector3 GetAxisVector()
    {
        switch (axis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default: return Vector3.up;
        }
    }
}