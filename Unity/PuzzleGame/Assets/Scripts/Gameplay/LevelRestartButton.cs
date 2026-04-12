using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRestartButton : MonoBehaviour
{
    public void RestartCurrentLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
