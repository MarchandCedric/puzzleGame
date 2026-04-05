using UnityEngine;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private LevelButtonActivate[] levelButtons;

    public void SelectButton(LevelButtonActivate selectedButton)
    {
        foreach (LevelButtonActivate button in levelButtons)
        {
            if (button != null)
                button.SetSelected(button == selectedButton);
        }
    }
}
