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

    public float turnOnTime;    // the time of day to turn the lights on (in hours)
    public float turnOffTime;   // the time of day to turn the lights off (in hours)

    public Light FloodLight_1, FloodLight_2, FloodLight_3, FloodLight_4, FloodLight_5, FloodLight_6, FloodLight_7;
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

        // if it's past the turn on time and before the turn off time, turn on the lights
        if (TimeOfDay > turnOnTime || TimeOfDay < turnOffTime)
        {
            FloodLight_1.enabled = true;
            FloodLight_2.enabled = true;
            FloodLight_3.enabled = true;
            FloodLight_4.enabled = true;
            FloodLight_5.enabled = true;
            FloodLight_6.enabled = true;
            FloodLight_7.enabled = true;

            CarHeadLight_L.enabled = true;
            CarHeadLight_R.enabled = true;
        }
        // otherwise, turn off the lights
        else
        {
            FloodLight_1.enabled = false;
            FloodLight_2.enabled = false;
            FloodLight_3.enabled = false;
            FloodLight_4.enabled = false;
            FloodLight_5.enabled = false;
            FloodLight_6.enabled = false;
            FloodLight_7.enabled = false;

            CarHeadLight_L.enabled = false;
            CarHeadLight_R.enabled = false;
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
}

