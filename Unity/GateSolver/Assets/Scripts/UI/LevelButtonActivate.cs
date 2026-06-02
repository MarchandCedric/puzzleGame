using UnityEngine;

public class LevelButtonActivate : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;
    [SerializeField] private LevelSelectionManager selectionManager;
    [SerializeField] private GameObject[] selectedOnlyObjects;

    public int LevelNumber => levelNumber;
    public LevelSelectionManager SelectionManager
    {
        get
        {
            ResolveSelectionManager();
            return selectionManager;
        }
    }

    private void Awake()
    {
        ResolveSelectionManager();
    }

    private void OnEnable()
    {
        ResolveSelectionManager();

        if (selectionManager != null)
            selectionManager.RegisterButton(this);
    }

    private void OnDisable()
    {
        if (selectionManager != null)
            selectionManager.UnregisterButton(this);
    }

    public void SetSelected(bool isSelected)
    {
        foreach (GameObject obj in selectedOnlyObjects)
        {
            if (obj != null)
                obj.SetActive(isSelected);
        }
    }

    public void SelectThisButton()
    {
        ResolveSelectionManager();

        if (selectionManager == null)
        {
            Debug.LogWarning($"{nameof(LevelButtonActivate)} on {name} cannot select because no {nameof(LevelSelectionManager)} was found.", this);
            return;
        }

        selectionManager.SelectButton(this);
    }

    public void Select()
    {
        SelectThisButton();
    }

    public void Unselect()
    {
        SetSelected(false);
    }

    private void ResolveSelectionManager()
    {
        if (selectionManager != null)
            return;

        selectionManager = GetComponentInParent<LevelSelectionManager>();

        if (selectionManager == null)
            selectionManager = FindAnyObjectByType<LevelSelectionManager>();
    }
}
