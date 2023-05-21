using System.Collections;
using UnityEngine;
using Unity;

public class GearSystem : MonoBehaviour
{
        public float carSpeed;
        public int currentGear;
        private SimcadeVehicleController vehicleController;
        public int[] gearSpeeds = new int[] { 40, 80, 120, 160, 220 };

        public Audio AudioSystem;

        private int currentGearTemp;

        void Start()
        {
            vehicleController = GetComponent<SimcadeVehicleController>();
            currentGear = 1;

        }

        void Update()
        {
            carSpeed = Mathf.RoundToInt(vehicleController.localVehicleVelocity.magnitude * 3.6f); //car speed in Km/hr

            gearShift();


        }


        void gearShift()
        {
            for (int i = 0; i < gearSpeeds.Length; i++)
            {
                if (carSpeed > gearSpeeds[i])
                {
                    currentGear = i + 1;
                }
                else break;
            }
            if (CurrntGearProperty != currentGear)
            {
                CurrntGearProperty = currentGear;
            }

        }

        public int CurrntGearProperty
        {
            get
            {
                return currentGearTemp;
            }

            set
            {
                currentGearTemp = value;

                if (vehicleController.accelerationInput > 0 && vehicleController.localVehicleVelocity.z > 0 && !AudioSystem.GearSound.isPlaying && vehicleController.vehicleIsGrounded)
                {
                    AudioSystem.GearSound.Play();
                    StartCoroutine(shiftingGear());
                }

                AudioSystem.engineSound.volume = 0.5f;
            }
        }

        IEnumerator shiftingGear()
        {
            vehicleController.CanAccelarate = false;
            yield return new WaitForSeconds(0.3f);
            vehicleController.CanAccelarate = true;
        }
}
