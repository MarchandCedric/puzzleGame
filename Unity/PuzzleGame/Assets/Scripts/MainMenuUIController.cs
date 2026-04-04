using System;
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
    private static readonly Vector2 MenuReferenceResolution = new Vector2(1080f, 1920f);
    private static readonly Color BackgroundColor = new Color(0.08f, 0.10f, 0.14f, 1f);
    private static readonly Color CardColor = new Color(0.12f, 0.15f, 0.20f, 0.96f);
    private static readonly Color AccentColor = new Color(0.93f, 0.66f, 0.20f, 1f);
    private static readonly Color SecondaryColor = new Color(0.24f, 0.30f, 0.38f, 1f);
    private static readonly Color MutedTextColor = new Color(0.84f, 0.88f, 0.92f, 1f);

    [Header("Brand")]
    [SerializeField] private string gameTitle = "PuzzleGame";
    [SerializeField] private string subtitle = "Working Title";

    [Header("Audio")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.6f;

    [Header("Levels")]
    [SerializeField] private List<MenuLevelDefinition> levels = new List<MenuLevelDefinition>();

    private const string MenuVolumeKey = "menu.music.volume";

    private AudioSource audioSource;
    private GameObject mainPage;
    private GameObject levelPage;
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

            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
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
            new MenuLevelDefinition(1, 1, 12, 16, 22, string.Empty),
            new MenuLevelDefinition(1, 2, 14, 18, 24, string.Empty),
            new MenuLevelDefinition(1, 3, 16, 20, 28, string.Empty),
            new MenuLevelDefinition(2, 1, 15, 20, 26, string.Empty),
            new MenuLevelDefinition(2, 2, 18, 24, 32, string.Empty),
            new MenuLevelDefinition(2, 3, 20, 27, 36, string.Empty)
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
        scaler.referenceResolution = MenuReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private Camera EnsureMainCamera()
    {
        Camera existingCamera = Camera.main;
        if (existingCamera != null)
            return existingCamera;

        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.04f, 0.06f, 0.09f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.nearClipPlane = 0.3f;
        camera.farClipPlane = 100f;
        return camera;
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

        EnsureMainCamera();
        Canvas canvas = EnsureCanvas();
        EnsureEventSystem();
        audioSource = EnsureAudioSource();

        BuildUi(canvas);
        RefreshLevelButtons();

        if (Application.isPlaying)
            PlayMusicIfConfigured();
    }

    private void BuildUi(Canvas canvas)
    {
        ClearRuntimeUi();

        RectTransform root = CreateUiObject("MenuRuntimeRoot", canvas.transform).GetComponent<RectTransform>();
        Stretch(root);

        Image background = root.gameObject.AddComponent<Image>();
        background.color = BackgroundColor;

        CreateBackgroundDecor(root);

        RectTransform safeArea = CreateUiObject("MenuRuntimeSafeArea", root).GetComponent<RectTransform>();
        Stretch(safeArea);
        safeArea.offsetMin = new Vector2(48f, 48f);
        safeArea.offsetMax = new Vector2(-48f, -48f);

        mainPage = CreatePage("MainPage", safeArea);
        levelPage = CreatePage("LevelPage", safeArea);

        BuildMainPage(mainPage.GetComponent<RectTransform>());
        BuildLevelPage(levelPage.GetComponent<RectTransform>());
        ShowPage(mainPage);
    }

    private void ClearRuntimeUi()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
            return;

        List<GameObject> runtimeChildren = new List<GameObject>();
        foreach (Transform child in canvas.transform)
        {
            if (child.name.StartsWith("MenuRuntime", StringComparison.Ordinal))
                runtimeChildren.Add(child.gameObject);
        }

        foreach (GameObject runtimeChild in runtimeChildren)
        {
            if (Application.isPlaying)
                Destroy(runtimeChild);
            else
                DestroyImmediate(runtimeChild);
        }
    }

    private void CreateBackgroundDecor(RectTransform root)
    {
        CreateDecorShape(root, new Vector2(0.10f, 0.18f), new Vector2(220f, 220f), new Color(0.93f, 0.66f, 0.20f, 0.16f));
        CreateDecorShape(root, new Vector2(0.88f, 0.84f), new Vector2(280f, 280f), new Color(0.20f, 0.48f, 0.80f, 0.14f));
        CreateDecorShape(root, new Vector2(0.84f, 0.12f), new Vector2(140f, 140f), new Color(1f, 1f, 1f, 0.07f));
    }

    private void CreateDecorShape(RectTransform parent, Vector2 anchor, Vector2 size, Color color)
    {
        RectTransform rect = CreateUiObject("MenuRuntimeDecor", parent).GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0f, 0f, 45f);

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private GameObject CreatePage(string name, RectTransform parent)
    {
        GameObject pageObject = CreateUiObject(name, parent);
        Stretch(pageObject.GetComponent<RectTransform>());
        return pageObject;
    }

    private void BuildMainPage(RectTransform page)
    {
        VerticalLayoutGroup layout = page.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 80, 56);
        layout.spacing = 28;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddFlexibleSpacer(page, 1f);

        RectTransform titleCard = CreateCenteredCard(page, "MainTitleCard", 780f, 420f);
        VerticalLayoutGroup titleLayout = titleCard.gameObject.AddComponent<VerticalLayoutGroup>();
        titleLayout.padding = new RectOffset(56, 56, 56, 56);
        titleLayout.spacing = 18;
        titleLayout.childAlignment = TextAnchor.UpperCenter;
        titleLayout.childControlWidth = true;
        titleLayout.childControlHeight = false;
        titleLayout.childForceExpandWidth = true;
        titleLayout.childForceExpandHeight = false;

        CreateLayoutLabel(titleCard, "Kicker", "PUZZLE PROTOTYPE", 28, FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor, 40f);
        CreateLayoutLabel(titleCard, "Title", gameTitle, 96, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, 110f);
        CreateLayoutLabel(titleCard, "Subtitle", subtitle, 34, FontStyle.Normal, TextAnchor.MiddleCenter, MutedTextColor, 44f);
        CreateLayoutLabel(titleCard, "Body", "A clean main menu with a separate level selection screen.", 28, FontStyle.Normal, TextAnchor.MiddleCenter, MutedTextColor, 76f);

        RectTransform actionsCard = CreateCenteredCard(page, "MainActionsCard", 780f, 320f);
        VerticalLayoutGroup actionsLayout = actionsCard.gameObject.AddComponent<VerticalLayoutGroup>();
        actionsLayout.padding = new RectOffset(56, 56, 48, 48);
        actionsLayout.spacing = 18;
        actionsLayout.childAlignment = TextAnchor.MiddleCenter;
        actionsLayout.childControlWidth = true;
        actionsLayout.childControlHeight = false;
        actionsLayout.childForceExpandWidth = true;
        actionsLayout.childForceExpandHeight = false;

        CreateLayoutLabel(actionsCard, "ActionTitle", "Main Menu", 38, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, 48f);
        CreateMenuButton(actionsCard, "PlayButton", "Play", "Open the level selection screen.", AccentColor, () => ShowPage(levelPage), 96f);
        CreateMenuButton(actionsCard, "QuitButton", "Quit", "Close the prototype.", SecondaryColor, QuitGame, 96f);

        CreateLayoutLabel(page, "FooterHint", "Tap Play to move to the level menu.", 24, FontStyle.Normal, TextAnchor.MiddleCenter, MutedTextColor, 36f);
        AddFlexibleSpacer(page, 1f);
    }

    private void BuildLevelPage(RectTransform page)
    {
        VerticalLayoutGroup layout = page.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(0, 0, 36, 36);
        layout.spacing = 24;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        RectTransform headerCard = CreateCenteredCard(page, "LevelHeaderCard", 900f, 220f);
        VerticalLayoutGroup headerLayout = headerCard.gameObject.AddComponent<VerticalLayoutGroup>();
        headerLayout.padding = new RectOffset(40, 40, 32, 32);
        headerLayout.spacing = 12;
        headerLayout.childAlignment = TextAnchor.UpperCenter;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = false;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;

        CreateMenuButton(headerCard, "BackButton", "Back", "Return to the main menu.", SecondaryColor, () => ShowPage(mainPage), 82f);
        CreateLayoutLabel(headerCard, "LevelTitle", "Level Select", 56, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, 64f);
        CreateLayoutLabel(headerCard, "LevelSubtitle", "Choose a stage below.", 26, FontStyle.Normal, TextAnchor.MiddleCenter, MutedTextColor, 36f);

        RectTransform listCard = CreateCenteredCard(page, "LevelListCard", 900f, 1420f);
        VerticalLayoutGroup listCardLayout = listCard.gameObject.AddComponent<VerticalLayoutGroup>();
        listCardLayout.padding = new RectOffset(32, 32, 32, 32);
        listCardLayout.spacing = 18;
        listCardLayout.childAlignment = TextAnchor.UpperCenter;
        listCardLayout.childControlWidth = true;
        listCardLayout.childControlHeight = true;
        listCardLayout.childForceExpandWidth = true;
        listCardLayout.childForceExpandHeight = false;

        CreateLayoutLabel(listCard, "ListHint", "Unlocked levels can be opened immediately.", 24, FontStyle.Normal, TextAnchor.MiddleCenter, MutedTextColor, 36f);
        RectTransform levelListContent = CreateScrollContent(listCard, new Vector2(836f, 1290f));

        int levelIndex = 0;
        int currentWorld = int.MinValue;
        RectTransform worldSection = null;

        for (int index = 0; index < levels.Count; index++)
        {
            MenuLevelDefinition level = levels[index];
            if (level.World != currentWorld)
            {
                currentWorld = level.World;
                worldSection = CreateWorldSection(levelListContent, currentWorld);
            }

            CreateLevelButton(worldSection, level, levelIndex);
            levelIndex++;
        }
    }

    private void RefreshLevelButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            if (!button.name.StartsWith("LevelButton_", StringComparison.Ordinal))
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
                buttonImage.color = unlocked ? new Color(0.11f, 0.18f, 0.24f, 0.98f) : new Color(0.10f, 0.11f, 0.13f, 0.96f);

            Text[] texts = button.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                switch (text.name)
                {
                    case "BadgeText":
                        text.text = $"W{level.World}";
                        break;
                    case "Title":
                        text.text = $"Stage {level.Level}";
                        break;
                    case "Thresholds":
                        text.text = $"3* {level.ThreeStarMoves}   2* {level.TwoStarMoves}   1* {level.OneStarMoves}";
                        break;
                    case "Result":
                        text.text = unlocked
                            ? (result.IsCompleted ? $"Best: {GetStarString(result.Stars)}  {result.BestMoves} moves" : "Not cleared yet")
                            : "Locked";
                        break;
                }
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

        Debug.Log($"Level selected: World {level.World}-{level.Level}. Assign a scene path to load gameplay.");
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit pressed. In the Unity Editor this is a no-op.");
#else
        Application.Quit();
