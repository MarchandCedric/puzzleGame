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
    [SerializeField] private Graphic[] starColorTargets;
    [SerializeField] private Color earnedStarColor = Color.white;
    [SerializeField] private Color unearnedStarColor = Color.gray;
    [SerializeField] private GameObject[] unlockedOnlyObjects;
    [SerializeField] private GameObject[] lockedOnlyObjects;
    [SerializeField] private GameObject[] selectedOnlyObjects;
    [SerializeField] private Graphic[] selectedColorTargets;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private string noBestMovesText = "--";

    private LevelSelectionManager selectionManager;
    private Color[] originalSelectedTargetColors;

    private void Awake()
    {
        ResolveReferences();
        CaptureOriginalSelectedTargetColors();
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

        RefreshStarObjects(bestStars);

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
        ApplySelectedTargetColors(isSelected);
    }

    private void RefreshStarObjects(int bestStars)
    {
        if (starColorTargets == null)
            return;

        for (int i = 0; i < starColorTargets.Length; i++)
        {
            if (starColorTargets[i] != null)
                starColorTargets[i].color = i < bestStars ? earnedStarColor : unearnedStarColor;
        }
    }

    private static void SetObjectsActive(GameObject[] objects, bool isActive)
    {
        if (objects == null)
            return;

        foreach (GameObject obj in objects)
        {
            if (obj != null)
                obj.SetActive(isActive);
        }
    }

    private void CaptureOriginalSelectedTargetColors()
    {
        if (selectedColorTargets == null)
        {
            originalSelectedTargetColors = new Color[0];
            return;
        }

        originalSelectedTargetColors = new Color[selectedColorTargets.Length];
        for (int i = 0; i < selectedColorTargets.Length; i++)
        {
            if (selectedColorTargets[i] != null)
                originalSelectedTargetColors[i] = selectedColorTargets[i].color;
        }
    }

    private void ApplySelectedTargetColors(bool isSelected)
    {
        if (selectedColorTargets == null)
            return;

        if (originalSelectedTargetColors == null || originalSelectedTargetColors.Length != selectedColorTargets.Length)
            CaptureOriginalSelectedTargetColors();

        for (int i = 0; i < selectedColorTargets.Length; i++)
        {
            Graphic target = selectedColorTargets[i];
            if (target == null)
                continue;

            target.color = isSelected ? selectedColor : originalSelectedTargetColors[i];
        }
    }
}
