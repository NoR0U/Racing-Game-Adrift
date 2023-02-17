using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Scene References
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    //Variables
    [SerializeField, Range(0, 120)] private float TimeOfDay;
    public Light[] lights;      // an array of lights to control
    public float turnOnTime;    // the time of day to turn the lights on (in hours)
    public float turnOffTime;   // the time of day to turn the lights off (in hours)

    public Light FloodLight_1, FloodLight_2, FloodLight_3, FloodLight_4, FloodLight_5, FloodLight_6, FloodLight_;
    public Light CarHeadLight_L, CarHeadLight_R;


    private void Update()
    {
        if (Preset == null)
            return;

        if (Application.isPlaying)
        {
            //(Replace with a reference to the game time)
            TimeOfDay += Time.deltaTime;
            TimeOfDay %= 120;       //Modulus to ensure always between 0-24
            UpdateLighting(TimeOfDay / 120f);
        }
        else
        {
            UpdateLighting(TimeOfDay / 120f);
        }
    }


    private void UpdateLighting(float timePercent)
    {
        //Set ambient and fog
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        //If the directional light is set then rotate and set it's color, I actually rarely use the rotation because it casts tall shadows unless you clamp the value
        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
        }

    }



    //Try to find a directional light to use if we haven't set one
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;

        //Search for lighting tab sun
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        //Search scene for light that fits criteria (directional)
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(TurnLightsOnAndOff());
    }

    IEnumerator  TurnLightsOnAndOff()
    {
        while (true)
        {


            // if it's past the turn on time and before the turn off time, turn on the lights
            if (TimeOfDay > turnOnTime && TimeOfDay < turnOffTime)
            {
                foreach (Light light in lights)
                {
                    light.enabled = true;
                }
            }
            // otherwise, turn off the lights
            else
            {
                foreach (Light light in lights)
                {
                    light.enabled = false;
                }
            }

            // wait for a minute before checking the time again
            yield return new WaitForSeconds(2f);
        }
    }
}

