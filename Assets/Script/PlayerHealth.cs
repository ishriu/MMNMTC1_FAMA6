using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float enemyContactDamage = 25f;
    public float damageCooldown = 1.0f;

    [Header("HUD References")]
    public RectTransform hudOverlayRect;
    public Image healthBarFill;
    public Color fullHealthColor = new Color(0.75f, 0.12f, 0.12f, 0.95f);
    public Color lowHealthColor = new Color(0.95f, 0.05f, 0.05f, 1f);

    public float CurrentHealth { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<float> OnHealthChanged;
    public event Action OnPlayerDied;

    // Exact inner bounds of the bottom-left health bar rectangle in Assets/UI.png (8000 x 4500)
    private const float BarMinX = 0.0215f;
    private const float BarMaxX = 0.3240f;
    private const float BarMinY = 0.0365f;
    private const float BarMaxY = 0.0840f;

    private float nextDamageTime = 0f;
    private RectTransform fillRect;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        CurrentHealth = maxHealth;
        IsDead = false;
    }

    void Start()
    {
        EnsureHealthBarUI();
        UpdateHealthBar();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        TryApplyEnemyContactDamage(collision.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        TryApplyEnemyContactDamage(collision.gameObject);
    }

    private void TryApplyEnemyContactDamage(GameObject other)
    {
        if (IsDead || Time.timeScale <= 0f || other == null)
        {
            return;
        }

        if (other.CompareTag("Enemy") && Time.time >= nextDamageTime)
        {
            nextDamageTime = Time.time + damageCooldown;
            TakeDamage(enemyContactDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        UpdateHealthBar();

        float normalized = maxHealth > 0f ? CurrentHealth / maxHealth : 0f;
        OnHealthChanged?.Invoke(normalized);

        if (CurrentHealth <= 0f)
        {
            IsDead = true;
            OnPlayerDied?.Invoke();
        }
    }

    private void EnsureHealthBarUI()
    {
        if (healthBarFill != null)
        {
            fillRect = healthBarFill.rectTransform;
            return;
        }

        Transform parent = hudOverlayRect;
        if (parent == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform imgTransform = canvas.transform.Find("Image");
                parent = imgTransform != null ? imgTransform : canvas.transform;
            }
        }

        if (parent == null)
        {
            return;
        }

        // Dark semi-transparent backing inside the UI.png bottom-left border
        GameObject bgObj = new GameObject("HealthBarBackground", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bgObj.layer = parent.gameObject.layer;
        bgObj.transform.SetParent(parent, false);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(BarMinX, BarMinY);
        bgRect.anchorMax = new Vector2(BarMaxX, BarMaxY);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.GetComponent<Image>();
        bgImg.color = new Color(0.08f, 0.02f, 0.02f, 0.65f);
        bgImg.raycastTarget = false;

        // Dynamic fill bar
        GameObject fillObj = new GameObject("HealthBarFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fillObj.layer = parent.gameObject.layer;
        fillObj.transform.SetParent(parent, false);
        fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(BarMinX, BarMinY);
        fillRect.anchorMax = new Vector2(BarMaxX, BarMaxY);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        healthBarFill = fillObj.GetComponent<Image>();
        healthBarFill.color = fullHealthColor;
        healthBarFill.raycastTarget = false;
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null || fillRect == null)
        {
            return;
        }

        float normalized = maxHealth > 0f ? Mathf.Clamp01(CurrentHealth / maxHealth) : 0f;
        fillRect.anchorMax = new Vector2(Mathf.Lerp(BarMinX, BarMaxX, normalized), BarMaxY);
        healthBarFill.color = Color.Lerp(lowHealthColor, fullHealthColor, normalized);
    }
}
