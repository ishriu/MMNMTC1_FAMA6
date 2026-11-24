using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeControls : MonoBehaviour
{
    Animator myCubeAnim;
    // Start is called before the first frame update
    void Start()
    {
        myCubeAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()

    {
            if (Input.GetKeyDown(KeyCode.T))
        {
            myCubeAnim.Play("twist");
        
         }
            if (Input.GetKeyDown(KeyCode.Z))
        {
            myCubeAnim.Play("scale");
        }
    }
}
