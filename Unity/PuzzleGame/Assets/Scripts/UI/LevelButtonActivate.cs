using UnityEngine;

public class LevelButtonActivate : MonoBehaviour
{
    [SerializeField] private GameObject[] selectedOnlyObjects;

    public void SetSelected(bool isSelected)
    {
        foreach (GameObject obj in selectedOnlyObjects)
        {
            if (obj != null)
                obj.SetActive(isSelected);
        }
    }

    public void Select()
    {
        SetSelected(true);
    }

    public void Unselect()
    {
        SetSelected(false);
    }
}
