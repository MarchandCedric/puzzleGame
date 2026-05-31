using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAuthGate : MonoBehaviour
{
    [SerializeField] private SupabaseAuthService authService;
    [SerializeField] private GameObject loginRoot;
    [SerializeField] private GameObject authenticatedRoot;
    [SerializeField] private GameObject loginLoadingRoot;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private Graphic[] loginPendingTintTargets;
    [SerializeField] private Color loginPendingTint = new Color(0.55f, 0.55f, 0.55f, 0.65f);
    [SerializeField] private bool pulseLoginButtonWhilePending = true;
    [SerializeField] private float pendingPulseMinAlpha = 0.45f;
    [SerializeField] private float pendingPulseSpeed = 2.5f;
    [SerializeField] private TMP_Text loginButtonText;
    [SerializeField] private string loginPendingText = "Connecting...";
    [SerializeField] private Color loginPendingTextColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [Header("Transitions")]
    [SerializeField] private CanvasGroup loginCanvasGroup;
    [SerializeField] private CanvasGroup authenticatedCanvasGroup;
    [SerializeField] private float transitionDuration = 0.25f;

    private bool isLoginPending;
    private bool hasAppliedInitialState;
    private Coroutine transitionRoutine;
    private Coroutine loginPendingPulseRoutine;
    private Color[] loginNormalTints;
    private string loginNormalText;
    private Color loginNormalTextColor;

    private void Awake()
    {
        if (authService == null)
            authService = FindAnyObjectByType<SupabaseAuthService>();

        CacheLoginNormalTints();

        if (loginButtonText != null)
        {
            loginNormalText = loginButtonText.text;
            loginNormalTextColor = loginButtonText.color;
        }
    }

    private void OnEnable()
    {
        if (authService != null)
            authService.AuthenticationChanged += HandleAuthenticationChanged;

        if (loginButton != null)
            loginButton.onClick.AddListener(SignIn);

        if (signOutButton != null)
            signOutButton.onClick.AddListener(SignOut);

        Refresh();
    }

    private void OnDisable()
    {
        if (authService != null)
            authService.AuthenticationChanged -= HandleAuthenticationChanged;

        if (loginButton != null)
            loginButton.onClick.RemoveListener(SignIn);

        if (signOutButton != null)
            signOutButton.onClick.RemoveListener(SignOut);
    }

    public void Refresh()
    {
        bool isAuthenticated = authService != null && authService.IsAuthenticated;
        ApplyAuthenticatedState(isAuthenticated);
        ApplyLoginPendingState(isLoginPending && !isAuthenticated);
    }

    private void HandleAuthenticationChanged(bool isAuthenticated)
    {
        isLoginPending = false;
        ApplyAuthenticatedState(isAuthenticated);
        ApplyLoginPendingState(false);
    }

    private void ApplyAuthenticatedState(bool isAuthenticated)
    {
        if (!hasAppliedInitialState || transitionDuration <= 0f)
        {
            SetRootVisible(loginRoot, loginCanvasGroup, !isAuthenticated, true);
            SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, isAuthenticated, true);
            hasAppliedInitialState = true;
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionAuthenticatedState(isAuthenticated));
    }

    private void SignIn()
    {
        if (authService == null)
            return;

        isLoginPending = true;
        ApplyLoginPendingState(true);
        authService.SignIn();
    }

    private void SignOut()
    {
        isLoginPending = false;
        ApplyLoginPendingState(false);

        if (authService != null)
            authService.SignOut();
    }

    private void ApplyLoginPendingState(bool isPending)
    {
        if (loginLoadingRoot != null)
            loginLoadingRoot.SetActive(isPending);

        if (loginButton != null)
            loginButton.interactable = !isPending;

        if (loginButtonText != null)
        {
            loginButtonText.text = isPending ? loginPendingText : loginNormalText;
            loginButtonText.color = isPending ? loginPendingTextColor : loginNormalTextColor;
        }

        ApplyPendingTints(isPending);

        if (loginPendingPulseRoutine != null)
        {
            StopCoroutine(loginPendingPulseRoutine);
            loginPendingPulseRoutine = null;
        }

        if (isPending && pulseLoginButtonWhilePending)
            loginPendingPulseRoutine = StartCoroutine(PulseLoginPendingTargets());
    }

    private void CacheLoginNormalTints()
    {
        if (loginPendingTintTargets == null || loginPendingTintTargets.Length == 0)
        {
            loginNormalTints = null;
            return;
        }

        loginNormalTints = new Color[loginPendingTintTargets.Length];
        for (int i = 0; i < loginPendingTintTargets.Length; i++)
            loginNormalTints[i] = loginPendingTintTargets[i] != null ? loginPendingTintTargets[i].color : Color.white;
    }

    private void ApplyPendingTints(bool isPending)
    {
        if (loginPendingTintTargets == null || loginNormalTints == null)
            return;

        for (int i = 0; i < loginPendingTintTargets.Length; i++)
        {
            Graphic target = loginPendingTintTargets[i];
            if (target == null)
                continue;

            target.color = isPending ? loginPendingTint : loginNormalTints[i];
        }
    }

    private IEnumerator PulseLoginPendingTargets()
    {
        if (loginPendingTintTargets == null || loginPendingTintTargets.Length == 0)
            yield break;

        float minAlpha = Mathf.Clamp01(pendingPulseMinAlpha);
        while (isLoginPending)
        {
            float pulse = Mathf.Lerp(minAlpha, loginPendingTint.a, Mathf.PingPong(Time.unscaledTime * pendingPulseSpeed, 1f));

            for (int i = 0; i < loginPendingTintTargets.Length; i++)
            {
                Graphic target = loginPendingTintTargets[i];
                if (target == null)
                    continue;

                Color color = loginPendingTint;
                color.a = pulse;
                target.color = color;
            }

            yield return null;
        }
    }

    private IEnumerator TransitionAuthenticatedState(bool isAuthenticated)
    {
        GameObject outgoingRoot = isAuthenticated ? loginRoot : authenticatedRoot;
        CanvasGroup outgoingGroup = isAuthenticated ? loginCanvasGroup : authenticatedCanvasGroup;
        GameObject incomingRoot = isAuthenticated ? authenticatedRoot : loginRoot;
        CanvasGroup incomingGroup = isAuthenticated ? authenticatedCanvasGroup : loginCanvasGroup;

        SetRootVisible(incomingRoot, incomingGroup, true, false);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, transitionDuration);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            SetCanvasAlpha(outgoingGroup, 1f - t);
            SetCanvasAlpha(incomingGroup, t);

            yield return null;
        }

        SetRootVisible(outgoingRoot, outgoingGroup, false, true);
        SetRootVisible(incomingRoot, incomingGroup, true, true);
        transitionRoutine = null;
    }

    private static void SetRootVisible(GameObject root, CanvasGroup canvasGroup, bool isVisible, bool setFinalAlpha)
    {
        if (root != null)
            root.SetActive(isVisible);

        if (canvasGroup == null)
            return;

        if (setFinalAlpha)
            canvasGroup.alpha = isVisible ? 1f : 0f;

        canvasGroup.interactable = isVisible;
        canvasGroup.blocksRaycasts = isVisible;
    }

    private static void SetCanvasAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
            canvasGroup.alpha = alpha;
    }
}
