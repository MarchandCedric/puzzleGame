using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private LevelButtonActivate[] levelButtons;
    [SerializeField] private bool autoSelectFirstLevel = true;

    private readonly List<LevelButtonActivate> registeredButtons = new List<LevelButtonActivate>();
    private LevelButtonActivate selectedButton;

    public bool HasSelectedLevel => selectedButton != null;
    public int SelectedLevelNumber => selectedButton != null ? selectedButton.LevelNumber : 0;
    public event Action<LevelButtonActivate> SelectionChanged;

    private void Awake()
    {
        RefreshSelectionVisuals();
    }

    private void Start()
    {
        if (autoSelectFirstLevel && selectedButton == null)
            SelectButton(GetFirstAvailableButton());
        else
            RefreshSelectionVisuals();
    }

    public void RegisterButton(LevelButtonActivate button)
    {
        if (button == null || registeredButtons.Contains(button))
            return;

        registeredButtons.Add(button);
        button.SetSelected(button == selectedButton);
    }

    public bool IsSelected(LevelButtonActivate button)
    {
        return button != null && button == selectedButton;
    }

    public void UnregisterButton(LevelButtonActivate button)
    {
        if (button == null)
            return;

        registeredButtons.Remove(button);

        if (selectedButton == button)
        {
            selectedButton = null;
            RefreshSelectionVisuals();
        }
    }

    public void SelectButton(LevelButtonActivate selectedButton)
    {
        if (selectedButton == null)
        {
            Debug.LogWarning($"{nameof(LevelSelectionManager)} cannot select a null level button.", this);
            return;
        }

        RegisterButton(selectedButton);
        this.selectedButton = selectedButton;
        RefreshSelectionVisuals();
    }

    public void ClearSelection()
    {
        selectedButton = null;
        RefreshSelectionVisuals();
    }

    private void RefreshSelectionVisuals()
    {
        ForEachKnownButton(button => button.SetSelected(button == selectedButton));
        SelectionChanged?.Invoke(selectedButton);
    }

    private LevelButtonActivate GetFirstAvailableButton()
    {
        LevelButtonActivate first = null;

        ForEachKnownButton(button =>
        {
            if (first == null || button.LevelNumber < first.LevelNumber)
                first = button;
        });

        return first;
    }

    private void ForEachKnownButton(System.Action<LevelButtonActivate> action)
    {
        var visitedButtons = new HashSet<LevelButtonActivate>();

        foreach (LevelButtonActivate button in levelButtons)
        {
            if (button != null && visitedButtons.Add(button))
                action(button);
        }

        foreach (LevelButtonActivate button in registeredButtons)
        {
            if (button != null && visitedButtons.Add(button))
                action(button);
        }
    }
}
