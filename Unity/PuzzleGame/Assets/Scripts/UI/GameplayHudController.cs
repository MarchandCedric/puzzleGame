using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHudController : MonoBehaviour
{
    [System.Serializable]
    private struct ItemSpriteEntry
    {
        public DoorKeyType keyType;
        public Sprite sprite;
    }

    [Header("Scene Sources")]
    [SerializeField] private GridMover mover;
    [SerializeField] private PlayerKeyRing keyRing;
    [SerializeField] private LevelSceneMetadata metadata;

    [Header("Text")]
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text movesText;

    [Header("Items")]
    [SerializeField] private Transform keysContainer;
    [SerializeField] private TMP_Text noKeysText;
    [SerializeField] private bool hideUnusedItemSlots = true;
    [SerializeField] private bool tintItemImage = false;
    [SerializeField] private ItemSpriteEntry[] itemSprites;

    private readonly List<GameObject> itemSlots = new List<GameObject>();

    private void Awake()
    {
        if (mover == null)
            mover = FindAnyObjectByType<GridMover>();

        if (keyRing == null)
            keyRing = FindAnyObjectByType<PlayerKeyRing>();

        if (metadata == null)
            metadata = FindAnyObjectByType<LevelSceneMetadata>();

        CacheItemSlots();
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

        List<DoorKeyType> heldKeys = new List<DoorKeyType>();
        if (keyRing != null)
        {
            foreach (DoorKeyType key in keyRing.HeldKeys)
                heldKeys.Add(key);
        }

        heldKeys.Sort();

        if (noKeysText != null)
            noKeysText.gameObject.SetActive(heldKeys.Count == 0);

        for (int i = 0; i < itemSlots.Count; i++)
        {
            bool hasItem = i < heldKeys.Count;
            ConfigureItemSlot(itemSlots[i], hasItem, hasItem ? heldKeys[i] : DoorKeyType.Red);
        }

        if (heldKeys.Count > itemSlots.Count)
            Debug.LogWarning($"[GameplayHudController] {heldKeys.Count} items held, but only {itemSlots.Count} HUD item slots are available.", this);
    }

    private void ConfigureItemSlot(GameObject slot, bool hasItem, DoorKeyType keyType)
    {
        if (slot == null)
            return;

        slot.SetActive(hasItem || !hideUnusedItemSlots);

        TMP_Text label = slot.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
            label.text = hasItem ? keyType.ToString().Substring(0, 1).ToUpperInvariant() : string.Empty;

        Image image = slot.GetComponent<Image>();
        if (image == null)
            image = slot.GetComponentInChildren<Image>(true);

        if (image != null)
        {
            image.sprite = hasItem ? GetItemSprite(keyType) : null;
            image.color = hasItem
                ? (tintItemImage ? GetKeyColor(keyType) : Color.white)
                : new Color(1f, 1f, 1f, 0f);
            image.preserveAspect = true;
        }
    }

    private Sprite GetItemSprite(DoorKeyType keyType)
    {
        foreach (ItemSpriteEntry entry in itemSprites)
        {
            if (entry.keyType == keyType)
                return entry.sprite;
        }

        return null;
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

    private void CacheItemSlots()
    {
        itemSlots.Clear();

        if (keysContainer == null)
            return;

        for (int i = 0; i < keysContainer.childCount; i++)
            itemSlots.Add(keysContainer.GetChild(i).gameObject);
    }
}
