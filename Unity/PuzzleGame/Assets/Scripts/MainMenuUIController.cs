using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class MainMenuUIController : MonoBehaviour
{
    [Header("Brand")]
    [SerializeField] private string gameTitle = "PuzzleGame";
    [SerializeField] private string subtitle = "Working Title";

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic = null;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.6f;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private float backgroundDriftAmplitude = 90f;
    [SerializeField] private float backgroundDriftSpeed = 0.35f;

    [Header("Levels")]
    [SerializeField] private List<MenuLevelDefinition> levels = new List<MenuLevelDefinition>();

    private const string MenuVolumeKey = "menu.music.volume";

    private AudioSource audioSource;
    private RectTransform pageTrack;
    private RectTransform mainPage;
    private RectTransform levelPage;
    private RectTransform settingsPage;
    private readonly List<RectTransform> floatingShapes = new List<RectTransform>();
    private readonly List<Vector2> floatingBasePositions = new List<Vector2>();
    private readonly List<Image> floatingImages = new List<Image>();
    private Coroutine slideRoutine;
    private MenuProgressStore progressStore;
    private Font uiFont;

    private void OnEnable()
    {
        RebuildForCurrentMode();
    }

    private void Awake()
    {
        RebuildForCurrentMode();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncLevelsFromScenes();
        RebuildForCurrentMode();
    }

    [ContextMenu("Sync Levels From Scenes")]
    private void SyncLevelsFromScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        string activeScenePath = gameObject.scene.path;
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        List<MenuLevelDefinition> syncedLevels = new List<MenuLevelDefinition>();
        string originalScenePath = activeScenePath;

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.Equals(scenePath, activeScenePath, StringComparison.OrdinalIgnoreCase))
                continue;

            UnityEditor.SceneManagement.SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            UnityEngine.SceneManagement.Scene openedScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            LevelSceneMetadata metadata = null;
            foreach (GameObject root in openedScene.GetRootGameObjects())
            {
                metadata = root.GetComponentInChildren<LevelSceneMetadata>(true);
                if (metadata != null)
                    break;
            }

            if (metadata != null)
            {
                syncedLevels.Add(new MenuLevelDefinition(
                    metadata.World,
                    metadata.Level,
                    metadata.ThreeStarMoves,
                    metadata.TwoStarMoves,
                    metadata.OneStarMoves,
                    scenePath));
            }

            EditorSceneManager.CloseScene(openedScene, true);
            EditorSceneManager.RestoreSceneManagerSetup(setup);
        }

        syncedLevels.Sort((left, right) =>
        {
            int worldCompare = left.World.CompareTo(right.World);
            return worldCompare != 0 ? worldCompare : left.Level.CompareTo(right.Level);
        });

        if (syncedLevels.Count > 0)
            levels = syncedLevels;

        if (!string.IsNullOrWhiteSpace(originalScenePath))
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
    }
