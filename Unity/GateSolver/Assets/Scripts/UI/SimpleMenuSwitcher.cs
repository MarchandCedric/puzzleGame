using UnityEngine;

public class SimpleMenuSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject levelScreen;
    [SerializeField] private GameObject warningOfflineScreen;

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

     public void ShowWarningOfflineScreen()
    {
        if (mainScreen != null)
            mainScreen.SetActive(false);

        if (warningOfflineScreen != null)
            warningOfflineScreen.SetActive(true);
    }
}
