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
    }

    // Update is called once per frame
    void Update()
    {
        mySphereRigid.AddForce(transform.forward * 30);

        Destroy(gameObject, .5f);
    }

    void OnCollisionEnter(Collision col)
    {
      if(col.gameObject.tag == "Enemy")
        {
            Destroy(col.gameObject);
            Destroy(gameObject);
        }
    }
}
