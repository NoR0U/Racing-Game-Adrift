using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class carController : MonoBehaviour
{

    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }
    [SerializeField]private driveType drive;

    //components:
    [HideInInspector]public inputs input;
    [HideInInspector]public Rigidbody _rigidbody;

    public float torque = 100 , maxRPM , minRPM;
    public AnimationCurve enginePower;
    [Range(0.2f, 0.8f)] public float EngineSmoothTime;
    [HideInInspector] public float engineRPM, engineLoad, DownForceValue, dragAmount, KPH;

    public float[] gears;
    public float finalDrive = 3.7f ;
    public int gearNum;

    [HideInInspector] public float ForwardStifness = 1.8f;
    [HideInInspector] public float SidewaysStifness = 2.0f;

    [HideInInspector]public Transform[] wheelTransforms;
    [HideInInspector]public WheelCollider[] wheelColliders;

    private Vector3 wheelPosition;
    private Quaternion wheelRotation;

    [Range(2f, 10f)] public float steeringDamper;
    [Range(0f, 10f)] public float steeringRange;
    public float maxSteer = 30f;


    //junk
    [HideInInspector]public bool vehicleChecked = false;
    private float vertical , horizontal;
    private float finalTurnAngle , radius;
    public float wheelsRPM , acceleration , totalPower , gearChangeRate;
    private float wheelsMaxRPM;
    private float engineLerpValue , brakePower ;
	private WheelFrictionCurve  forwardFriction,sidewaysFriction;
    private float[] wheelSlip;
    public bool engineLerp;
    private bool reverse , grounded;
    
    public float currentVelocity , lastFrameVelocity , Gforce;

    void Start()
    {
        findValues();
    }
  
    void Update()
    {
        currentVelocity = _rigidbody.velocity.magnitude;

        moveCar();
        updateWheels();
    }

    /// <summary>
    /// This function is called every fixed framerate frame, if the MonoBehaviour is enabled.
    /// </summary>
    void FixedUpdate()
    {
        currentVelocity = _rigidbody.velocity.magnitude;
        Gforce=( currentVelocity- lastFrameVelocity ) / ( Time.deltaTime * Physics.gravity.magnitude );
        lastFrameVelocity =currentVelocity;
    }


    void findValues()
    {
        print("tt");
        foreach (Transform i in gameObject.transform)
        {
            if(i.transform.name == "WheelColiders")
            {
                wheelColliders = new WheelCollider[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++)
                {
                    wheelColliders[q] = i.transform.GetChild(q).GetComponent<WheelCollider>();
                }    
            }

            if(i.transform.name == "WheelMeshes")
            {
                wheelTransforms = new Transform[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++)
                {
                    wheelTransforms[q] = i.transform.GetChild(q);
                }    
            }
        }

        //get components:
        input = GetComponent<inputs>();
        _rigidbody = GetComponent<Rigidbody>();

        wheelSlip = new float[wheelColliders.Length];
        vehicleChecked = true;

    }

    void moveCar()
    {
        runEngine();
        steerVehicle();
    }

    void runEngine()
    {
        lerpEngine();
        wheelRPM();
        manual();

        acceleration = vertical > 0 ?  vertical : wheelsRPM <= 1 ? vertical  : 0 ;
        
        if(!isGrounded())
        {
            acceleration = engineRPM > 1000 ? acceleration / 2 : acceleration; 
        }

        if(engineRPM >= maxRPM)
        {
            setEngineLerp(maxRPM - 1000);
        }

        if(!engineLerp)
        {
            engineRPM = Mathf.Lerp(engineRPM,1000f + Mathf.Abs(wheelsRPM) *  finalDrive *  (gears[gearNum]) , (EngineSmoothTime * 10) * Time.deltaTime);
            totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum] * finalDrive) * acceleration  ;
        }
        
        engineLoad = Mathf.Lerp(engineLoad,vertical - ((engineRPM - 1000) / maxRPM ),(EngineSmoothTime * 10) * Time.deltaTime);
        runCar();

        wheelsMaxRPM = engineRPM / (gears[gearNum] * finalDrive);
        Debug.Log(wheelsMaxRPM);
    }

    void runCar()
    {
        if(drive == driveType.rearWheelDrive)
        {
            for (int i = 2; i < wheelColliders.Length; i++)
            {
                if (wheelColliders[i].rpm <= wheelsMaxRPM)
                {
                    wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2);
                }

                else
                {
                    wheelColliders[i].motorTorque = 0;
                }
            }
        }

        else if(drive == driveType.frontWheelDrive)
        {
            for (int i = 0; i < wheelColliders.Length - 2; i++)
            {
                if (wheelColliders[i].rpm <= wheelsMaxRPM)
                {
                    wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / (wheelColliders.Length - 2);
                }

                else
                {
                    wheelColliders[i].motorTorque = 0;
                }
            }
        }

        else
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if(wheelColliders[i].rpm <= wheelsMaxRPM)
                {
                    wheelColliders[i].motorTorque = (vertical == 0) ? 0 : totalPower / wheelColliders.Length;
                }

                else
                {
                    wheelColliders[i].motorTorque = 0;
                }
                    
            }
        }


        for (int i = 0; i < wheelColliders.Length; i++){
            if(KPH <= 1 && KPH >= -1 && vertical == 0){
                brakePower = 5;
            }

            else
            {
                if(vertical < 0 && KPH > 1 && !reverse)
                    brakePower =  (wheelSlip[i] <= 0.3f) ? brakePower + -vertical * 100 : brakePower > 0 ? brakePower  + vertical * 50 : 0 ;
                else 
                    brakePower = 0;
            }
            wheelColliders[i].brakeTorque = brakePower;
        }

        wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = (input.handbrake)? Mathf.Infinity : 0f;

        _rigidbody.angularDrag = (KPH > 100)? KPH / 100 : 0;
        _rigidbody.drag = dragAmount + (KPH / 40000) ;

        KPH = _rigidbody.velocity.magnitude * 3.6f;
        friction();
    }

    void steerVehicle()
    {

        vertical = input.vertical;
        horizontal = Mathf.Lerp(horizontal , input.horizontal , (input.horizontal != 0) ? steeringDamper * Time.deltaTime : 3 * 2 * Time.deltaTime);

        finalTurnAngle = (radius > steeringRange) ? radius : steeringRange;

        wheelColliders[0].steerAngle = input.horizontal * maxSteer;
        wheelColliders[1].steerAngle = input.horizontal * maxSteer;
        /*if (horizontal > 0 )
        {
            //rear tracks size is set to 1.5f       wheel base has been set to 2.55f
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
        }

        else if (horizontal < 0 )
        {                                                          
            wheelColliders[0].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle + (1.5f / 2))) * horizontal;
            wheelColliders[1].steerAngle = Mathf.Rad2Deg * Mathf.Atan(2.55f / (finalTurnAngle - (1.5f / 2))) * horizontal;
			//transform.Rotate(Vector3.up * steerHelping);
        }

        else
        {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }*/

    }
    
    void updateWheels()
    {
        for (int i = 0; i < wheelColliders.Length; i++) {
            wheelColliders[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelTransforms[i].transform.rotation = wheelRotation;
            wheelTransforms[i].transform.position = wheelPosition;
        }
    }

    void wheelRPM()
    {
        float sum = 0;
        int R = 0;
        for (int i = 0; i < 4; i++)
        {
            sum += wheelColliders[i].rpm;
            R++;
        }
        wheelsRPM = (R != 0) ? sum / R : 0;
 
        if(wheelsRPM < 0 && !reverse )
        {
            reverse = true;
            //if (gameObject.tag != "AI") manager.changeGear();
        }

        else if(wheelsRPM > 0 && reverse)
        {
            reverse = false;
            //if (gameObject.tag != "AI") manager.changeGear();
        }
    }

    void setEngineLerp(float num)
    {
        engineLerp = true;
        engineLerpValue = num;
    }

    void lerpEngine()
    {
        if(engineLerp)
        {
            totalPower = 0;
            engineRPM = Mathf.Lerp(engineRPM,engineLerpValue,20 * Time.deltaTime );
            engineLerp = engineRPM <= engineLerpValue + 100 ? false : true;
        }
    }   

    bool isGrounded()
    {
        if(wheelColliders[0].isGrounded &&wheelColliders[1].isGrounded &&wheelColliders[2].isGrounded &&wheelColliders[3].isGrounded )
            return true;
        else
            return false;
    }

    void manual()
    {

        if((Input.GetAxis("Fire2") == 1  ) && gearNum <= gears.Length && Time.time >= gearChangeRate )
        {
            gearNum  = gearNum +1;
            gearChangeRate = Time.time + 1f/3f ;
            setEngineLerp(engineRPM - ( engineRPM > 1500 ? 2000 : 700));
            //audio.DownShift();

        }

        if((Input.GetAxis("Fire3") == 1 ) && gearNum >= 1  && Time.time >= gearChangeRate)
        {
            gearChangeRate = Time.time + 1f/3f ;
            gearNum --;
            setEngineLerp(engineRPM - ( engineRPM > 1500 ? 1500 : 700));
            //audio.DownShift();
        }
    
    }

    void friction()
    {
        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheelColliders.Length];
        for (int i = 0; i < wheelColliders.Length ; i++)
        {
            if(wheelColliders[i].GetGroundHit(out hit) && i >= 2 )
            {
                forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = (input.handbrake)?  .55f : ForwardStifness; 
                wheelColliders[i].forwardFriction = forwardFriction;

                sidewaysFriction = wheelColliders[i].sidewaysFriction;
                sidewaysFriction.stiffness = (input.handbrake)? .55f : SidewaysStifness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;
                
                grounded = true;

                sum += Mathf.Abs(hit.sidewaysSlip);
            }

            else grounded = false;

            wheelSlip[i] = Mathf.Abs( hit.forwardSlip ) + Mathf.Abs(hit.sidewaysSlip) ;
            sidewaysSlip[i] = Mathf.Abs(hit.sidewaysSlip);
           
        }

        sum /= wheelColliders.Length - 2 ;
        radius = (KPH > 60) ?  4 + (sum * -25) + KPH / 8 : 4;
        
    }

    void OnGUI()
    {

        float pos = 50;

        GUI.Label(new Rect(20, pos, 200, 20),"currentGear: " + gearNum.ToString("0"));
        pos+=25f;

        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineRPM,0,maxRPM);
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"Torque: " + totalPower.ToString("0"));
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"KPH: " + KPH.ToString("0"));
        pos+=25f;

        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineLoad, 0.0F, 1.0F);
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"brakes: " + brakePower.ToString("0"));
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"currentVelocity: " + currentVelocity.ToString("0"));
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"lastFrameVelocity: " + lastFrameVelocity.ToString("0"));
        pos+=25f;

        GUI.Label(new Rect(20, pos, 200, 20),"Gforce: " + Gforce.ToString("0"));
        pos+=25f; 
    }
}
