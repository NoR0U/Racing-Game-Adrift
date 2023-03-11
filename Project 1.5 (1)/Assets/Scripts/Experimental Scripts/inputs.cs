using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputs : MonoBehaviour{

   
    [HideInInspector]public float vertical;
    [HideInInspector]public float horizontal;
    [HideInInspector]public bool handbrake;
    [HideInInspector]public bool boosting;


    void Update()
    {
        keyboard();
    }

    public void keyboard () {
        vertical = Input.GetAxis ("Vertical");
        horizontal = Input.GetAxis ("Horizontal");
        handbrake = (Input.GetAxis ("Jump") != 0) ? true : false;
        boosting = (Input.GetKey (KeyCode.LeftShift)) ? true : false;
        //if (Input.GetKey (KeyCode.LeftShift)) boosting = true;
        //else boosting = false;

    }



}
