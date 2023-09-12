using UnityEngine;
using System.Collections;

public class Tires : MonoBehaviour
{
    public ParticleSystem ps;
    private ParticleSystemRenderer particleRenderer;
    private Material particleMaterial;
    public WheelCollider wheelCollider;
    public float slip;
    public float threshhold = 0;

    void Start()
    {
        particleRenderer = ps.GetComponent<ParticleSystemRenderer>();
        particleMaterial = particleRenderer.material;
    }

    void Update()
    {
        WheelHit hit;
        wheelCollider.GetGroundHit(out hit);
        //slip = (Mathf.Abs(hit.forwardSlip) + Mathf.Abs(hit.sidewaysSlip))/2;
        slip = (Mathf.Abs(hit.sidewaysSlip));

        if (slip >= 0.25f)
        {
            ps.enableEmission = true;
        }

        else
        {
            ps.enableEmission = false;
        }
    }
}