using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuAuthGate : MonoBehaviour
{
    [SerializeField] private SupabaseAuthService authService;
    [SerializeField] private GameObject loginRoot;
    [SerializeField] private GameObject authenticatedRoot;
    [SerializeField] private GameObject warningOfflineRoot;
    [SerializeField] private GameObject playOfflineUi;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button playOfflineButton;
    [SerializeField] private Button confirmOfflineButton;
    [SerializeField] private Button cancelOfflineButton;
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
    private bool isOfflineSession;
    private bool hasAppliedInitialState;
    private bool? currentAuthenticatedState;
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

        if (playOfflineButton != null)
            playOfflineButton.onClick.AddListener(ShowOfflineWarning);

        if (confirmOfflineButton != null)
            confirmOfflineButton.onClick.AddListener(ConfirmOfflinePlay);

        if (cancelOfflineButton != null)
            cancelOfflineButton.onClick.AddListener(CancelOfflinePlay);

        if (signOutButton != null)
            signOutButton.onClick.AddListener(SignOut);

        ShowLoginImmediately();
        ApplyLoginPendingState(false);

        if (authService != null && authService.IsAuthenticated)
            StartCoroutine(ApplyAuthenticatedSessionAfterFirstFrame());
    }

    private void OnDisable()
    {
        if (authService != null)
            authService.AuthenticationChanged -= HandleAuthenticationChanged;

        if (loginButton != null)
            loginButton.onClick.RemoveListener(SignIn);

        if (playOfflineButton != null)
            playOfflineButton.onClick.RemoveListener(ShowOfflineWarning);

        if (confirmOfflineButton != null)
            confirmOfflineButton.onClick.RemoveListener(ConfirmOfflinePlay);

        if (cancelOfflineButton != null)
            cancelOfflineButton.onClick.RemoveListener(CancelOfflinePlay);

        if (signOutButton != null)
            signOutButton.onClick.RemoveListener(SignOut);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            Refresh();
    }

    public void Refresh()
    {
        bool isAuthenticated = authService != null && authService.IsAuthenticated;
        if (isAuthenticated)
        {
            isOfflineSession = false;
            ApplyAuthenticatedState(true);
            HideOfflineUi();
        }
        else if (isOfflineSession)
        {
            ApplyOfflineState();
        }
        else
        {
            ApplyAuthenticatedState(false);
            HideOfflineUi();
        }

        ApplyLoginPendingState(isLoginPending && !isAuthenticated);
        ApplySessionControls(isAuthenticated);
    }

    public void RefreshFromAuthService()
    {
        Refresh();
    }

    private void HandleAuthenticationChanged(bool isAuthenticated)
    {
        isLoginPending = false;
        if (isAuthenticated)
            isOfflineSession = false;

        ApplyAuthenticatedState(isAuthenticated);
        HideOfflineUi();
        ApplyLoginPendingState(false);
        ApplySessionControls(isAuthenticated);
    }

    private void ApplyAuthenticatedState(bool isAuthenticated)
    {
        if (currentAuthenticatedState.HasValue && currentAuthenticatedState.Value == isAuthenticated)
        {
            SetRootVisible(loginRoot, loginCanvasGroup, !isAuthenticated, true);
            SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, isAuthenticated, true);
            return;
        }

        if (!hasAppliedInitialState || transitionDuration <= 0f)
        {
            SetRootVisible(loginRoot, loginCanvasGroup, !isAuthenticated, true);
            SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, isAuthenticated, true);
            hasAppliedInitialState = true;
            currentAuthenticatedState = isAuthenticated;
            return;
        }

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionAuthenticatedState(isAuthenticated));
        currentAuthenticatedState = isAuthenticated;
    }

    private void SignIn()
    {
        if (authService == null)
            return;

        isOfflineSession = false;
        HideOfflineUi();
        isLoginPending = true;
        ApplyLoginPendingState(true);
        ApplySessionControls(false);
        authService.SignIn();
    }

    public void ShowOfflineWarning()
    {
        if (warningOfflineRoot == null)
        {
            Debug.LogWarning($"{nameof(MainMenuAuthGate)} cannot show offline warning because warningOfflineRoot is not assigned.");
            return;
        }

        if (playOfflineUi != null)
            playOfflineUi.SetActive(false);

        SetRootVisible(loginRoot, loginCanvasGroup, true, true);
        SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, false, true);
           
        warningOfflineRoot.SetActive(true);
    }


    public void FromOfflineToLogin()
    {
        isLoginPending = false;
        isOfflineSession = true;
        SetRootVisible(loginRoot, loginCanvasGroup, true, true);
        SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, false, true);
        SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, false, true);
        SetRootVisible(playOfflineUi, authenticatedCanvasGroup, false, true);
        
        HideOfflineUi();
        ApplySessionControls(false);

        
    }

    public void ConfirmOfflinePlay()
    {
           
        isLoginPending = false;
        isOfflineSession = true;
        ApplyLoginPendingState(false);
        ApplyOfflineState();
        ApplySessionControls(false);
    }

    public void CancelOfflinePlay()
    {
        isOfflineSession = false;
        HideOfflineUi();
        ApplyAuthenticatedState(false);
        ApplySessionControls(false);
    }

    private void ShowLoginImmediately()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        SetRootVisible(loginRoot, loginCanvasGroup, true, true);
        SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, false, true);
        HideOfflineUi();
        ApplySessionControls(false);
        hasAppliedInitialState = true;
        currentAuthenticatedState = false;
    }

    private IEnumerator ApplyAuthenticatedSessionAfterFirstFrame()
    {
        yield return null;

        if (authService != null && authService.IsAuthenticated)
            ApplyAuthenticatedState(true);
    }

    private void SignOut()
    {
        isLoginPending = false;
        isOfflineSession = false;
        HideOfflineUi();
        ApplyLoginPendingState(false);

        if (authService != null)
            authService.SignOut();
        else
            ApplyAuthenticatedState(false);
    }

    private void ApplyOfflineState()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (warningOfflineRoot != null)
            warningOfflineRoot.SetActive(false);

        SetRootVisible(loginRoot, loginCanvasGroup, true, true);
        SetRootVisible(authenticatedRoot, authenticatedCanvasGroup, false, true);

        if (playOfflineUi != null)
            playOfflineUi.SetActive(true);
        else
            Debug.LogWarning($"{nameof(MainMenuAuthGate)} cannot show offline play UI because playOfflineUi is not assigned.");

        if (loginRoot != null)
            loginRoot.SetActive(false);

        hasAppliedInitialState = true;
        currentAuthenticatedState = false;
    }

    private void HideOfflineUi()
    {
        if (warningOfflineRoot != null)
            warningOfflineRoot.SetActive(false);

        if (playOfflineUi != null)
            playOfflineUi.SetActive(false);
    }

    private void ApplySessionControls(bool isAuthenticated)
    {
        if (signOutButton != null)
            signOutButton.gameObject.SetActive(isAuthenticated);
    }

    private void ApplyLoginPendingState(bool isPending)
    {
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
