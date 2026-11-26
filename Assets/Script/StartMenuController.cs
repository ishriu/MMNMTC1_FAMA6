using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class StartMenuController : MonoBehaviour
{
    [Header("Scene Navigation")]
    public string gameplaySceneName = "MIDTERMS";

    [Header("Assets (auto-populated in Editor)")]
    public Texture2D backgroundTexture;
    public TMP_FontAsset uiFont;
    public AudioClip menuBgm;
    [Range(0f, 1f)]
    public float menuBgmVolume = 0.5f;

    [Header("Panel Content")]
    [TextArea(4, 10)]
    public string creditsText = "CLOSER TO MEMORY\n\nCredits coming soon.";

    [TextArea(6, 12)]
    public string howToPlayText =
        "CONTROLS\n" +
        "WASD — Move\n" +
        "Shift — Run   |   Ctrl — Crouch   |   Space — Jump\n" +
        "Mouse — Look Around\n" +
        "Left Click — Shoot\n" +
        "Space / E / Esc — Close Pop-up\n\n" +
        "OBJECTIVE\n" +
        "Kill 20 enemies, collect 15 glowing items, and view\n" +
        "10 \"Did You Know?\" pop-ups without dying to win.";

    private GameObject settingsPanel;
    private GameObject creditsPanel;
    private GameObject howToPlayPanel;
    private TMP_Text volumeValueText;
    private Slider volumeSlider;
    private AudioSource bgmSource;

    void Awake()
    {
#if UNITY_EDITOR
        LoadDefaultAssetsIfMissing();
#endif
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioListener.volume = GameAudioSettings.MasterVolume;

        SetupBgm();
        BuildMenuUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllSubPanels();
        }
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        CloseAllSubPanels();
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(GameAudioSettings.MasterVolume);
            UpdateVolumeLabel(GameAudioSettings.MasterVolume);
        }
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OpenCredits()
    {
        CloseAllSubPanels();
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void OpenHowToPlay()
    {
        CloseAllSubPanels();
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    public void CloseAllSubPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    private void OnVolumeChanged(float value)
    {
        GameAudioSettings.MasterVolume = value;
        UpdateVolumeLabel(value);
    }

    private void UpdateVolumeLabel(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void SetupBgm()
    {
        if (menuBgm == null) return;

        bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.clip = menuBgm;
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
        bgmSource.volume = menuBgmVolume;
        bgmSource.Play();
    }

    private void BuildMenuUI()
    {
        // Full-screen start_menu.png background
        GameObject bgObj = new GameObject("StartMenuBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        bgObj.layer = gameObject.layer;
        bgObj.transform.SetParent(transform, false);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        RawImage bgImage = bgObj.GetComponent<RawImage>();
        bgImage.texture = backgroundTexture;
        bgImage.color = backgroundTexture != null ? Color.white : Color.black;
        bgImage.raycastTarget = false;

        // Exact normalized hitboxes over the 4 rectangles in Assets/start_menu.png (1437 x 795)
        CreateOverlayHitbox(bgObj.transform, "PlayButton", new Vector2(0.0745f, 0.5786f), new Vector2(0.3118f, 0.6377f), new Color(0.8f, 0.1f, 0.1f, 0.18f), PlayGame);
        CreateOverlayHitbox(bgObj.transform, "SettingsButton", new Vector2(0.0745f, 0.4742f), new Vector2(0.3118f, 0.5333f), new Color(1f, 1f, 1f, 0.12f), OpenSettings);
        CreateOverlayHitbox(bgObj.transform, "CreditsButton", new Vector2(0.0745f, 0.3748f), new Vector2(0.3118f, 0.4340f), new Color(1f, 1f, 1f, 0.12f), OpenCredits);
        CreateOverlayHitbox(bgObj.transform, "HowToPlayButton", new Vector2(0.0703f, 0.2755f), new Vector2(0.3076f, 0.3346f), new Color(1f, 1f, 1f, 0.12f), OpenHowToPlay);

        settingsPanel = BuildSettingsPanel();
        creditsPanel = BuildTextModalPanel("CreditsPanel", "CREDITS", creditsText);
        howToPlayPanel = BuildTextModalPanel("HowToPlayPanel", "HOW TO PLAY", howToPlayText);

        CloseAllSubPanels();
    }

    private void CreateOverlayHitbox(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color highlightTint, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = gameObject.layer;
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = btnObj.GetComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(1f, 1f, 1f, 0f);
        cb.highlightedColor = highlightTint;
        cb.pressedColor = new Color(highlightTint.r, highlightTint.g, highlightTint.b, Mathf.Min(1f, highlightTint.a * 1.8f));
        cb.selectedColor = highlightTint;
        cb.fadeDuration = 0.08f;
        btn.colors = cb;
        btn.onClick.AddListener(onClick);
    }

    private GameObject BuildModalShell(string panelName, string title, out Transform cardTransform)
    {
        GameObject overlay = new GameObject(panelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.layer = gameObject.layer;
        overlay.transform.SetParent(transform, false);

        RectTransform oRect = overlay.GetComponent<RectTransform>();
        oRect.anchorMin = Vector2.zero;
        oRect.anchorMax = Vector2.one;
        oRect.offsetMin = Vector2.zero;
        oRect.offsetMax = Vector2.zero;

        Image oImg = overlay.GetComponent<Image>();
        oImg.color = new Color(0f, 0f, 0f, 0.82f);
        oImg.raycastTarget = true;

        // Subtle outer border frame
        GameObject border = new GameObject("CardBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        border.layer = gameObject.layer;
        border.transform.SetParent(overlay.transform, false);
        RectTransform bRect = border.GetComponent<RectTransform>();
        bRect.anchorMin = new Vector2(0.22f, 0.16f);
        bRect.anchorMax = new Vector2(0.78f, 0.84f);
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        border.GetComponent<Image>().color = new Color(0.45f, 0.45f, 0.45f, 0.9f);

        GameObject card = new GameObject("Card", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        card.layer = gameObject.layer;
        card.transform.SetParent(border.transform, false);
        RectTransform cRect = card.GetComponent<RectTransform>();
        cRect.anchorMin = Vector2.zero;
        cRect.anchorMax = Vector2.one;
        cRect.offsetMin = new Vector2(2f, 2f);
        cRect.offsetMax = new Vector2(-2f, -2f);
        card.GetComponent<Image>().color = new Color(0.03f, 0.03f, 0.03f, 0.98f);

        // Title
        TMP_Text titleTmp = CreateText(card.transform, "Title", title, 52f, TextAlignmentOptions.Center);
        RectTransform tRect = titleTmp.rectTransform;
        tRect.anchorMin = new Vector2(0.08f, 0.78f);
        tRect.anchorMax = new Vector2(0.92f, 0.93f);
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        // Back button
        CreateStyledButton(card.transform, "BackButton", "BACK", new Vector2(0.35f, 0.07f), new Vector2(0.65f, 0.18f), CloseAllSubPanels);

        cardTransform = card.transform;
        return overlay;
    }

    private GameObject BuildSettingsPanel()
    {
        GameObject panel = BuildModalShell("SettingsPanel", "SETTINGS", out Transform card);

        TMP_Text label = CreateText(card, "VolumeLabel", "MASTER VOLUME", 32f, TextAlignmentOptions.Left);
        RectTransform lRect = label.rectTransform;
        lRect.anchorMin = new Vector2(0.12f, 0.56f);
        lRect.anchorMax = new Vector2(0.70f, 0.66f);
        lRect.offsetMin = Vector2.zero;
        lRect.offsetMax = Vector2.zero;

        volumeValueText = CreateText(card, "VolumeValue", Mathf.RoundToInt(GameAudioSettings.MasterVolume * 100f) + "%", 32f, TextAlignmentOptions.Right);
        RectTransform vRect = volumeValueText.rectTransform;
        vRect.anchorMin = new Vector2(0.70f, 0.56f);
        vRect.anchorMax = new Vector2(0.88f, 0.66f);
        vRect.offsetMin = Vector2.zero;
        vRect.offsetMax = Vector2.zero;

        volumeSlider = CreateVolumeSlider(card, new Vector2(0.12f, 0.44f), new Vector2(0.88f, 0.52f));
        volumeSlider.SetValueWithoutNotify(GameAudioSettings.MasterVolume);
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);

        return panel;
    }

    private GameObject BuildTextModalPanel(string panelName, string title, string body)
    {
        GameObject panel = BuildModalShell(panelName, title, out Transform card);

        TMP_Text bodyTmp = CreateText(card, "BodyText", body, 28f, TextAlignmentOptions.Center);
        bodyTmp.enableWordWrapping = true;
        bodyTmp.enableAutoSizing = true;
        bodyTmp.fontSizeMin = 18f;
        bodyTmp.fontSizeMax = 30f;
        RectTransform bRect = bodyTmp.rectTransform;
        bRect.anchorMin = new Vector2(0.08f, 0.22f);
        bRect.anchorMax = new Vector2(0.92f, 0.75f);
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;

        return panel;
    }

    private Slider CreateVolumeSlider(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject sliderObj = new GameObject("VolumeSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.layer = gameObject.layer;
        sliderObj.transform.SetParent(parent, false);
        RectTransform sRect = sliderObj.GetComponent<RectTransform>();
        sRect.anchorMin = anchorMin;
        sRect.anchorMax = anchorMax;
        sRect.offsetMin = Vector2.zero;
        sRect.offsetMax = Vector2.zero;

        // Track background
        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bg.layer = gameObject.layer;
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 0.35f);
        bgRect.anchorMax = new Vector2(1f, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);

        // Fill Area + Fill
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.layer = gameObject.layer;
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.GetComponent<RectTransform>();
        faRect.anchorMin = new Vector2(0f, 0.35f);
        faRect.anchorMax = new Vector2(1f, 0.65f);
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.layer = gameObject.layer;
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fRect = fill.GetComponent<RectTransform>();
        fRect.anchorMin = Vector2.zero;
        fRect.anchorMax = Vector2.one;
        fRect.offsetMin = Vector2.zero;
        fRect.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(0.75f, 0.12f, 0.12f, 1f);

        // Handle Slide Area + Handle
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.layer = gameObject.layer;
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform haRect = handleArea.GetComponent<RectTransform>();
        haRect.anchorMin = Vector2.zero;
        haRect.anchorMax = Vector2.one;
        haRect.offsetMin = new Vector2(10f, 0f);
        haRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        handle.layer = gameObject.layer;
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform hRect = handle.GetComponent<RectTransform>();
        hRect.sizeDelta = new Vector2(22f, 0f);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = Color.white;

        Slider slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.fillRect = fRect;
        slider.handleRect = hRect;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }

    private void CreateStyledButton(Transform parent, string name, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
    {
        GameObject border = new GameObject(name + "Border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        border.layer = gameObject.layer;
        border.transform.SetParent(parent, false);
        RectTransform bRect = border.GetComponent<RectTransform>();
        bRect.anchorMin = anchorMin;
        bRect.anchorMax = anchorMax;
        bRect.offsetMin = Vector2.zero;
        bRect.offsetMax = Vector2.zero;
        border.GetComponent<Image>().color = new Color(0.55f, 0.55f, 0.55f, 1f);

        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = gameObject.layer;
        btnObj.transform.SetParent(border.transform, false);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(2f, 2f);
        rect.offsetMax = new Vector2(-2f, -2f);

        Image img = btnObj.GetComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        cb.highlightedColor = new Color(0.20f, 0.20f, 0.20f, 1f);
        cb.pressedColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        TMP_Text tmp = CreateText(btnObj.transform, "Label", text, 30f, TextAlignmentOptions.Center);
        tmp.rectTransform.anchorMin = Vector2.zero;
        tmp.rectTransform.anchorMax = Vector2.one;
        tmp.rectTransform.offsetMin = Vector2.zero;
        tmp.rectTransform.offsetMax = Vector2.zero;
    }

    private TMP_Text CreateText(Transform parent, string name, string content, float fontSize, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.layer = gameObject.layer;
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        if (uiFont != null)
        {
            tmp.font = uiFont;
        }
        return tmp;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LoadDefaultAssetsIfMissing();
    }

    private void LoadDefaultAssetsIfMissing()
    {
        if (backgroundTexture == null)
        {
            backgroundTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/start_menu.png");
        }
        if (uiFont == null)
        {
            uiFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Roman_New_Times SDF.asset");
        }
        if (menuBgm == null)
        {
            menuBgm = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/envSound.mp3");
        }
    }

    private const string PlayFromStartMenuPrefKey = "AlwaysStartFromStartMenu";
    private const string StartMenuScenePath = "Assets/Scenes/StartMenu.unity";

    [InitializeOnLoadMethod]
    private static void EnsureBuildSettingsScenes()
    {
        EditorApplication.delayCall += () =>
        {
            string[] requiredPaths = { StartMenuScenePath, "Assets/MIDTERMS.unity" };
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool changed = false;

            for (int i = 0; i < requiredPaths.Length; i++)
            {
                string path = requiredPaths[i];
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path)))
                {
                    continue;
                }

                int existingIdx = current.FindIndex(s => s.path == path);
                if (existingIdx < 0)
                {
                    current.Insert(Mathf.Min(i, current.Count), new EditorBuildSettingsScene(path, true));
                    changed = true;
                }
                else if (i == 0 && existingIdx != 0)
                {
                    var item = current[existingIdx];
                    item.enabled = true;
                    current.RemoveAt(existingIdx);
                    current.Insert(0, item);
                    changed = true;
                }
                else if (!current[existingIdx].enabled)
                {
                    current[existingIdx].enabled = true;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorBuildSettings.scenes = current.ToArray();
            }

            ApplyPlayModeStartScene();
        };
    }

    private static void ApplyPlayModeStartScene()
    {
        bool enabled = EditorPrefs.GetBool(PlayFromStartMenuPrefKey, true);
        EditorSceneManager.playModeStartScene = enabled
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(StartMenuScenePath)
            : null;
    }

    [MenuItem("Tools/Always Start From StartMenu")]
    private static void TogglePlayFromStartMenu()
    {
        bool next = !EditorPrefs.GetBool(PlayFromStartMenuPrefKey, true);
        EditorPrefs.SetBool(PlayFromStartMenuPrefKey, next);
        ApplyPlayModeStartScene();
    }

    [MenuItem("Tools/Always Start From StartMenu", true)]
    private static bool TogglePlayFromStartMenuValidate()
    {
        Menu.SetChecked("Tools/Always Start From StartMenu", EditorPrefs.GetBool(PlayFromStartMenuPrefKey, true));
        return true;
    }
#endif
}
