using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        //ConfigureVehicleSubsteps(float speedThreshold, int stepsBelowThreshold, int stepsAboveThreshold);
    }

    private void Steer()
    {
        steeringAngle = maxsteeringAngle * horizontalInput;
        WheelColider_FL.steerAngle = steeringAngle;
        WheelColider_FR.steerAngle = steeringAngle;
    }

    private void Accelerate()
    {
        WheelColider_FL.motorTorque = verticalInput * motorForce;
        WheelColider_FR.motorTorque = verticalInput * motorForce;
        WheelColider_RL.motorTorque = verticalInput * motorForce;
        WheelColider_RR.motorTorque = verticalInput * motorForce;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(WheelColider_FL, Wheel_FL);
        UpdateWheelPose(WheelColider_FR, Wheel_FR);
        UpdateWheelPose(WheelColider_RL, Wheel_RL);
        UpdateWheelPose(WheelColider_RR, Wheel_RR);
    }

    private void UpdateWheelPose(WheelCollider collider, Transform transform)
    {
        Vector3 _pos = transform.position;
        Quaternion _quat = transform.rotation;

        collider.GetWorldPose(out _pos, out _quat);

        transform.position = _pos;
        transform.rotation = _quat;
    }

    private void FixedUpdate()
    {
        Update();
        Steer();
        Accelerate();
        UpdateWheelPoses();
    }



    private float horizontalInput;
    private float verticalInput;
    private float steeringAngle;

    public WheelCollider WheelColider_FL, WheelColider_FR;
    public WheelCollider WheelColider_RL, WheelColider_RR;
    public Transform Wheel_FL, Wheel_FR;
    public Transform Wheel_RL, Wheel_RR;
    public float maxsteeringAngle = 30;
    public float motorForce = 50;
}
