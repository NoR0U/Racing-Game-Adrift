using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class inputs_2 : MonoBehaviour
{


     public float vertical;
     public float horizontal;

     public float throttleInput;
     public float brakeInput;
     
    [HideInInspector] public bool handbrake;
    [HideInInspector] public bool boosting;


    void Update()
    {
        keyboard();

        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake = (Input.GetAxis("Jump") != 0) ? true : false;
        boosting = (Input.GetKey(KeyCode.LeftShift)) ? true : false;

        if (vertical > 0)
        {
            throttleInput = vertical;
        }

        else if (vertical < 0)
        {
            brakeInput = vertical;
        }

        else
        {
            throttleInput = 0;
            brakeInput = 0;
        }
    }

    public void keyboard()
    {
        //if (Input.GetKey (KeyCode.LeftShift)) boosting = true;
        //else boosting = false;
    }
}