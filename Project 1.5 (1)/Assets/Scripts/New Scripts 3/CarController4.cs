using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CarController4 : MonoBehaviour
{
    internal enum driveType
    {
        frontWheelDrive,
        rearWheelDrive,
        allWheelDrive
    }

    [SerializeField] private driveType drive;

    //components:
    [HideInInspector] public inputs_2 input;
    [HideInInspector] public Rigidbody _rigidbody;

    public float torque = 100, maxRPM, minRPM;
    public AnimationCurve enginePower;
    [Range(0f, 1f)] public float EngineSmoothTime;
    [HideInInspector] public float engineRPM, engineLoad, DownForceValue, dragAmount, KPH;

    public float[] gears;
    public float finalDrive = 3.7f;
    public int gearNum;

    [Range(0f, 2.5f)] public float forwardStifness;
    [Range(0f, 2.5f)] public float sidewaysStifness;

    [Range(0f, 2.5f)] public float forwardStifnessHandbrake;
    [Range(0f, 2.5f)] public float sidewaysStifnessHandbrake;

    public Transform centerOfMass;
    [HideInInspector] public Transform[] wheelTransforms;
    [HideInInspector] public WheelCollider[] wheelColliders;

    private Vector3 wheelPosition;
    private Quaternion wheelRotation;

    public float slip;
    public float maxSlip;

    private float substepsSpeedThreshold = 50f;
    private int substepsStepsBelowThreshold = 3000;
    private int substepsStepsAboveThreshold = 100;

    [Range(2f, 10f)] public float steeringDamper;
    [Range(0f, 10f)] public float steeringRange;
    public float maxSteer = 30f;

    public Light BrakeLight_L, BrakeLight_R;

    //assists
    public bool abs;

    //junk
    [HideInInspector] public bool vehicleChecked = false;
    private float vertical, horizontal;
    private float brakeInput, throttleInput;
    private float finalTurnAngle, radius;
    [HideInInspector] public float wheelsRPM, acceleration, totalPower, gearChangeRate;
    [HideInInspector] public float wheelsMaxRPM;
    [HideInInspector] public float engineLerpValue, brakePower;
    [Range(10f, 100f)] public float brakeInputPower;
    private WheelFrictionCurve forwardFriction, sidewaysFriction;
    public float[] wheelSlip;
    [HideInInspector] public bool engineLerp;
    private bool reverse, grounded;




    public float currentVelocity, lastFrameVelocity, Gforce;

    void Start()
    {
        findValues();

        _rigidbody.centerOfMass = centerOfMass.localPosition;
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
        Gforce = (currentVelocity - lastFrameVelocity) / (Time.deltaTime * Physics.gravity.magnitude);
        lastFrameVelocity = currentVelocity;
    }

    void OnEnable()
    {
        foreach (Transform i in gameObject.transform)
        {
            if (i.transform.name == "WheelColiders")
            {
                wheelColliders = new WheelCollider[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++)
                {
                    wheelColliders[q] = i.transform.GetChild(q).GetComponent<WheelCollider>();

                    wheelColliders[q].ConfigureVehicleSubsteps(substepsSpeedThreshold, substepsStepsBelowThreshold, substepsStepsAboveThreshold);
                }
            }

            if (i.transform.name == "WheelMeshes")
            {
                wheelTransforms = new Transform[i.transform.childCount];
                for (int q = 0; q < i.transform.childCount; q++)
                {
                    wheelTransforms[q] = i.transform.GetChild(q);
                }
            }
        }
    }

    void findValues()
    {
        //get components:
        input = GetComponent<inputs_2>();
        _rigidbody = GetComponent<Rigidbody>();

        wheelSlip = new float[wheelColliders.Length];
        vehicleChecked = true;
    }



    void moveCar()
    {
        runEngine();
        steerVehicle();

        slipDetection();
        friction();
    }



    void runEngine()
    {
        lerpEngine();
        wheelRPM();
        manual();
        runCar();
        gearbox();
        

        if (throttleInput > 0)
        {
            acceleration = throttleInput;
        }

        else
        {
            acceleration = 0;
        }

        if (!isGrounded())
        {
            acceleration = engineRPM > 1000 ? acceleration / 2 : acceleration;
        }

        if (engineRPM >= maxRPM)
        {
            setEngineLerp(maxRPM - 1000);
        }

        if (!engineLerp)
        {
            engineRPM = Mathf.Lerp(engineRPM, 1000f + Mathf.Abs(wheelsRPM) * finalDrive * (gears[gearNum]), (EngineSmoothTime * 10) * Time.deltaTime);
        }

        engineLoad = Mathf.Lerp(engineLoad, vertical - ((engineRPM - 1000) / maxRPM), (EngineSmoothTime * 10) * Time.deltaTime);
        

        wheelsMaxRPM = engineRPM / (gears[gearNum] * finalDrive);
        Debug.Log(wheelsMaxRPM);
    }



    void setEngineLerp(float num)
    {
        engineLerp = true;
        engineLerpValue = num;
    }



    void lerpEngine()
    {
        if (engineLerp)
        {
            totalPower = 0;
            engineRPM = Mathf.Lerp(engineRPM, engineLerpValue, 100 * Time.deltaTime);
            engineLerp = engineRPM <= engineLerpValue + 100 ? false : true;
        }
    }



    bool isGrounded()
    {
        if (wheelColliders[0].isGrounded && wheelColliders[1].isGrounded && wheelColliders[2].isGrounded && wheelColliders[3].isGrounded)
            return true;
        else
            return false;
    }



    void gearbox()
    { 
        totalPower = enginePower.Evaluate(engineRPM) * (gears[gearNum] * finalDrive) * acceleration;
    }


    
    void slipDetection()
    {
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            slip = wheelSlip[i];

            if (slip > maxSlip)
            {
                maxSlip = slip;
            }
        }

    }



    void runCar()
    {
        if (drive == driveType.rearWheelDrive)
        {
            for (int i = 2; i < wheelColliders.Length; i++)
            {
                if (gears[gearNum] > 0)
                {

                    if (wheelColliders[i].rpm <= wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : totalPower / (wheelColliders.Length - 2);
                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }

                else if (gears[gearNum] < 0)
                {

                    if (wheelColliders[i].rpm != wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : (totalPower * 4) / (wheelColliders.Length - 2);

                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }
            }
        }

        else if (drive == driveType.frontWheelDrive)
        {
            for (int i = 0; i < wheelColliders.Length - 2; i++)
            {
                if (gears[gearNum] > 0)
                {

                    if (wheelColliders[i].rpm <= wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : totalPower / (wheelColliders.Length - 2);
                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }

                else if (gears[gearNum] < 0)
                {

                    if (wheelColliders[i].rpm != wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : (totalPower * 4) / (wheelColliders.Length - 2);

                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }
            }
        }

        else
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (gears[gearNum] > 0)
                {

                    if (wheelColliders[i].rpm <= wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : totalPower / (wheelColliders.Length - 2);
                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }

                else if (gears[gearNum] < 0)
                {

                    if (wheelColliders[i].rpm != wheelsMaxRPM)
                    {
                        wheelColliders[i].motorTorque = (throttleInput == 0) ? 0 : (totalPower * 4) / (wheelColliders.Length - 2);

                    }

                    else
                    {
                        wheelColliders[i].motorTorque = 0;
                    }
                }
            }
        }


        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if (KPH <= 1 && KPH >= -1 && throttleInput == 0)
            {
                brakePower = 5;
            }

            else
            {
                if (brakeInput < 0 && abs == true)
                {
                    if (slip <= 0.25)
                    {
                        //brakePower = brakePower + -brakeInput * 10000;
                        brakePower = 1000000 * ((-brakeInput * 100) * (Mathf.Pow(KPH, -2)));
                    }

                    else if (brakePower > 0)
                    {
                        //brakePower = brakePower + brakeInput * brakeInputPower;
                        brakePower = 0;
                    }

                    else
                    {
                        brakePower = 0;
                    }
                }

                else if (brakeInput < 0 && abs == false)
                {   
                    if(brakeInput < 0)
                    {
                        brakePower = 1000000 * ((-brakeInput * 10) * (Mathf.Pow(KPH, -2)));
                    }

                    else
                    {
                        brakePower = 0;
                    }
                }

                else
                {
                    brakePower = 0;
                }
            }       
            
            if (brakeInput < 0)
            {
                BrakeLight_L.enabled = true;
                BrakeLight_R.enabled = true;
            }

            else
            {
                BrakeLight_L.enabled = false;
                BrakeLight_R.enabled = false;
            }

            wheelColliders[i].brakeTorque = brakePower;

            if (wheelColliders[i].rpm == 0 && KPH > 0 && brakeInput < 0)
            {
                WheelFrictionCurve friction = wheelColliders[i].forwardFriction;
                friction.stiffness = 0.1f;
                wheelColliders[i].forwardFriction = friction;
            }

            else
            {
                forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = (input.handbrake) ? forwardStifnessHandbrake : forwardStifness;
                wheelColliders[i].forwardFriction = forwardFriction;
            }
        }

        //wheelColliders[2].brakeTorque = wheelColliders[3].brakeTorque = (input.handbrake) ? Mathf.Infinity : 0f;

        _rigidbody.angularDrag = (KPH > 100) ? KPH / 100 : 0;

        if (throttleInput > 0)
        {
            _rigidbody.drag = dragAmount + (KPH / 1250);
        }

        else
        {
            _rigidbody.drag = dragAmount + (KPH / 750);
        }

        KPH = _rigidbody.velocity.magnitude * 3.6f;
    }



    void friction()
    {
        WheelHit hit;
        float sum = 0;
        float[] sidewaysSlip = new float[wheelColliders.Length];
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            if (wheelColliders[i].GetGroundHit(out hit) && i <= 3)
            {
                grounded = true;

                sum += Mathf.Abs(hit.sidewaysSlip);
            }

            else if (wheelColliders[i].GetGroundHit(out hit) && i >= 2)
            {
                forwardFriction = wheelColliders[i].forwardFriction;
                forwardFriction.stiffness = (input.handbrake) ? forwardStifnessHandbrake : forwardStifness;
                wheelColliders[i].forwardFriction = forwardFriction;

                sidewaysFriction = wheelColliders[i].sidewaysFriction;
                sidewaysFriction.stiffness = (input.handbrake) ? sidewaysStifnessHandbrake : sidewaysStifness;
                wheelColliders[i].sidewaysFriction = sidewaysFriction;

                grounded = true;

                sum += Mathf.Abs(hit.sidewaysSlip);
            }

            else grounded = false;

            wheelSlip[i] = Mathf.Abs(hit.forwardSlip) /* Mathf.Abs(hit.sidewaysSlip)*/;
            sidewaysSlip[i] = Mathf.Abs(hit.sidewaysSlip);
        }

        //sum /= wheelColliders.Length - 2 ;
        radius = (KPH > 60) ? 4 + (sum * -25) + KPH / 8 : 4;
    }



    void steerVehicle()
    {
        brakeInput = input.brakeInput;
        throttleInput = input.throttleInput;

        vertical = input.vertical;
        horizontal = Mathf.Lerp(horizontal , input.horizontal , (input.horizontal != 0) ? steeringDamper * Time.deltaTime : 3 * 2 * Time.deltaTime);

        finalTurnAngle = (radius > steeringRange) ? radius : steeringRange;

        wheelColliders[0].steerAngle = input.horizontal * maxSteer;
        wheelColliders[1].steerAngle = input.horizontal * maxSteer;
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



    void OnGUI()
    {

        float pos = 50;

        GUI.Label(new Rect(20, pos, 200, 20), "currentGear: " + gearNum.ToString("0"));
        pos += 25f;

        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineRPM, 0, maxRPM);
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "Torque: " + totalPower.ToString("0"));
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "KPH: " + KPH.ToString("0"));
        pos += 25f;

        GUI.HorizontalSlider(new Rect(20, pos, 200, 20), engineLoad, 0.0F, 1.0F);
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "brakes: " + brakePower.ToString("0"));
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "currentVelocity: " + currentVelocity.ToString("0"));
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "lastFrameVelocity: " + lastFrameVelocity.ToString("0"));
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "Gforce: " + Gforce.ToString("0"));
        pos += 25f;

        GUI.Label(new Rect(20, pos, 200, 20), "Motor Tourqe: " + wheelColliders[2].motorTorque.ToString("0"));
        pos += 25f;
    }

}