#endif

    private void Update()
    {
        AnimateBackground();
    }

    [ContextMenu("Clear Menu Progress")]
    private void ClearMenuProgress()
    {
        new MenuProgressStore().Clear(levels);
    }

    private void EnsureDefaultLevels()
    {
        if (levels.Count > 0)
            return;

        levels = new List<MenuLevelDefinition>
        {
            new MenuLevelDefinition(1, 1, 12, 16, 22, ""),
            new MenuLevelDefinition(1, 2, 14, 18, 24, ""),
            new MenuLevelDefinition(1, 3, 16, 20, 28, ""),
            new MenuLevelDefinition(2, 1, 15, 20, 26, ""),
            new MenuLevelDefinition(2, 2, 18, 24, 32, ""),
            new MenuLevelDefinition(2, 3, 20, 27, 36, "")
        };
    }

    private Canvas EnsureCanvas()
    {
        Canvas existingCanvas = FindAnyObjectByType<Canvas>();
        if (existingCanvas != null)
            return existingCanvas;

        GameObject canvasObject = new GameObject("MenuCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetAsLastSibling();
    }

    private AudioSource EnsureAudioSource()
    {
        AudioSource source = GetComponent<AudioSource>();
        if (source == null)
            source = gameObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.loop = true;
        source.volume = Application.isPlaying ? PlayerPrefs.GetFloat(MenuVolumeKey, musicVolume) : musicVolume;
        return source;
    }

    private void PlayMusicIfConfigured()
    {
        if (menuMusic == null || audioSource == null)
            return;

        audioSource.clip = menuMusic;
        audioSource.volume = PlayerPrefs.GetFloat(MenuVolumeKey, musicVolume);
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    private void RebuildForCurrentMode()
    {
        EnsureDefaultLevels();

        progressStore = new MenuProgressStore();
        uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        audioSource = EnsureAudioSource();

        floatingShapes.Clear();
        floatingBasePositions.Clear();
        floatingImages.Clear();

        BuildUi(canvas);
        RefreshLevelButtons();

        if (Application.isPlaying)
            PlayMusicIfConfigured();
    }

    private void BuildUi(Canvas canvas)
    {
        ClearRuntimeUi();

        GameObject root = CreateUiObject("MenuRuntimeRoot", canvas.transform);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        Stretch(rootRect);

        Image background = root.AddComponent<Image>();
        background.color = new Color(0.08f, 0.11f, 0.15f, 1f);

        CreateAnimatedBackground(rootRect);

        GameObject viewport = CreateUiObject("MenuRuntimeViewport", root.transform);
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        Stretch(viewportRect);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        GameObject track = CreateUiObject("MenuRuntimePageTrack", viewport.transform);
        pageTrack = track.GetComponent<RectTransform>();
        pageTrack.anchorMin = Vector2.zero;
        pageTrack.anchorMax = Vector2.one;
        pageTrack.offsetMin = Vector2.zero;
        pageTrack.offsetMax = new Vector2(2160f, 0f);
        pageTrack.pivot = new Vector2(0f, 0.5f);
        pageTrack.anchoredPosition = Vector2.zero;

        mainPage = CreatePage("MainPage", pageTrack, 0f);
        levelPage = CreatePage("LevelPage", pageTrack, 1080f);
        settingsPage = CreatePage("SettingsPage", pageTrack, 2160f);

        BuildMainPage(mainPage);
        BuildLevelPage(levelPage);
        BuildSettingsPage(settingsPage);
    }

    private void ClearRuntimeUi()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        List<GameObject> runtimeChildren = new List<GameObject>();
        foreach (Transform child in canvas.transform)
        {
            if (child.name.StartsWith("MenuRuntime"))
                runtimeChildren.Add(child.gameObject);
        }

        foreach (GameObject runtimeChild in runtimeChildren)
        {
            if (Application.isPlaying)
                Destroy(runtimeChild);
            else
                DestroyImmediate(runtimeChild);
        }

        floatingShapes.Clear();
        floatingBasePositions.Clear();
        floatingImages.Clear();
    }

    private void CreateAnimatedBackground(RectTransform root)
    {
        CreateFloatingShape(root, new Vector2(140f, 320f), new Vector2(420f, 420f), new Color(0.95f, 0.55f, 0.18f, 0.18f));
        CreateFloatingShape(root, new Vector2(860f, 1560f), new Vector2(560f, 560f), new Color(0.18f, 0.65f, 0.58f, 0.16f));
        CreateFloatingShape(root, new Vector2(180f, 1460f), new Vector2(300f, 300f), new Color(0.96f, 0.87f, 0.42f, 0.14f));
    }

    private void CreateFloatingShape(RectTransform parent, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject shape = CreateUiObject("MenuRuntimeShape", parent);
        RectTransform rect = shape.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = shape.AddComponent<Image>();
        image.color = color;

        floatingShapes.Add(rect);
        floatingBasePositions.Add(anchoredPosition);
        floatingImages.Add(image);
    }

    private void AnimateBackground()
    {
        if (floatingShapes.Count == 0)
            return;

        for (int index = 0; index < floatingShapes.Count; index++)
        {
            float time = Time.unscaledTime * backgroundDriftSpeed + index;
            RectTransform rect = floatingShapes[index];
            Vector2 basePosition = floatingBasePositions[index];
            rect.anchoredPosition = basePosition + new Vector2(Mathf.Sin(time) * backgroundDriftAmplitude, Mathf.Cos(time * 1.2f) * backgroundDriftAmplitude);

            Color color = floatingImages[index].color;
            color.a = 0.12f + (Mathf.Sin(time * 0.85f) + 1f) * 0.04f;
            floatingImages[index].color = color;
        }
    }

    private RectTransform CreatePage(string name, RectTransform parent, float anchoredX)
    {
        GameObject pageObject = CreateUiObject(name, parent);
        RectTransform rect = pageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = new Vector2(1080f, 0f);
        rect.anchoredPosition = new Vector2(anchoredX, 0f);
        return rect;
    }

    private void BuildMainPage(RectTransform page)
    {
        CreateLabel(page, "Title", gameTitle, 104, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(540f, 1450f), new Vector2(900f, 120f), Color.white);
        CreateLabel(page, "Subtitle", subtitle, 38, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(540f, 1360f), new Vector2(700f, 60f), new Color(0.93f, 0.82f, 0.63f));

        RectTransform card = CreateCard(page, "MenuCard", new Vector2(540f, 860f), new Vector2(700f, 540f));
        CreateButton(card, "PlayButton", "Play", new Vector2(0f, 120f), new Vector2(420f, 90f), new Color(0.93f, 0.66f, 0.18f), () => SlideTo(levelPage));
        CreateButton(card, "SettingsButton", "Settings", new Vector2(0f, 0f), new Vector2(420f, 90f), new Color(0.20f, 0.55f, 0.55f), () => SlideTo(settingsPage));
        CreateButton(card, "QuitButton", "Quit", new Vector2(0f, -120f), new Vector2(420f, 90f), new Color(0.30f, 0.32f, 0.38f), QuitGame);

        CreateLabel(page, "Hint", "Move efficiently to earn stars and unlock the next level.", 28, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(540f, 240f), new Vector2(760f, 80f), new Color(0.87f, 0.89f, 0.92f));
    }

    private void BuildLevelPage(RectTransform page)
    {
        CreateLabel(page, "LevelTitle", "Select Level", 82, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(540f, 1680f), new Vector2(800f, 100f), Color.white);
        CreateLabel(page, "LevelSubtitle", "Earn at least one star to unlock the next level.", 28, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(540f, 1605f), new Vector2(860f, 50f), new Color(0.88f, 0.90f, 0.93f));
        CreateButton(page, "BackButton", "Back", new Vector2(150f, 1760f), new Vector2(180f, 70f), new Color(0.25f, 0.28f, 0.35f), () => SlideTo(mainPage));

        RectTransform listRoot = CreateCard(page, "LevelListCard", new Vector2(540f, 860f), new Vector2(860f, 1180f));
        GridLayoutGroup grid = listRoot.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(380f, 170f);
        grid.spacing = new Vector2(24f, 24f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;
        grid.padding = new RectOffset(32, 32, 32, 32);

        for (int index = 0; index < levels.Count; index++)
        {
            CreateLevelButton(listRoot, levels[index], index);
        }
    }

    private void BuildSettingsPage(RectTransform page)
    {
        CreateLabel(page, "SettingsTitle", "Settings", 82, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(540f, 1680f), new Vector2(700f, 100f), Color.white);
        CreateButton(page, "SettingsBackButton", "Back", new Vector2(150f, 1760f), new Vector2(180f, 70f), new Color(0.25f, 0.28f, 0.35f), () => SlideTo(mainPage));

        RectTransform card = CreateCard(page, "SettingsCard", new Vector2(540f, 920f), new Vector2(760f, 480f));
        CreateLabel(card, "MusicLabel", "Music Volume", 34, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0f, 120f), new Vector2(420f, 50f), Color.white);
        Slider slider = CreateSlider(card, "MusicSlider", new Vector2(0f, 20f), new Vector2(480f, 40f));
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = PlayerPrefs.GetFloat(MenuVolumeKey, musicVolume);
        slider.onValueChanged.AddListener(SetMusicVolume);

        CreateLabel(card, "SettingsInfo", "More settings can plug in here later without changing the menu flow.", 26, FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, -110f), new Vector2(540f, 90f), new Color(0.86f, 0.88f, 0.92f));
    }

    private void RefreshLevelButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (!button.name.StartsWith("LevelButton_"))
                continue;

            string[] tokens = button.name.Split('_');
            if (tokens.Length < 2 || !int.TryParse(tokens[1], out int index))
                continue;

            MenuLevelDefinition level = levels[index];
            bool unlocked = progressStore.IsUnlocked(levels, index);
            MenuLevelResult result = progressStore.GetResult(level);

            button.interactable = unlocked;

            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
                buttonImage.color = unlocked ? new Color(0.20f, 0.24f, 0.31f) : new Color(0.14f, 0.15f, 0.18f);

            Text[] texts = button.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                if (text.name == "Title")
                    text.text = $"World {level.World} - {level.Level}";
                else if (text.name == "Thresholds")
                    text.text = $"3* {level.ThreeStarMoves}  2* {level.TwoStarMoves}  1* {level.OneStarMoves}";
                else if (text.name == "Result")
                    text.text = unlocked
                        ? (result.IsCompleted ? $"Best: {result.Stars} star(s) in {result.BestMoves} moves" : "Unplayed")
                        : "Locked";
            }
        }
    }

    private void OnLevelPressed(int index)
    {
        MenuLevelDefinition level = levels[index];
        if (!progressStore.IsUnlocked(levels, index))
            return;

        if (!string.IsNullOrWhiteSpace(level.ScenePath))
        {
            SceneManager.LoadScene(level.ScenePath);
            return;
        }

        Debug.Log($"Level selected: World {level.World}-{level.Level}. Assign a scene name later to load gameplay.");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit pressed. In the Unity Editor this is a no-op.");
#else
        Application.Quit();
#endif
    }

    private void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat(MenuVolumeKey, value);
        PlayerPrefs.Save();

        if (audioSource != null)
            audioSource.volume = value;
    }

    private void SlideTo(RectTransform targetPage)
    {
        float targetX = -targetPage.anchoredPosition.x;

        if (slideRoutine != null)
            StopCoroutine(slideRoutine);

        slideRoutine = StartCoroutine(SlidePageTrack(targetX));
    }

    private IEnumerator SlidePageTrack(float targetX)
    {
        Vector2 start = pageTrack.anchoredPosition;
        Vector2 end = new Vector2(targetX, start.y);
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            pageTrack.anchoredPosition = Vector2.Lerp(start, end, eased);
            yield return null;
        }

        pageTrack.anchoredPosition = end;
        slideRoutine = null;
    }

    private RectTransform CreateCard(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject card = CreateUiObject(name, parent);
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = card.AddComponent<Image>();
        image.color = new Color(0.10f, 0.14f, 0.18f, 0.92f);

        return rect;
    }

    private Button CreateButton(RectTransform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        CreateLabel(rect, "Label", label, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, size, Color.white);
        return button;
    }

    private void CreateLevelButton(RectTransform parent, MenuLevelDefinition level, int index)
    {
        GameObject buttonObject = CreateUiObject($"LevelButton_{index}", parent);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.20f, 0.24f, 0.31f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => OnLevelPressed(index));

        VerticalLayoutGroup layout = buttonObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(18, 18, 18, 18);
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        ContentSizeFitter fitter = buttonObject.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        CreateLabel(buttonObject.GetComponent<RectTransform>(), "Title", $"World {level.World} - {level.Level}", 28, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(320f, 36f), Color.white);
        CreateLabel(buttonObject.GetComponent<RectTransform>(), "Thresholds", $"3* {level.ThreeStarMoves}  2* {level.TwoStarMoves}  1* {level.OneStarMoves}", 22, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(320f, 32f), new Color(0.94f, 0.85f, 0.55f));
        CreateLabel(buttonObject.GetComponent<RectTransform>(), "Result", "Unplayed", 20, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(320f, 28f), new Color(0.86f, 0.89f, 0.93f));
    }

    private Slider CreateSlider(RectTransform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject sliderObject = CreateUiObject(name, parent);
        RectTransform rect = sliderObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Slider slider = sliderObject.AddComponent<Slider>();

        GameObject background = CreateUiObject("Background", rect);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        Stretch(backgroundRect);
        background.AddComponent<Image>().color = new Color(0.19f, 0.20f, 0.24f, 1f);

        GameObject fillArea = CreateUiObject("Fill Area", rect);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(16f, 0f);
        fillAreaRect.offsetMax = new Vector2(-16f, 0f);

        GameObject fill = CreateUiObject("Fill", fillAreaRect);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        Stretch(fillRect);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.95f, 0.67f, 0.18f, 1f);

        GameObject handleSlideArea = CreateUiObject("Handle Slide Area", rect);
        RectTransform handleAreaRect = handleSlideArea.GetComponent<RectTransform>();
        Stretch(handleAreaRect);
        handleAreaRect.offsetMin = new Vector2(16f, 0f);
        handleAreaRect.offsetMax = new Vector2(-16f, 0f);

        GameObject handle = CreateUiObject("Handle", handleAreaRect);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(36f, 36f);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.98f, 0.96f, 0.90f, 1f);

        slider.targetGraphic = handleImage;
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private Text CreateLabel(RectTransform parent, string name, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        GameObject labelObject = CreateUiObject(name, parent);
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Text label = labelObject.AddComponent<Text>();
        label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        return label;
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}

