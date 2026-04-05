using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyRing : MonoBehaviour
{
    [SerializeField] private List<DoorKeyType> startingKeys = new List<DoorKeyType>();
    [Header("Debug")]
    [SerializeField] private List<DoorKeyType> debugHeldKeys = new List<DoorKeyType>();

    private readonly HashSet<DoorKeyType> keys = new HashSet<DoorKeyType>();
    public event Action KeysChanged;

    public IEnumerable<DoorKeyType> HeldKeys => keys;

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
        if (!keys.Add(keyType))
            return;

        SyncDebugState();
        KeysChanged?.Invoke();
    }

    public bool TryConsumeKey(DoorKeyType keyType)
    {
        bool removed = keys.Remove(keyType);

        if (!removed)
            return false;

        SyncDebugState();
        KeysChanged?.Invoke();
        return true;
    }

    private void SyncDebugState()
    {
        debugHeldKeys.Clear();
        foreach (DoorKeyType key in keys)
            debugHeldKeys.Add(key);
    }
}
