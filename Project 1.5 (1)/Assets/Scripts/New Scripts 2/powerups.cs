using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class powerups : MonoBehaviour{

    public float boostForce;
    public float fieldOfView;

    public Camera camera;


    void Start()
    {
        camera = FindObjectOfType<Camera>();
    }

    void Update(){
        if(Input.GetKeyDown(KeyCode.LeftShift)){
            Nitrus n = new Nitrus();
            n.boost(gameObject , boostForce , camera , fieldOfView);
        }
    }

}
