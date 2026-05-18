using UnityEngine;

public class RotatePortal : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    public float speed = 0.5f;
    public float startY = 0f;
    public float maxY = 1f;
    public float fadeEndY = 0.2f;
    public float fadeStartY = 0.8f;

    private Renderer portalRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color baseColor;

    void Start()
    {
        portalRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        Material material = portalRenderer.sharedMaterial;
        baseColor = material != null && material.HasProperty(BaseColorId)
            ? material.GetColor(BaseColorId)
            : Color.white;
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        float y = transform.position.y;

        float alpha = 1f;

        if (y < fadeEndY)
        {
            alpha = Mathf.InverseLerp(startY, fadeEndY, y);
        }
        else if (y > fadeStartY)
        {
            alpha = Mathf.InverseLerp(maxY, fadeStartY, y);
        }

        Color c = baseColor;
        c.a = alpha;

        portalRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(BaseColorId, c);
        portalRenderer.SetPropertyBlock(propertyBlock);

        transform.Rotate(0, 50 * Time.deltaTime, 0);

        if (y >= maxY)
        {
            Vector3 pos = transform.position;
            pos.y = startY;
            transform.position = pos;
        }
    }
}
