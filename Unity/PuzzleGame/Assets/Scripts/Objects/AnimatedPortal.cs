using UnityEngine;

public class PortalPulse : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private float inactiveAlpha = 0.25f;
    [SerializeField] private float inactiveEmissionMultiplier = 0.2f;
    [SerializeField] private float activeEmissionMultiplier = 1f;

    private Renderer[] portalRenderers;
    private MaterialPropertyBlock propertyBlock;
    private Color[] baseColors;
    private Color[] emissionColors;
    private bool isActive;

    void Start()
    {
        portalRenderers = GetComponentsInChildren<Renderer>(true);
        propertyBlock = new MaterialPropertyBlock();
        baseColors = new Color[portalRenderers.Length];
        emissionColors = new Color[portalRenderers.Length];

        for (int i = 0; i < portalRenderers.Length; i++)
        {
            Material material = portalRenderers[i].sharedMaterial;
            baseColors[i] = material != null && material.HasProperty(BaseColorId)
                ? material.GetColor(BaseColorId)
                : Color.white;
            emissionColors[i] = material != null && material.HasProperty(EmissionColorId)
                ? material.GetColor(EmissionColorId)
                : baseColors[i];
        }

        ApplyVisualState();
    }

    void Update()
    {
        // float alpha = 0.15f + Mathf.Sin(Time.time * 1f) * 0.1f;

        // Color c = mat.color;
        // c.a = alpha;
        // mat.color = c;
    }

    public void SetPortalActive(bool active)
    {
        if (isActive == active)
            return;

        isActive = active;
        ApplyVisualState();
    }

    private void ApplyVisualState()
    {
        if (portalRenderers == null || propertyBlock == null)
            return;

        float alpha = isActive ? 1f : inactiveAlpha;
        float emissionMultiplier = isActive ? activeEmissionMultiplier : inactiveEmissionMultiplier;

        for (int i = 0; i < portalRenderers.Length; i++)
        {
            Renderer portalRenderer = portalRenderers[i];
            if (portalRenderer == null)
                continue;

            Color baseColor = baseColors[i];
            baseColor.a = alpha;

            Color emissionColor = emissionColors[i] * emissionMultiplier;

            portalRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, baseColor);
            propertyBlock.SetColor(EmissionColorId, emissionColor);
            portalRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
