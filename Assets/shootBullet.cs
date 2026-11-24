using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shootBullet : MonoBehaviour
{
    public GameObject myBullet;
    public Transform bulletPosition;

    AudioSource mySFX;

    // Start is called before the first frame update
    void Start()
    {
        mySFX = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(myBullet, bulletPosition.position, bulletPosition.rotation);

            mySFX.Play();
        }
    }
}
