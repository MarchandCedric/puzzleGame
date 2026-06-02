using System.Collections;
using UnityEngine;

public class PortalCompletionEffect : MonoBehaviour
{
    [SerializeField] private Transform playerTarget = null;
    [SerializeField] private GameObject flashRoot = null;
    [SerializeField] private Light flashLight = null;
    [SerializeField] private float playerScaleOutPortion = 0.35f;
    [SerializeField] private float flashStartPortion = 0.2f;
    [SerializeField] private float maxLightIntensity = 3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);

    private Coroutine playRoutine;
    private Vector3 originalPlayerScale = Vector3.one;

    private void Awake()
    {
        if (flashRoot != null)
            flashRoot.SetActive(false);

        if (flashLight != null)
            flashLight.intensity = 0f;
    }

    public void Play(Transform fallbackPlayerTarget, float duration)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        Transform target = playerTarget != null ? playerTarget : fallbackPlayerTarget;
        playRoutine = StartCoroutine(PlayRoutine(target, Mathf.Max(0.01f, duration)));
    }

    private IEnumerator PlayRoutine(Transform target, float duration)
    {
        if (target != null)
            originalPlayerScale = target.localScale;

        if (flashRoot != null)
        {
            flashRoot.transform.position = target != null ? target.position : transform.position;
            flashRoot.SetActive(false);
        }

        float scaleDuration = Mathf.Max(0.01f, duration * Mathf.Clamp01(playerScaleOutPortion));
        float flashDelay = Mathf.Max(0f, duration * Mathf.Clamp01(flashStartPortion));
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (target != null)
            {
                float scaleT = Mathf.Clamp01(elapsed / scaleDuration);
                float scale = Mathf.Clamp01(scaleCurve.Evaluate(scaleT));
                target.localScale = originalPlayerScale * scale;
            }

            if (elapsed >= flashDelay)
            {
                if (flashRoot != null && !flashRoot.activeSelf)
                    flashRoot.SetActive(true);

                if (flashLight != null)
                {
                    float flashT = Mathf.Clamp01((elapsed - flashDelay) / Mathf.Max(0.01f, duration - flashDelay));
                    flashLight.intensity = Mathf.Max(0f, flashCurve.Evaluate(flashT) * maxLightIntensity);
                }
            }

            yield return null;
        }

        if (target != null)
            target.localScale = Vector3.zero;

        if (flashLight != null)
            flashLight.intensity = 0f;

        if (flashRoot != null)
            flashRoot.SetActive(false);

        playRoutine = null;
    }
}
