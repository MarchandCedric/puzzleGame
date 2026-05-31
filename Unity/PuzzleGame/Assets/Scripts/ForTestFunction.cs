using UnityEngine;

public class SupabaseAuthTestLogin : MonoBehaviour
{
    [SerializeField] private SupabaseAuthService authService;
    [SerializeField] private string accessToken;

    public void LoginWithTestToken()
    {
        if (authService == null)
        {
            authService = FindAnyObjectByType<SupabaseAuthService>();
            authService.SetSessionForTesting(accessToken);
        }
        else
            authService.SignOut();
    }
        
}