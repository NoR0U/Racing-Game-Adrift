using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skidmarks : MonoBehaviour
{
    [SerializeField]Rigidbody Rigidbody;
    [SerializeField] Skidmark_Controller skidmarksController;



    WheelCollider wheelCollider;
    WheelHit wheelHitInfo;



    public float skidSpeedMin = 0.5f;
    public float skidIntensity = 20.0f;
    public float slipMultiplier = 10.0f;
    int lastSkid = -1;
    float lastFixedUpdateTime;



    protected void Awake()
    {
        wheelCollider = GetComponent<WheelCollider>();
        lastFixedUpdateTime = Time.time;
    }



    protected void FixedUpdate()
    {
        lastFixedUpdateTime = Time.time;
    }



    protected void LateUpdate()
    {
        if (wheelCollider.GetGroundHit(out wheelHitInfo))
        {
            Vector3 localVelocity = transform.InverseTransformDirection(Rigidbody.velocity);
            float skidTotal = Mathf.Abs(localVelocity.x);

            float wheelAngularVelocity = wheelCollider.radius * ((2 * Mathf.PI * wheelCollider.rpm) / 60);
            float carForwardVel = Vector3.Dot(Rigidbody.velocity, transform.forward);
            float wheelSpin = Mathf.Abs(carForwardVel - wheelAngularVelocity) * slipMultiplier;

            wheelSpin = Mathf.Max(0, wheelSpin * (10 - Mathf.Abs(carForwardVel)));

            skidTotal += wheelSpin;

            if (skidTotal >= skidSpeedMin)
            {
                float intensity = Mathf.Clamp01(skidTotal / slipMultiplier);
                Vector3 skidPoint = wheelHitInfo.point + (Rigidbody.velocity * (Time.time - lastFixedUpdateTime));
                lastSkid = skidmarksController.AddSkidMark(skidPoint, wheelHitInfo.normal, intensity, lastSkid);
            }

            else
            {
                lastSkid = -1;
            }
        }



        else
        {
            lastSkid = -1;
        }
    }
}
