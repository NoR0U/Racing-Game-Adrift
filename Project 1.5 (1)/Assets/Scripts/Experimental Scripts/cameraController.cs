using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraController : MonoBehaviour
{

    public float fieldOfView = 68f;

    private GameObject atachedVehicle;
    private int locationIndicator = 1 ;
    private carController controllerRef;
    private Camera camera;

    private float bias ;
    private float smoothTime = .5f;
    private float smoothTimemin = .5f , max = .9f;
    private Vector3 newPos;
    private Transform target;
    
    private float bandEffect;

    private GameObject focusPoint;
    private GameObject driverPoint;

    public Vector2[] cameraPos;

    private bool hasDriver = false;
    
    public float temp = 10;

    void Start(){

        cameraPos = new Vector2[4];
        cameraPos[0] = new Vector2(2,0);
        cameraPos[1] = new Vector2(7.5f,0.5f);
        cameraPos[2] = new Vector2(8.4f,1.6f);
        cameraPos[3] = new Vector2(8.4f,1.6f);

        camera = gameObject.GetComponent<Camera>();
        atachedVehicle = GameObject.FindGameObjectWithTag("Player");

        focusPoint = atachedVehicle.transform.Find("focus").gameObject;


        target = focusPoint.transform;

        controllerRef = atachedVehicle.GetComponent<carController>();

        camera.usePhysicalProperties = true;
        camera.fieldOfView = fieldOfView;

    }

    void FixedUpdate(){

        updateCam();

    }

    void Update()
    {
        if(hasDriver)
        if(locationIndicator == cameraPos.Length-1 && driverPoint != null){
        transform.position = driverPoint.transform.position;
        transform.rotation = driverPoint.transform.rotation;
        camera.fieldOfView = 80;

        }

        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView , fieldOfView , temp * Time.deltaTime );
    }



    public void cycleCamera(){
        if(locationIndicator >= cameraPos.Length-1 || locationIndicator < 0) locationIndicator = 0;
            else locationIndicator ++;
    }

    public void updateCam(){
        if(Input.GetKeyDown(KeyCode.Tab)){
            cycleCamera();
        }    

        if(locationIndicator != cameraPos.Length-1){

            bandEffect = (controllerRef.KPH < 400) ? 300 - controllerRef.KPH /400 : 200; 

            //camera.fieldOfView = fieldOfView;

            newPos = target.position - (target.forward * cameraPos[locationIndicator].x) + (target.up * cameraPos[locationIndicator].y) ;
            transform.position = newPos * (1 - smoothTime) + transform.position * smoothTime;
            transform.LookAt(target.transform);
            transform.localPosition += transform.forward * controllerRef.KPH / bandEffect ;

            bias = max - controllerRef.KPH  / 400  ;
            smoothTime =( bias >=smoothTimemin)? bias :smoothTimemin;

        }
    }

}
