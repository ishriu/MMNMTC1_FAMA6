using System;
using UnityEngine;
using TMPro;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    public TMP_Text deathText;

    public int KillCount { get; private set; }
    public event Action<int> OnKillCountChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        KillCount = 0;
        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddKill()
    {
        KillCount++;
        UpdateDisplay();
        OnKillCountChanged?.Invoke(KillCount);
    }

    void UpdateDisplay()
    {
        if (deathText != null)
        {
            deathText.text = KillCount.ToString();
        }
    }
}
