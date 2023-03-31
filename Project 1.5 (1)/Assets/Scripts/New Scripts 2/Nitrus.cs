using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nitrus {

    public Nitrus(){}

    public void boost(GameObject car , float boostForce , Camera camera , float fieldOfView){
        Rigidbody R = car.GetComponent<Rigidbody>();
        R.AddForce(car.transform.forward * boostForce );
        camera.fieldOfView += fieldOfView;
    }

}
