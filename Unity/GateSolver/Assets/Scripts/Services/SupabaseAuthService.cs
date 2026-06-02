using System;
using System.Collections.Generic;
using UnityEngine;

public class SupabaseAuthService : MonoBehaviour
{
    private const string AccessTokenKey = "auth.supabase.access_token";
    private const string RefreshTokenKey = "auth.supabase.refresh_token";
    private const string ExpiresAtKey = "auth.supabase.expires_at";

    [SerializeField] private string supabaseUrl = "";
    [SerializeField] private string anonKey = "";
    [SerializeField] private string provider = "google";
    [SerializeField] private string scopes = "openid email profile";
    [SerializeField] private string redirectUrl = "gatesolver://auth/callback";

    public string SupabaseUrl => supabaseUrl;
    public string AnonKey => anonKey;
    public string AccessToken => PlayerPrefs.GetString(AccessTokenKey, string.Empty);
    public string RefreshToken => PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
    public bool IsAuthenticated => HasStoredAccessToken() && !IsAccessTokenExpired();
    public string LastAuthDebugMessage { get; private set; } = "Auth: waiting...";

    public event Action<bool> AuthenticationChanged;

    private string lastHandledDeepLinkUrl;

    private void Awake()
    {
        Application.deepLinkActivated += TryHandleDeepLink;

        if (!string.IsNullOrWhiteSpace(Application.absoluteURL))
            TryHandleDeepLink(Application.absoluteURL);

        TryHandleAndroidIntentDeepLink();
    }

    private void Start()
    {
        TryHandleAndroidIntentDeepLink();
        AuthenticationChanged?.Invoke(IsAuthenticated);
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= TryHandleDeepLink;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            TryHandleAndroidIntentDeepLink();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
            TryHandleAndroidIntentDeepLink();
    }

    public void SignIn()
    {
        if (string.IsNullOrWhiteSpace(supabaseUrl))
        {
            Debug.LogWarning($"{nameof(SupabaseAuthService)} cannot sign in because Supabase URL is empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(redirectUrl))
        {
            Debug.LogWarning($"{nameof(SupabaseAuthService)} cannot sign in because redirect URL is empty.");
            return;
        }

        string baseUrl = supabaseUrl.TrimEnd('/');
        string authUrl =
            $"{baseUrl}/auth/v1/authorize?provider={Uri.EscapeDataString(provider)}" +
            $"&redirect_to={Uri.EscapeDataString(redirectUrl)}" +
            $"&scopes={Uri.EscapeDataString(scopes)}";

        Application.OpenURL(authUrl);
    }

    public void SignOut()
    {
        PlayerPrefs.DeleteKey(AccessTokenKey);
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.DeleteKey(ExpiresAtKey);
        PlayerPrefs.Save();

        AuthenticationChanged?.Invoke(false);
    }

    public void CompleteSignInFromCallbackUrl(string callbackUrl)
    {
        TryHandleDeepLink(callbackUrl);
    }

    public void SetSessionForTesting(string accessToken, string refreshToken = "", int expiresInSeconds = 3600)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.LogWarning($"{nameof(SupabaseAuthService)} cannot store an empty access token.");
            return;
        }

        StoreSession(accessToken, refreshToken, expiresInSeconds);
        AuthenticationChanged?.Invoke(IsAuthenticated);
    }

