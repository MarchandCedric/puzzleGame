using UnityEngine;

public class SupabaseAuthTestLogin : MonoBehaviour
{
    [SerializeField] private SupabaseAuthService authService;
    [SerializeField] private MainMenuAuthGate authGate;
    [SerializeField] private string accessToken;

    public void LoginWithTestToken()
    {
        if (authService == null)
            authService = FindAnyObjectByType<SupabaseAuthService>();

        if (authGate == null)
            authGate = FindAnyObjectByType<MainMenuAuthGate>();

        if (authService == null)
        {
            Debug.LogWarning($"{nameof(SupabaseAuthTestLogin)} failed: no {nameof(SupabaseAuthService)} found in the scene.");
            return;
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Debug.LogWarning($"{nameof(SupabaseAuthTestLogin)} failed: access token is empty.");
            return;
        } 

        if (authService != null && !authService.IsAuthenticated)
        {
            authService.SetSessionForTesting(accessToken);
        } else
        {
            Debug.LogWarning($"Deconnexion.");
            authService.SignOut();
        }
            

        if (authGate != null)
            authGate.RefreshFromAuthService();

        Debug.Log(authService.IsAuthenticated
            ? $"{nameof(SupabaseAuthTestLogin)} succeeded: test session is authenticated."
            : $"{nameof(SupabaseAuthTestLogin)} failed: token was stored, but auth service is not authenticated.");
    }
}
