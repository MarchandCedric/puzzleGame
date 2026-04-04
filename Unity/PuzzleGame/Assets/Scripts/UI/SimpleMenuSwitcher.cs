using UnityEngine;

public class SimpleMenuSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject levelScreen;

    private void Start()
    {
        ShowMainScreen();
    }

    public void ShowMainScreen()
    {
        if (mainScreen != null)
            mainScreen.SetActive(true);

        if (levelScreen != null)
            levelScreen.SetActive(false);
    }

    public void ShowLevelScreen()
    {
        if (mainScreen != null)
            mainScreen.SetActive(false);

        if (levelScreen != null)
            levelScreen.SetActive(true);
    }
}
