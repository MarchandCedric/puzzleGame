using UnityEngine;

public class SimpleMenuSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainScreen;
    [SerializeField] private GameObject levelScreen;
    [SerializeField] private GameObject warningOfflineScreen;
    [SerializeField] private MainMenuAuthGate authGate;

    private void Start()
    {
        if (authGate == null)
            authGate = FindAnyObjectByType<MainMenuAuthGate>();

        ShowMainScreen();
    }

    public void ShowMainScreen()
    {
        if (mainScreen != null)
            mainScreen.SetActive(true);

        if (levelScreen != null)
            levelScreen.SetActive(false);

        if (warningOfflineScreen != null)
            warningOfflineScreen.SetActive(false);
    }

    public void ShowLevelScreen()
    {
        if (mainScreen != null)
            mainScreen.SetActive(false);

        if (levelScreen != null)
            levelScreen.SetActive(true);

        if (warningOfflineScreen != null)
            warningOfflineScreen.SetActive(false);
    }

    public void ShowWarningOfflineScreen()
    {
        if (authGate != null)
        {
            authGate.ShowOfflineWarning();
            return;
        }

        if (mainScreen != null)
            mainScreen.SetActive(false);

        if (levelScreen != null)
            levelScreen.SetActive(false);

        if (warningOfflineScreen != null)
            warningOfflineScreen.SetActive(true);
    }

    public void ConfirmOfflinePlay()
    {
        if (authGate != null)
        {
            authGate.ConfirmOfflinePlay();
            return;
        }

        if (warningOfflineScreen != null)
            warningOfflineScreen.SetActive(false);

        ShowMainScreen();
    }

    public void CancelOfflineWarning()
    {
        if (authGate != null)
        {
            authGate.CancelOfflinePlay();
            return;
        }

        ShowMainScreen();
    }
}
