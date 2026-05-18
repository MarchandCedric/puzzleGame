using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelCompletionUiController : MonoBehaviour
{
    [SerializeField] private LevelSceneFlowController levelFlow = null;
    [SerializeField] private GameObject root = null;
    [SerializeField] private TMP_Text levelText = null;
    [SerializeField] private TMP_Text movesText = null;
    [SerializeField] private TMP_Text starsText = null;
    [SerializeField] private GameObject[] earnedStarObjects = new GameObject[0];
    [SerializeField] private Button nextLevelButton = null;
    [SerializeField] private Button menuButton = null;
    [SerializeField] private Button retryButton = null;

    private void Awake()
    {
        if (levelFlow == null)
            levelFlow = FindAnyObjectByType<LevelSceneFlowController>();

        if (root == null)
            root = gameObject;

        if (levelFlow != null)
            levelFlow.LevelCompleted += HandleLevelCompleted;

        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(LoadNextLevel);

        if (menuButton != null)
            menuButton.onClick.AddListener(ReturnToMenu);

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryLevel);

        if (levelFlow != null && levelFlow.HasCompleted)
            HandleLevelCompleted(levelFlow.LastCompletionResult);
        else
            root.SetActive(false);
    }

    private void OnDestroy()
    {
        if (levelFlow != null)
            levelFlow.LevelCompleted -= HandleLevelCompleted;

        if (nextLevelButton != null)
            nextLevelButton.onClick.RemoveListener(LoadNextLevel);

        if (menuButton != null)
            menuButton.onClick.RemoveListener(ReturnToMenu);

        if (retryButton != null)
            retryButton.onClick.RemoveListener(RetryLevel);
    }

    private void HandleLevelCompleted(LevelCompletionResult result)
    {
        root.SetActive(true);

        if (levelText != null)
            levelText.text = $"Level {result.World}-{result.Level}";

        if (movesText != null)
            movesText.text = $"Moves {result.MoveCount}";

        if (starsText != null)
            starsText.text = $"{result.Stars}/3";

        for (int i = 0; i < earnedStarObjects.Length; i++)
        {
            if (earnedStarObjects[i] != null)
                earnedStarObjects[i].SetActive(i < result.Stars);
        }

        if (nextLevelButton != null)
            nextLevelButton.interactable = levelFlow != null && levelFlow.HasNextLevel;
    }

    private void LoadNextLevel()
    {
        if (levelFlow != null)
            levelFlow.LoadNextLevel();
    }

    private void ReturnToMenu()
    {
        if (levelFlow != null)
            levelFlow.ReturnToMenu();
    }

    private static void RetryLevel()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
}