[Serializable]
public class MenuLevelDefinition
{
    public int World;
    public int Level;
    public int ThreeStarMoves;
    public int TwoStarMoves;
    public int OneStarMoves;
    public string ScenePath;

    public MenuLevelDefinition(int world, int level, int threeStarMoves, int twoStarMoves, int oneStarMoves, string scenePath)
    {
        World = world;
        Level = level;
        ThreeStarMoves = threeStarMoves;
        TwoStarMoves = twoStarMoves;
        OneStarMoves = oneStarMoves;
        ScenePath = scenePath;
    }

    public string Id => $"{World}-{Level}";
}

public readonly struct MenuLevelResult
{
    public MenuLevelResult(bool isCompleted, int stars, int bestMoves)
    {
        IsCompleted = isCompleted;
        Stars = stars;
        BestMoves = bestMoves;
    }

    public bool IsCompleted { get; }
    public int Stars { get; }
    public int BestMoves { get; }
}

public class MenuProgressStore
{
    public MenuLevelResult GetResult(MenuLevelDefinition level)
    {
        string completionKey = CompletionKey(level);
        if (!PlayerPrefs.HasKey(completionKey))
            return new MenuLevelResult(false, 0, 0);

        int stars = PlayerPrefs.GetInt(StarsKey(level), 0);
        int moves = PlayerPrefs.GetInt(MovesKey(level), 0);
        return new MenuLevelResult(true, stars, moves);
    }

