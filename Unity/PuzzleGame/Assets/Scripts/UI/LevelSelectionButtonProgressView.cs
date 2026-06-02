using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class LevelSelectionButtonProgressView : MonoBehaviour
{
    [SerializeField] private LevelCatalog levelCatalog;
    [SerializeField] private LevelButtonActivate levelButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bestMovesText;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private TMP_Text attemptsText;
    [SerializeField] private Button button;
    [SerializeField] private GameObject[] earnedStarObjects;
    [SerializeField] private GameObject[] unlockedOnlyObjects;
    [SerializeField] private GameObject[] lockedOnlyObjects;
    [SerializeField] private GameObject[] selectedOnlyObjects;
    [SerializeField] private string noBestMovesText = "--";

    private LevelSelectionManager selectionManager;

    private void Awake()
    {
        ResolveReferences();
        Refresh();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToSelectionManager();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromSelectionManager();
    }

    public void Refresh()
    {
        if (levelButton == null)
            return;

        int world = 1;
        int level = levelButton.LevelNumber;
        string title = $"Level {levelButton.LevelNumber}";
        bool isUnlocked = levelCatalog == null || levelButton.LevelNumber <= 1;

        if (levelCatalog != null && levelCatalog.TryGetByLevelNumber(levelButton.LevelNumber, out LevelCatalogEntry entry))
        {
            world = entry.World;
            level = entry.Level;
            title = entry.Title;
            isUnlocked = levelCatalog.IsLevelUnlocked(entry.LevelNumber);
        }

        int bestMoves = LevelProgressStore.GetBestMoves(world, level);
        int bestStars = LevelProgressStore.GetBestStars(world, level);
        int attempts = LevelProgressStore.GetTotalAttempts(world, level);

        if (titleText != null)
            titleText.text = title;

        if (bestMovesText != null)
            bestMovesText.text = bestMoves > 0 ? bestMoves.ToString() : noBestMovesText;

        if (starsText != null)
            starsText.text = $"{bestStars}/3";

        if (attemptsText != null)
            attemptsText.text = attempts.ToString();

        if (button != null)
            button.interactable = isUnlocked;

        for (int i = 0; i < earnedStarObjects.Length; i++)
        {
            if (earnedStarObjects[i] != null)
                earnedStarObjects[i].SetActive(i < bestStars);
        }

        SetObjectsActive(unlockedOnlyObjects, isUnlocked);
        SetObjectsActive(lockedOnlyObjects, !isUnlocked);
        RefreshSelectedObjects();
    }

    private void ResolveReferences()
    {
        if (levelButton == null)
            levelButton = GetComponent<LevelButtonActivate>();

        if (button == null)
            button = GetComponent<Button>();
    }

    private void SubscribeToSelectionManager()
    {
        UnsubscribeFromSelectionManager();

        if (levelButton == null)
            return;

        selectionManager = levelButton.SelectionManager;
        if (selectionManager != null)
            selectionManager.SelectionChanged += HandleSelectionChanged;
    }

    private void UnsubscribeFromSelectionManager()
    {
        if (selectionManager != null)
            selectionManager.SelectionChanged -= HandleSelectionChanged;

        selectionManager = null;
    }

    private void HandleSelectionChanged(LevelButtonActivate selectedButton)
    {
        RefreshSelectedObjects();
    }

    private void RefreshSelectedObjects()
    {
        bool isSelected = selectionManager != null && selectionManager.IsSelected(levelButton);
        SetObjectsActive(selectedOnlyObjects, isSelected);
    }

    private static void SetObjectsActive(GameObject[] objects, bool isActive)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(isActive);
        }
    }
}
