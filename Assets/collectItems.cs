using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class collectItems : MonoBehaviour
{
    public TMP_Text numberText;
    int kolekCollected = 0;
    // Start is called before the first frame update
    void Start()
    {
        numberText.text = kolekCollected.ToString();
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

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowNextPopup();
            }
        }
    }
}