    public void SaveResult(MenuLevelDefinition level, int moves)
    {
        int stars = EvaluateStars(level, moves);
        MenuLevelResult existing = GetResult(level);

        int bestStars = Mathf.Max(existing.Stars, stars);
        int bestMoves = existing.IsCompleted && existing.BestMoves > 0 ? Mathf.Min(existing.BestMoves, moves) : moves;

        PlayerPrefs.SetInt(CompletionKey(level), 1);
        PlayerPrefs.SetInt(StarsKey(level), bestStars);
        PlayerPrefs.SetInt(MovesKey(level), bestMoves);
        PlayerPrefs.Save();
    }

    public bool IsUnlocked(List<MenuLevelDefinition> levels, int index)
    {
        if (index <= 0)
            return true;

        MenuLevelResult previous = GetResult(levels[index - 1]);
        return previous.IsCompleted && previous.Stars >= 1;
    }

    public void Clear(List<MenuLevelDefinition> levels)
    {
        foreach (MenuLevelDefinition level in levels)
        {
            PlayerPrefs.DeleteKey(CompletionKey(level));
            PlayerPrefs.DeleteKey(StarsKey(level));
            PlayerPrefs.DeleteKey(MovesKey(level));
        }

        PlayerPrefs.Save();
    }

    private static int EvaluateStars(MenuLevelDefinition level, int moves)
    {
        if (moves <= level.ThreeStarMoves)
            return 3;
        if (moves <= level.TwoStarMoves)
            return 2;
        if (moves <= level.OneStarMoves)
            return 1;
        return 0;
    }

    private static string CompletionKey(MenuLevelDefinition level) => $"menu.level.{level.Id}.completed";
    private static string StarsKey(MenuLevelDefinition level) => $"menu.level.{level.Id}.stars";
    private static string MovesKey(MenuLevelDefinition level) => $"menu.level.{level.Id}.moves";
}
