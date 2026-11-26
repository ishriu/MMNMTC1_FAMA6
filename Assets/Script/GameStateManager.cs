using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public static bool IsGameEnded { get; private set; }

    [Header("Win Condition")]
    public int killsToWin = 20;
    public int collectiblesToWin = 15;
    public int popupsToWin = 10;

    [Header("Scene Navigation")]
    public string startMenuSceneName = "StartMenu";

    [Header("Screen Assets (auto-populated in Editor)")]
    public Texture2D winScreenTexture;
    public Texture2D loseScreenTexture;
    public TMP_FontAsset uiFont;

    private GameObject endScreenPanel;
    private RawImage endScreenImage;
    private FirstPersonLook cachedLook;
    private FirstPersonMovement cachedMovement;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        IsGameEnded = false;

#if UNITY_EDITOR
        LoadDefaultAssetsIfMissing();
#endif
    }

    void Start()
    {
        cachedLook = FindObjectOfType<FirstPersonLook>();
        cachedMovement = FindObjectOfType<FirstPersonMovement>();

        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied += TriggerLose;
        }

        if (KillCounter.Instance != null)
        {
            KillCounter.Instance.OnKillCountChanged += HandleProgressChanged;
        }

        if (collectItems.Instance != null)
        {
            collectItems.Instance.OnCollectibleCountChanged += HandleProgressChanged;
        }

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OnPopupClosed += HandleProgressChanged;
        }
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied -= TriggerLose;
        }

        if (KillCounter.Instance != null)
        {
            KillCounter.Instance.OnKillCountChanged -= HandleProgressChanged;
        }

        if (collectItems.Instance != null)
        {
            collectItems.Instance.OnCollectibleCountChanged -= HandleProgressChanged;
        }

        if (PopupManager.Instance != null)
        {
            PopupManager.Instance.OnPopupClosed -= HandleProgressChanged;
        }

        if (Instance == this)
        {
            if (IsGameEnded)
            {
                Time.timeScale = 1f;
                IsGameEnded = false;
            }
            Instance = null;
        }
    }

    private void HandleProgressChanged(int _)
    {
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (IsGameEnded || PopupManager.IsPopupOpen)
        {
            return;
        }

        int kills = KillCounter.Instance != null ? KillCounter.Instance.KillCount : 0;
        int collectibles = collectItems.Instance != null ? collectItems.Instance.CollectiblesCount : 0;
        int popups = PopupManager.Instance != null ? PopupManager.Instance.PopupsShown : 0;

        if (kills >= killsToWin && collectibles >= collectiblesToWin && popups >= popupsToWin)
        {
            TriggerWin();
        }
    }

    public void TriggerWin()
    {
        if (IsGameEnded) return;
        ShowEndScreen(winScreenTexture, new Color(1f, 1f, 1f, 0.14f));
    }

    public void TriggerLose()
    {
        if (IsGameEnded) return;
        if (PopupManager.Instance != null && PopupManager.IsPopupOpen)
        {
            PopupManager.Instance.ClosePopup();
        }
        ShowEndScreen(loseScreenTexture, new Color(0.8f, 0.1f, 0.1f, 0.22f));
    }

    public void PlayAgain()
    {
        IsGameEnded = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        IsGameEnded = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(startMenuSceneName);
    }

    private void ShowEndScreen(Texture2D screenTexture, Color buttonHighlight)
    {
        IsGameEnded = true;
        Time.timeScale = 0f;

        if (cachedLook == null) cachedLook = FindObjectOfType<FirstPersonLook>();
        if (cachedMovement == null) cachedMovement = FindObjectOfType<FirstPersonMovement>();

        if (cachedLook != null) cachedLook.enabled = false;
        if (cachedMovement != null) cachedMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EnsureEndScreenUI(buttonHighlight);
        if (endScreenImage != null)
        {
            endScreenImage.texture = screenTexture;
            endScreenImage.color = screenTexture != null ? Color.white : Color.black;
        }

        if (endScreenPanel != null)
        {
            endScreenPanel.transform.SetAsLastSibling();
            endScreenPanel.SetActive(true);
        }
    }

    private void EnsureEndScreenUI(Color playAgainHighlight)
    {
        if (endScreenPanel != null) return;

        // Full-screen black backdrop + won.png / died.png RawImage
        endScreenPanel = new GameObject("EndScreenPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        endScreenPanel.layer = gameObject.layer;
        endScreenPanel.transform.SetParent(transform, false);

        RectTransform panelRect = endScreenPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = endScreenPanel.GetComponent<Image>();
        panelBg.color = Color.black;
        panelBg.raycastTarget = true;

        GameObject imgObj = new GameObject("EndScreenArtwork", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        imgObj.layer = gameObject.layer;
        imgObj.transform.SetParent(endScreenPanel.transform, false);

        RectTransform imgRect = imgObj.GetComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;

        endScreenImage = imgObj.GetComponent<RawImage>();
        endScreenImage.raycastTarget = false;

        // Exact normalized hitbox over PLAY AGAIN box in Assets/won.png & Assets/died.png (1920 x 1080)
        CreateOverlayHitbox(
            imgObj.transform,
            "PlayAgainButton",
            new Vector2(0.3813f, 0.3380f),
            new Vector2(0.6182f, 0.3963f),
            playAgainHighlight,
            PlayAgain
        );

        // Matching outlined MAIN MENU button directly underneath PLAY AGAIN
        CreateStyledButton(
            imgObj.transform,
            "MainMenuButton",
            "MAIN MENU",
            new Vector2(0.3813f, 0.2450f),
            new Vector2(0.6182f, 0.3033f),
            ReturnToMainMenu
        );
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
        border.GetComponent<Image>().color = new Color(0.55f, 0.55f, 0.55f, 0.85f);

        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = gameObject.layer;
        btnObj.transform.SetParent(border.transform, false);
        RectTransform rect = btnObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(1.5f, 1.5f);
        rect.offsetMax = new Vector2(-1.5f, -1.5f);

        Image img = btnObj.GetComponent<Image>();
        img.color = Color.white;

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        cb.highlightedColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        cb.pressedColor = new Color(0.10f, 0.10f, 0.10f, 1f);
        btn.colors = cb;
        btn.onClick.AddListener(onClick);

        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.layer = gameObject.layer;
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontSize = 30f;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 16f;
        tmp.fontSizeMax = 32f;
        if (uiFont != null)
        {
            tmp.font = uiFont;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LoadDefaultAssetsIfMissing();
    }

    private void LoadDefaultAssetsIfMissing()
    {
        if (winScreenTexture == null)
        {
            winScreenTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/won.png");
        }
        if (loseScreenTexture == null)
        {
            loseScreenTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/died.png");
        }
        if (uiFont == null)
        {
            uiFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Roman_New_Times SDF.asset");
        }
    }
#endif
}
