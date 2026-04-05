using UnityEngine;

public class ScrollingBackgroundTexture : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private string texturePropertyName = "_BaseMap";
    [SerializeField] private Vector2 scrollSpeed = new Vector2(0.0035f, 0.0012f);
    [SerializeField] private Vector2 initialOffset = Vector2.zero;

    private Material runtimeMaterial;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponent<Renderer>();

        if (targetRenderer != null)
            runtimeMaterial = targetRenderer.material;
    }

    private void LateUpdate()
    {
        if (runtimeMaterial == null)
            return;

        Vector2 offset = initialOffset + (scrollSpeed * Time.time);
        runtimeMaterial.SetTextureOffset(texturePropertyName, offset);
    }
}