    private void TryHandleDeepLink(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (string.Equals(lastHandledDeepLinkUrl, url, StringComparison.Ordinal))
            return;

        if (!string.IsNullOrWhiteSpace(redirectUrl) &&
            !url.StartsWith(redirectUrl, StringComparison.OrdinalIgnoreCase))
        {
            LastAuthDebugMessage = "Auth: ignored URL, redirectUrl mismatch.";
            return;
        }

        lastHandledDeepLinkUrl = url;
        Debug.Log($"{nameof(SupabaseAuthService)} received auth callback URL.");

        Dictionary<string, string> parameters = ParseCallbackParameters(url);
        LastAuthDebugMessage = $"Auth: parsed {parameters.Count} parameter(s).";

        if (parameters.TryGetValue("error", out string error))
        {
            LastAuthDebugMessage = $"Auth error: {error}";
            Debug.LogWarning($"Supabase sign-in failed: {error}");
            AuthenticationChanged?.Invoke(false);
            return;
        }

        if (parameters.ContainsKey("code"))
        {
            LastAuthDebugMessage = "Auth: received code, not access_token.";
            Debug.LogWarning("Supabase returned an auth code. This Unity client expects the implicit OAuth flow with access_token in the callback.");
            AuthenticationChanged?.Invoke(false);
            return;
        }

        if (!parameters.TryGetValue("access_token", out string accessToken) || string.IsNullOrWhiteSpace(accessToken))
        {
            LastAuthDebugMessage = "Auth: no access_token found.";
            Debug.LogWarning("Supabase sign-in callback did not contain an access_token.");
            AuthenticationChanged?.Invoke(false);
            return;
        }

        parameters.TryGetValue("refresh_token", out string refreshToken);
        int expiresInSeconds = 3600;
        if (parameters.TryGetValue("expires_in", out string expiresInText))
            int.TryParse(expiresInText, out expiresInSeconds);

        StoreSession(accessToken, refreshToken, expiresInSeconds);
        LastAuthDebugMessage = $"Auth: stored token, IsAuthenticated={IsAuthenticated}.";
        AuthenticationChanged?.Invoke(IsAuthenticated);
    }

    private void TryHandleAndroidIntentDeepLink()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        string intentUrl = GetAndroidIntentDataString();
        if (!string.IsNullOrWhiteSpace(intentUrl))
            TryHandleDeepLink(intentUrl);
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
        catch (Exception exception)
        {
            Debug.LogWarning($"{nameof(SupabaseAuthService)} could not read Android intent data: {exception.Message}");
            return string.Empty;
        }
    }
#endif

    private static void StoreSession(string accessToken, string refreshToken, int expiresInSeconds)
    {
        PlayerPrefs.SetString(AccessTokenKey, accessToken);

        if (!string.IsNullOrWhiteSpace(refreshToken))
            PlayerPrefs.SetString(RefreshTokenKey, refreshToken);

        long expiresAt = DateTimeOffset.UtcNow.AddSeconds(Mathf.Max(1, expiresInSeconds)).ToUnixTimeSeconds();
        PlayerPrefs.SetString(ExpiresAtKey, expiresAt.ToString());
        PlayerPrefs.Save();
    }

    private static Dictionary<string, string> ParseCallbackParameters(string url)
    {
        string decodedUrl = Uri.UnescapeDataString(url);
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        AddParameters(result, GetUriPart(decodedUrl, '?', '#'));
        AddParameters(result, GetUriPart(decodedUrl, '#', '\0'));
        return result;
    }

    private static string GetUriPart(string url, char startMarker, char endMarker)
    {
        int startIndex = url.IndexOf(startMarker);
        if (startIndex < 0 || startIndex + 1 >= url.Length)
            return string.Empty;

        int valueStartIndex = startIndex + 1;
        int endIndex = endMarker == '\0' ? -1 : url.IndexOf(endMarker, valueStartIndex);
        if (endIndex < 0)
            return url.Substring(valueStartIndex);

        return url.Substring(valueStartIndex, endIndex - valueStartIndex);
    }

    private static void AddParameters(Dictionary<string, string> result, string parameterText)
    {
        if (string.IsNullOrWhiteSpace(parameterText))
            return;

        string[] pairs = parameterText.Split('&');
        foreach (string pair in pairs)
        {
            if (string.IsNullOrWhiteSpace(pair))
                continue;

            int separatorIndex = pair.IndexOf('=');
            string key = separatorIndex >= 0 ? pair.Substring(0, separatorIndex) : pair;
            string value = separatorIndex >= 0 ? pair.Substring(separatorIndex + 1) : string.Empty;

            if (string.IsNullOrWhiteSpace(key))
                continue;

            result[Uri.UnescapeDataString(key)] = Uri.UnescapeDataString(value);
        }
    }

    private static bool HasStoredAccessToken()
    {
        return !string.IsNullOrWhiteSpace(PlayerPrefs.GetString(AccessTokenKey, string.Empty));
    }

    private static bool IsAccessTokenExpired()
    {
        string expiresAtText = PlayerPrefs.GetString(ExpiresAtKey, string.Empty);
        if (!long.TryParse(expiresAtText, out long expiresAt))
            return false;

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return now >= expiresAt;
    }
}
