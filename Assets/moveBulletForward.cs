using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveBulletForward : MonoBehaviour
{
    Rigidbody mySphereRigid;
    // Start is called before the first frame update
    void Start()
    {
        mySphereRigid = GetComponent<Rigidbody>();
        Destroy(gameObject, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        mySphereRigid.AddForce(transform.forward * 30);
    }

    void OnCollisionEnter(Collision col)
    {
      if(col.gameObject.tag == "Enemy")
        {
            col.gameObject.tag = "Untagged";
            if (KillCounter.Instance != null)
            {
                KillCounter.Instance.AddKill();
            }
            Destroy(col.gameObject);
            Destroy(gameObject);
        }
    }
}
