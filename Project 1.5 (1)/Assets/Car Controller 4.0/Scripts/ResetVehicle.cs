using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetVehicle : MonoBehaviour
{
        public void resetVehicle()
        {
            var pos = transform.position;
            pos.y += 1;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
        }

        public void Quit()
        {
            Application.Quit();
        }

        public void ResetScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                resetVehicle();
            }
        }
}
