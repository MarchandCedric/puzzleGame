using System;
using System.Collections;
using UnityEngine;

public class ResultStarsController : MonoBehaviour
{
    [SerializeField] private ResultStar[] stars;

    public bool IsAnimating { get; private set; }
    public bool HasCompletedAnimation { get; private set; }
    public event Action AnimationCompleted;

    public void ShowStars(int starCount)
    {
        StopAllCoroutines();
        IsAnimating = true;
        HasCompletedAnimation = false;

        foreach (var star in stars)
        {
            if (star != null)
                star.SetEmpty();
        }

        StartCoroutine(AnimateStars(starCount));
    }

    private IEnumerator AnimateStars(int starCount)
    {
        for (int i = 0; i < starCount && i < stars.Length; i++)
        {
            if (stars[i] == null)
                continue;

            yield return stars[i].ShowFilled(i * 0.18f);
        }

        IsAnimating = false;
        HasCompletedAnimation = true;
        AnimationCompleted?.Invoke();
    }
}
