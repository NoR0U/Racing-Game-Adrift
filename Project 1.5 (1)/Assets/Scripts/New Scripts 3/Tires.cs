using UnityEngine;
using System.Collections;

public class Tires : MonoBehaviour
{
    public AudioSource SkidSFX;
    public ParticleSystem ps;
    private ParticleSystemRenderer particleRenderer;
    private Material particleMaterial;
    public WheelCollider wheelCollider;

    private float pitch;
    public float slip;
    public float threshhold = 0.25f;
    [Range(0.01f, 1)] public float rampUpSpeed;
    [Range(0.01f, 1)] public float rampDownSpeed;

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

        if (slip >= threshhold)
        {
            //pitch = Random.Range(0.8f, 1);
            ps.enableEmission = true;
            SkidSFX.volume = SkidSFX.volume + rampUpSpeed;
            //SkidSFX.pitch = pitch;
        }

        else
        {
            ps.enableEmission = false;
            SkidSFX.volume = SkidSFX.volume - rampDownSpeed;
        }
    }
}