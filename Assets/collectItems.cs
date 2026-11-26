using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class collectItems : MonoBehaviour
{
    public static collectItems Instance { get; private set; }

    public TMP_Text numberText;
    int kolekCollected = 0;

    public int CollectiblesCount => kolekCollected;
    public event Action<int> OnCollectibleCountChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        numberText.text = kolekCollected.ToString();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter(Collision collectibles)
    {
        if (collectibles.gameObject.tag=="kolek")
        {
            collectibles.gameObject.tag = "Untagged";
            Destroy(collectibles.gameObject);

            kolekCollected++;

            numberText.text = kolekCollected.ToString();
            OnCollectibleCountChanged?.Invoke(kolekCollected);

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowNextPopup();
            }
        }
    }
}
