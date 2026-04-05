using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHudController : MonoBehaviour
{
    [Header("Scene Sources")]
    [SerializeField] private GridMover mover;
    [SerializeField] private PlayerKeyRing keyRing;
    [SerializeField] private LevelSceneMetadata metadata;

    [Header("Text")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text movesText;

    [Header("Keys")]
    [SerializeField] private Transform keysContainer;
    [SerializeField] private GameObject keyItemPrefab;
    [SerializeField] private TMP_Text noKeysText;

    private readonly List<GameObject> spawnedKeyItems = new List<GameObject>();

    private void Awake()
    {
        if (mover == null)
            mover = FindAnyObjectByType<GridMover>();

        if (keyRing == null)
            keyRing = FindAnyObjectByType<PlayerKeyRing>();

        if (metadata == null)
            metadata = FindAnyObjectByType<LevelSceneMetadata>();
    }

    private void OnEnable()
    {
        if (mover != null)
            mover.MoveResolved += HandleMoveResolved;

        if (keyRing != null)
            keyRing.KeysChanged += HandleKeysChanged;
    }

    private void Start()
    {
        RefreshAll();
    }

    private void OnDisable()
    {
        if (mover != null)
            mover.MoveResolved -= HandleMoveResolved;

        if (keyRing != null)
            keyRing.KeysChanged -= HandleKeysChanged;
    }

    private void HandleMoveResolved(Vector3Int _, int moveCount)
    {
        RefreshMoves(moveCount);
    }

    private void HandleKeysChanged()
    {
        RefreshKeys();
    }

    public void RefreshAll()
    {
        RefreshLevelName();
        RefreshMoves(mover != null ? mover.MoveCount : 0);
        RefreshKeys();
    }

    private void RefreshLevelName()
    {
        if (levelNameText == null)
            return;

        if (metadata == null)
        {
            levelNameText.text = "Level";
            return;
        }

        levelNameText.text = $"Level {metadata.World}-{metadata.Level}";
    }

    private void RefreshMoves(int moveCount)
    {
        if (movesText == null)
            return;

        movesText.text = $"Moves {moveCount}";
    }

    private void RefreshKeys()
    {
        if (keysContainer == null)
            return;

        ClearSpawnedKeyItems();

        List<DoorKeyType> heldKeys = new List<DoorKeyType>();
        if (keyRing != null)
        {
            foreach (DoorKeyType key in keyRing.HeldKeys)
                heldKeys.Add(key);
        }

        heldKeys.Sort();

        if (noKeysText != null)
            noKeysText.gameObject.SetActive(heldKeys.Count == 0);

        if (keyItemPrefab == null)
            return;

        foreach (DoorKeyType keyType in heldKeys)
        {
            GameObject keyItem = Instantiate(keyItemPrefab, keysContainer);
            keyItem.name = $"{keyType}KeyHudItem";
            ConfigureKeyItem(keyItem, keyType);
            spawnedKeyItems.Add(keyItem);
        }
    }

    private void ConfigureKeyItem(GameObject keyItem, DoorKeyType keyType)
    {
        TMP_Text label = keyItem.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = keyType.ToString().Substring(0, 1).ToUpperInvariant();

        Image image = keyItem.GetComponent<Image>();
        if (image == null)
            image = keyItem.GetComponentInChildren<Image>(true);

        if (image != null)
            image.color = GetKeyColor(keyType);
    }

    private static Color GetKeyColor(DoorKeyType keyType)
    {
        switch (keyType)
        {
            case DoorKeyType.Red:
                return new Color32(230, 77, 77, 255);
            case DoorKeyType.Blue:
                return new Color32(73, 145, 255, 255);
            case DoorKeyType.Green:
                return new Color32(63, 196, 110, 255);
            case DoorKeyType.Yellow:
                return new Color32(243, 191, 61, 255);
            default:
                return Color.white;
        }
    }

    private void ClearSpawnedKeyItems()
    {
        foreach (GameObject item in spawnedKeyItems)
        {
            if (item != null)
                Destroy(item);
        }

        spawnedKeyItems.Clear();
    }
}
