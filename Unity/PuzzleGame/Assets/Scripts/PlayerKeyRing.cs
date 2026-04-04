using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyRing : MonoBehaviour
{
    [SerializeField] private List<DoorKeyType> startingKeys = new List<DoorKeyType>();
    [Header("Debug")]
    [SerializeField] private List<DoorKeyType> debugHeldKeys = new List<DoorKeyType>();

    private readonly HashSet<DoorKeyType> keys = new HashSet<DoorKeyType>();

    private void Awake()
    {
        foreach (DoorKeyType key in startingKeys)
            keys.Add(key);

        SyncDebugState();
    }

    public bool HasKey(DoorKeyType keyType)
    {
        return keys.Contains(keyType);
    }

    public void AddKey(DoorKeyType keyType)
    {
        keys.Add(keyType);
        SyncDebugState();
    }

    public bool TryConsumeKey(DoorKeyType keyType)
    {
        bool removed = keys.Remove(keyType);

        if (removed)
            SyncDebugState();

        return removed;
    }

    private void SyncDebugState()
    {
        debugHeldKeys.Clear();
        foreach (DoorKeyType key in keys)
            debugHeldKeys.Add(key);
    }
}