#endif
    }

    private void ShowPage(GameObject pageToShow)
    {
        if (mainPage != null)
            mainPage.SetActive(pageToShow == mainPage);

        if (levelPage != null)
            levelPage.SetActive(pageToShow == levelPage);
    }

    private void CreateLevelButton(RectTransform parent, MenuLevelDefinition level, int index)
    {
        GameObject buttonObject = CreateUiObject($"LevelButton_{index}", parent);
        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 780f;
        layoutElement.preferredHeight = 150f;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.11f, 0.18f, 0.24f, 0.98f);
        AddShadow(image, new Color(0f, 0f, 0f, 0.20f), new Vector2(0f, -6f));

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => OnLevelPressed(index));

        HorizontalLayoutGroup rowLayout = buttonObject.AddComponent<HorizontalLayoutGroup>();
        rowLayout.padding = new RectOffset(28, 28, 20, 20);
        rowLayout.spacing = 18;
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childControlHeight = true;
        rowLayout.childControlWidth = false;
        rowLayout.childForceExpandHeight = false;
        rowLayout.childForceExpandWidth = false;

        RectTransform badge = CreateFixedChip(buttonObject.transform, "Badge", 92f, 92f, AccentColor);
        CreateCenteredText(badge, "BadgeText", $"W{level.World}", 24, FontStyle.Bold, new Color(0.12f, 0.14f, 0.16f));

        RectTransform textColumn = CreateLayoutContainer(buttonObject.transform, "TextColumn");
        LayoutElement textColumnLayout = textColumn.gameObject.AddComponent<LayoutElement>();
        textColumnLayout.preferredWidth = 470f;
        textColumnLayout.flexibleWidth = 1f;
        VerticalLayoutGroup textLayout = textColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        textLayout.spacing = 8;
        textLayout.childAlignment = TextAnchor.MiddleLeft;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = false;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = false;
        CreateLayoutLabel(textColumn, "Title", $"Stage {level.Level}", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, 40f);
        CreateLayoutLabel(textColumn, "Thresholds", $"3* {level.ThreeStarMoves}   2* {level.TwoStarMoves}   1* {level.OneStarMoves}", 22, FontStyle.Normal, TextAnchor.MiddleLeft, AccentColor, 32f);
        CreateLayoutLabel(textColumn, "Result", "Not cleared yet", 20, FontStyle.Normal, TextAnchor.MiddleLeft, MutedTextColor, 28f);

        RectTransform actionColumn = CreateLayoutContainer(buttonObject.transform, "ActionColumn");
        LayoutElement actionLayout = actionColumn.gameObject.AddComponent<LayoutElement>();
        actionLayout.preferredWidth = 150f;
        VerticalLayoutGroup actionColumnLayout = actionColumn.gameObject.AddComponent<VerticalLayoutGroup>();
        actionColumnLayout.childAlignment = TextAnchor.MiddleCenter;
        actionColumnLayout.childControlWidth = true;
        actionColumnLayout.childControlHeight = false;
        actionColumnLayout.childForceExpandWidth = true;
        actionColumnLayout.childForceExpandHeight = true;
        CreateLayoutLabel(actionColumn, "ActionHint", "Tap to enter", 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, 34f);
    }

    private RectTransform CreateScrollContent(RectTransform parent, Vector2 size)
    {
        RectTransform viewportRect = CreateUiObject("Viewport", parent).GetComponent<RectTransform>();
        LayoutElement viewportLayout = viewportRect.gameObject.AddComponent<LayoutElement>();
        viewportLayout.preferredWidth = size.x;
        viewportLayout.preferredHeight = size.y;

        Image viewportImage = viewportRect.gameObject.AddComponent<Image>();
        viewportImage.color = new Color(0f, 0f, 0f, 0.02f);
        viewportRect.gameObject.AddComponent<RectMask2D>();

        RectTransform contentRect = CreateUiObject("Content", viewportRect).GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = Vector2.zero;
        contentRect.anchoredPosition = Vector2.zero;

        VerticalLayoutGroup contentLayout = contentRect.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(0, 0, 0, 24);
        contentLayout.spacing = 26f;
        contentLayout.childAlignment = TextAnchor.UpperCenter;
        contentLayout.childControlHeight = true;
        contentLayout.childControlWidth = true;
        contentLayout.childForceExpandHeight = false;
        contentLayout.childForceExpandWidth = true;

        ContentSizeFitter contentFitter = contentRect.gameObject.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        ScrollRect scrollRect = parent.gameObject.AddComponent<ScrollRect>();
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 32f;

        return contentRect;
    }

    private RectTransform CreateWorldSection(RectTransform parent, int world)
    {
        RectTransform section = CreateLayoutContainer(parent, $"World_{world}");
        VerticalLayoutGroup sectionLayout = section.gameObject.AddComponent<VerticalLayoutGroup>();
        sectionLayout.spacing = 14f;
        sectionLayout.childAlignment = TextAnchor.UpperCenter;
        sectionLayout.childControlHeight = true;
        sectionLayout.childControlWidth = true;
        sectionLayout.childForceExpandHeight = false;
        sectionLayout.childForceExpandWidth = true;

        ContentSizeFitter fitter = section.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        RectTransform header = CreateCenteredCard(section, $"World_{world}_Header", 840f, 88f);
        LayoutElement headerLayout = header.gameObject.AddComponent<LayoutElement>();
        headerLayout.preferredWidth = 840f;
        headerLayout.preferredHeight = 88f;
        HorizontalLayoutGroup headerRow = header.gameObject.AddComponent<HorizontalLayoutGroup>();
        headerRow.padding = new RectOffset(28, 28, 18, 18);
        headerRow.spacing = 16;
        headerRow.childAlignment = TextAnchor.MiddleLeft;
        headerRow.childControlWidth = false;
        headerRow.childControlHeight = false;
        headerRow.childForceExpandWidth = false;
        headerRow.childForceExpandHeight = false;
        CreateLayoutLabel(header, "WorldLabel", $"World {world}", 32, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, 40f, 200f);
        CreateLayoutLabel(header, "WorldHint", "Clear stages to unlock what comes next.", 20, FontStyle.Normal, TextAnchor.MiddleLeft, MutedTextColor, 30f, 520f);

        RectTransform list = CreateLayoutContainer(section, $"World_{world}_Levels");
        VerticalLayoutGroup listLayout = list.gameObject.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 14f;
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlHeight = true;
        listLayout.childControlWidth = true;
        listLayout.childForceExpandHeight = false;
        listLayout.childForceExpandWidth = true;

        ContentSizeFitter listFitter = list.gameObject.AddComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        listFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        return list;
    }

    private RectTransform CreateCenteredCard(Transform parent, string name, float width, float height)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
        layout.flexibleWidth = 0f;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = CardColor;
        AddShadow(image, new Color(0f, 0f, 0f, 0.22f), new Vector2(0f, -8f));
        return rect;
    }

    private RectTransform CreateLayoutContainer(Transform parent, string name)
    {
        return CreateUiObject(name, parent).GetComponent<RectTransform>();
    }

    private void AddFlexibleSpacer(RectTransform parent, float flexibleHeight)
    {
        RectTransform spacer = CreateLayoutContainer(parent, "Spacer");
        LayoutElement layout = spacer.gameObject.AddComponent<LayoutElement>();
        layout.flexibleHeight = flexibleHeight;
    }

    private Text CreateLayoutLabel(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color, float preferredHeight)
    {
        return CreateLayoutLabel(parent, name, text, fontSize, fontStyle, alignment, color, preferredHeight, -1f);
    }

    private Text CreateLayoutLabel(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color, float preferredHeight, float preferredWidth)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = preferredHeight;
        if (preferredWidth > 0f)
            layout.preferredWidth = preferredWidth;

        Text label = rect.gameObject.AddComponent<Text>();
        label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        AddShadow(label, new Color(0f, 0f, 0f, 0.24f), new Vector2(0f, -2f));
        return label;
    }

    private Button CreateMenuButton(Transform parent, string name, string label, string detail, Color color, UnityEngine.Events.UnityAction onClick, float height)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        AddShadow(image, new Color(0f, 0f, 0f, 0.22f), new Vector2(0f, -6f));

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        VerticalLayoutGroup contentLayout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        contentLayout.padding = new RectOffset(24, 24, 14, 14);
        contentLayout.spacing = 4;
        contentLayout.childAlignment = TextAnchor.MiddleCenter;
        contentLayout.childControlWidth = true;
        contentLayout.childControlHeight = false;
        contentLayout.childForceExpandWidth = true;
        contentLayout.childForceExpandHeight = false;

        CreateLayoutLabel(rect, "Label", label, 34, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, 40f);
        CreateLayoutLabel(rect, "Detail", detail, 18, FontStyle.Normal, TextAnchor.MiddleCenter, new Color(0.95f, 0.97f, 0.99f, 0.94f), 24f);
        return button;
    }

    private RectTransform CreateFixedChip(Transform parent, string name, float width, float height, Color color)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        LayoutElement layout = rect.gameObject.AddComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.preferredHeight = height;
        layout.minWidth = width;

        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        AddShadow(image, new Color(0f, 0f, 0f, 0.18f), new Vector2(0f, -4f));
        return rect;
    }

    private Text CreateCenteredText(RectTransform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color)
    {
        RectTransform rect = CreateUiObject(name, parent).GetComponent<RectTransform>();
        Stretch(rect);

        Text label = rect.gameObject.AddComponent<Text>();
        label.font = uiFont;
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = color;
        return label;
    }

    private static string GetStarString(int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        return new string('*', stars) + new string('-', 3 - stars);
    }

    private static void AddShadow(Graphic graphic, Color color, Vector2 distance)
    {
        Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
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
