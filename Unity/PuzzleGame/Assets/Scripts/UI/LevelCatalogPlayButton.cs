using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCatalogPlayButton : MonoBehaviour
{
    [SerializeField] private LevelCatalog levelCatalog;

    public void LoadFirstLevel()
    {
        if (levelCatalog == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because no catalog is assigned.");
            return;
        }

        LevelCatalogEntry firstLevel = levelCatalog.GetFirstLevel();
        if (firstLevel == null)
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because the catalog is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(firstLevel.SceneName))
        {
            Debug.LogWarning($"{nameof(LevelCatalogPlayButton)} cannot load first level because its scene name is empty.");
            return;
        }

        SceneManager.LoadScene(firstLevel.SceneName);
    }
}
