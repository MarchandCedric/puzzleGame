using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DeepLinkDebugText : MonoBehaviour
{
    private TMP_Text text;
    private SupabaseAuthService authService;
    private string lastDeepLinkText = "Deep link: waiting...";

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        authService = FindAnyObjectByType<SupabaseAuthService>();
        RefreshText();

        Application.deepLinkActivated += HandleDeepLink;

        if (!string.IsNullOrWhiteSpace(Application.absoluteURL))
            HandleDeepLink(Application.absoluteURL);

        TryReadAndroidIntentDeepLink();
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= HandleDeepLink;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            TryReadAndroidIntentDeepLink();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            TryReadAndroidIntentDeepLink();
    }

    private void HandleDeepLink(string url)
    {
        lastDeepLinkText = string.IsNullOrEmpty(url) ? "Deep link: empty" : url;
        RefreshText();
    }

    private void RefreshText()
    {
        string authText = authService != null ? authService.LastAuthDebugMessage : "Auth: service not found.";
        text.text = $"{lastDeepLinkText}\n{authText}";
    }

    private void TryReadAndroidIntentDeepLink()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string url = GetAndroidIntentDataString();
        if (!string.IsNullOrEmpty(url))
            HandleDeepLink(url);
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    private static string GetAndroidIntentDataString()
    {
        try
        {
            using AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");

            return intent?.Call<string>("getDataString");
        }
        catch
        {
            return string.Empty;
        }
    }
#endif
}
