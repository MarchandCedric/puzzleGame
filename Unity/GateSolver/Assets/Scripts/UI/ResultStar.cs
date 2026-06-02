using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultStar : MonoBehaviour
{
    [SerializeField] private Image emptyStar;
    [SerializeField] private Image filledStar;

    [Header("Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip appearSound;
    [SerializeField] private ParticleSystem appearParticles;

    public void SetEmpty()
    {
        emptyStar.enabled = true;
        filledStar.enabled = false;

        filledStar.transform.localScale = Vector3.zero;

        if (appearParticles != null)
            appearParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public IEnumerator ShowFilled(float delay)
    {
        yield return new WaitForSeconds(delay);

        filledStar.enabled = true;
        filledStar.color = new Color(1, 1, 1, 0);
        filledStar.transform.localScale = Vector3.zero;

        if (audioSource != null && appearSound != null)
            audioSource.PlayOneShot(appearSound);

        if (appearParticles != null)
            appearParticles.Play();

        float duration = 0.25f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            float scale = Mathf.Lerp(0f, 1.25f, p);
            float alpha = Mathf.Lerp(0f, 1f, p);

            filledStar.transform.localScale = Vector3.one * scale;

            Color c = filledStar.color;
            c.a = alpha;
            filledStar.color = c;

            yield return null;
        }

        duration = 0.12f;
        t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;

            float scale = Mathf.Lerp(1.25f, 1f, p);
            filledStar.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        filledStar.transform.localScale = Vector3.one;

        Color finalColor = filledStar.color;
        finalColor.a = 1f;
        filledStar.color = finalColor;
    }
}