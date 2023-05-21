using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSkid : MonoBehaviour
{
        [HideInInspector]
		public Skidmarks skidmarks;
		public TireSmoke smoke;
		private AudioSource skidSound;


		private const float MaxSkidIntensity = 1.0f;
		private int lastSkid = -1;
		private float lastFixedUpdateTime;

		//[HideInInspector]
		public float radius, skidTotal;
		[HideInInspector]
		public Vector3 skidPoint, normal;


		private void Start()
		{
			smoke.transform.localPosition = Vector3.up * radius;
			skidSound = GetComponent<AudioSource>();
			skidSound.mute = true;

			lastFixedUpdateTime = Time.time;
		}

		protected void FixedUpdate()
		{
			lastFixedUpdateTime = Time.time;
			SkidLogic();
		}

		public void SkidLogic()
		{
			float intensity = Mathf.Clamp01(skidTotal / MaxSkidIntensity);


			if (skidTotal > 0)
			{
				lastSkid = skidmarks.AddSkidMark(skidPoint, normal, intensity, lastSkid);
				if (smoke && intensity > 0.2f)
				{
					smoke.playSmoke();
					skidSound.mute = false;
				}
				else if (smoke)
				{
					smoke.stopSmoke();
				}
				skidSound.volume = intensity / 3;
			}
			else
			{
				skidSound.mute = true;
				lastSkid = -1;
				if (smoke)
				{
					smoke.stopSmoke();
				}
			}
		}


}

