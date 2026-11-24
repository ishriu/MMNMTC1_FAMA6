using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    public static bool IsPopupOpen { get; private set; }
    public static bool IsInputBlocked => IsPopupOpen || Time.frameCount <= lastCloseFrame;

    private static int lastCloseFrame = -1;

    [Header("HUD Counter")]
    public TMP_Text foundText;

    [Header("Pop-up Assets (1..10)")]
    public Texture2D[] popupTextures;

    [Header("Audio")]
    public AudioClip collectSound;
    [Range(0f, 1f)]
    public float collectVolume = 1f;

    [Header("Optional Custom UI (auto-built if left empty)")]
    public GameObject popupPanel;
    public RawImage popupImage;
    public Button closeButton;

    public int PopupsShown { get; private set; }
    public event Action<int> OnPopupCountChanged;

    private int nextPopupIndex = 0;
    private AudioSource audioSource;
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
        IsPopupOpen = false;
        lastCloseFrame = -1;

#if UNITY_EDITOR
        LoadDefaultAssetsIfMissing();
#endif

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        EnsurePopupUI();
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    void Start()
    {
        PopupsShown = 0;
        nextPopupIndex = 0;
        UpdateCounterDisplay();

        cachedLook = FindObjectOfType<FirstPersonLook>();
        cachedMovement = FindObjectOfType<FirstPersonMovement>();
    }

    void Update()
    {
        if (!IsPopupOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            ClosePopup();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            if (IsPopupOpen)
            {
                Time.timeScale = 1f;
                IsPopupOpen = false;
            }
            Instance = null;
        }
    }

    public void ShowNextPopup()
    {
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound, collectVolume);
        }

        PopupsShown++;
        UpdateCounterDisplay();
        OnPopupCountChanged?.Invoke(PopupsShown);

        if (popupTextures == null || popupTextures.Length == 0)
        {
            Debug.LogWarning("PopupManager: No popupTextures assigned.");
            return;
        }

        EnsurePopupUI();

        Texture2D tex = popupTextures[nextPopupIndex];
        nextPopupIndex = (nextPopupIndex + 1) % popupTextures.Length;

        if (popupImage != null && tex != null)
        {
            popupImage.texture = tex;
        }

        if (popupPanel != null)
        {
            popupPanel.transform.SetAsLastSibling();
            popupPanel.SetActive(true);
        }

        SetGameplayPaused(true);
    }

    public void ClosePopup()
    {
        if (!IsPopupOpen)
        {
            return;
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        lastCloseFrame = Time.frameCount;
        SetGameplayPaused(false);
    }

    private void SetGameplayPaused(bool paused)
    {
        IsPopupOpen = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (cachedLook == null)
        {
            cachedLook = FindObjectOfType<FirstPersonLook>();
        }
        if (cachedMovement == null)
        {
            cachedMovement = FindObjectOfType<FirstPersonMovement>();
        }

        if (cachedLook != null)
        {
            cachedLook.enabled = !paused;
        }
        if (cachedMovement != null)
        {
            cachedMovement.enabled = !paused;
        }

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;
    }

    private void UpdateCounterDisplay()
    {
        if (foundText != null)
        {
            foundText.text = PopupsShown.ToString();
        }
    }

    private void EnsurePopupUI()
    {
        if (popupPanel != null && popupImage != null)
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(ClosePopup);
                closeButton.onClick.AddListener(ClosePopup);
            }
            return;
        }

        // Full-screen translucent dark backdrop under Canvas
        GameObject panelObj = new GameObject("PopupPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObj.layer = gameObject.layer;
        panelObj.transform.SetParent(transform, false);

        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image backdrop = panelObj.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.8f);
        backdrop.raycastTarget = true;

        // Centered 1:1 popup card (sized relative to screen so it fits any resolution)
        GameObject cardObj = new GameObject("PopupCard", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
        cardObj.layer = gameObject.layer;
        cardObj.transform.SetParent(panelObj.transform, false);

        RectTransform cardRect = cardObj.GetComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.18f);
        cardRect.anchorMax = new Vector2(0.5f, 0.90f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;

        AspectRatioFitter fitter = cardObj.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
        fitter.aspectRatio = 1f;

        RawImage rawImg = cardObj.GetComponent<RawImage>();
        rawImg.color = Color.white;
        rawImg.raycastTarget = false;

        // Continue / Close button below the card
        GameObject btnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.layer = gameObject.layer;
        btnObj.transform.SetParent(panelObj.transform, false);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.06f);
        btnRect.anchorMax = new Vector2(0.5f, 0.14f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(260f, 0f);
        btnRect.anchoredPosition = Vector2.zero;

        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        colors.highlightedColor = new Color(0.30f, 0.30f, 0.30f, 1f);
        colors.pressedColor = new Color(0.08f, 0.08f, 0.08f, 1f);
        btn.colors = colors;
        btn.onClick.AddListener(ClosePopup);

        GameObject textObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObj.layer = gameObject.layer;
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = textObj.GetComponent<TextMeshProUGUI>();
        label.text = "CONTINUE";
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.fontSize = 30f;
        label.enableAutoSizing = true;
        label.fontSizeMin = 16f;
        label.fontSizeMax = 34f;
        if (foundText != null && foundText.font != null)
        {
            label.font = foundText.font;
        }

        popupPanel = panelObj;
        popupImage = rawImg;
        closeButton = btn;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        LoadDefaultAssetsIfMissing();
    }

    private void LoadDefaultAssetsIfMissing()
    {
        bool needsTextures = popupTextures == null || popupTextures.Length == 0;
        if (!needsTextures)
        {
            for (int i = 0; i < popupTextures.Length; i++)
            {
                if (popupTextures[i] == null)
                {
                    needsTextures = true;
                    break;
                }
            }
        }

        if (needsTextures)
        {
            Texture2D[] loaded = new Texture2D[10];
            bool foundAny = false;
            for (int i = 0; i < 10; i++)
            {
                loaded[i] = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Popups/{i + 1}.png");
                if (loaded[i] != null)
                {
                    foundAny = true;
                }
            }
            if (foundAny)
            {
                popupTextures = loaded;
            }
        }

        if (collectSound == null)
        {
            collectSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/collectSound.wav");
        }
    }
#endif
}
