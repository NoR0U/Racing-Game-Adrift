using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController2 : MonoBehaviour
{
    public Transform centerOfMass;
    public float motorTorque = 1500;
    public float maxSteer = 20f;
    public float Steer { get; set; }
    public float Throttle { get; set; }

    private Rigidbody _rigidbody;
    private Wheel[] wheels;



    void Start()
    {
        wheels = GetComponentsInChildren<Wheel>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.centerOfMass = centerOfMass.localPosition;
    }

    void OnEnable()
    {
        WheelCollider WheelColliders = GetComponentInChildren<WheelCollider>();
        //WheelColliders.ConfigureVehicleSubsteps(1000 speedThreshold, 20 . stepsBelowThreshold, 20 . stepsAboveThreshold); //(1000,20,20) = substeps fixed in 20
    }

    void Update()
    {
        foreach (var wheel in wheels)
        {
            wheel.SteerAngle = Steer * maxSteer;
            wheel.Torque = Throttle * motorTorque;
        }
    }
}
