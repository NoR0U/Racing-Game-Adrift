using UnityEngine;
using UnityEditor;
using System;

namespace Ashsvp
{
    public class SimcadeVehicleCreator : EditorWindow
    {
        GameObject preset;
        Transform VehicleBody;
        Transform wheelFL;
        Transform wheelFR;
        Transform wheelRL;
        Transform wheelRR;

        MeshRenderer bodyMesh;
        MeshRenderer wheelMesh;

        private GameObject NewVehicle;


        [MenuItem("Tools/Simcade Vehicle Physics")]

        static void OpenWindow()
        {
            SimcadeVehicleCreator vehicleCreatorWindow = (SimcadeVehicleCreator)GetWindow(typeof(SimcadeVehicleCreator));
            vehicleCreatorWindow.minSize = new Vector2(400, 300);
            vehicleCreatorWindow.Show();
        }

        private void OnGUI()
        {
            var style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = Color.green;

            GUILayout.Label("Simcade Vehicle Creator", style);
            preset = EditorGUILayout.ObjectField("Simcade Vehicle Preset", preset, typeof(GameObject), true) as GameObject;

            GUILayout.Label("Your Vehicle", style);
            VehicleBody = EditorGUILayout.ObjectField("Vehicle Body", VehicleBody, typeof(Transform), true) as Transform;

            GUILayout.Label("Wheels", style);
            wheelFL = EditorGUILayout.ObjectField("wheel FL", wheelFL, typeof(Transform), true) as Transform;
            wheelFR = EditorGUILayout.ObjectField("wheel FR", wheelFR, typeof(Transform), true) as Transform;
            wheelRL = EditorGUILayout.ObjectField("wheel RL", wheelRL, typeof(Transform), true) as Transform;
            wheelRR = EditorGUILayout.ObjectField("wheel RR", wheelRR, typeof(Transform), true) as Transform;

            GUILayout.Label("Meshes", style);
            bodyMesh = EditorGUILayout.ObjectField("Body Mesh", bodyMesh, typeof(MeshRenderer), true) as MeshRenderer;
            wheelMesh = EditorGUILayout.ObjectField("Wheel Mesh", wheelMesh, typeof(MeshRenderer), true) as MeshRenderer;

            if (GUILayout.Button("Create Simcade Vehicle"))
            {
                createVehicle();
            }

        }


        private void createVehicle()
        {
            NewVehicle = Instantiate(preset, bodyMesh.bounds.center, VehicleBody.rotation);
            NewVehicle.transform.Find("Body Collider").position = bodyMesh.bounds.center;
            NewVehicle.transform.Find("Body Collider").GetComponent<BoxCollider>().size = bodyMesh.bounds.size;
            NewVehicle.transform.Find("Body Collider").GetComponent<BoxCollider>().center = Vector3.zero;
            NewVehicle.name = "Ash_" + VehicleBody.name;
            GameObject.DestroyImmediate(NewVehicle.transform.Find("Body Mesh").GetChild(0).gameObject);
            if (NewVehicle.transform.Find("Wheels").Find("wheel FL"))
            {
                GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheel FL").Find("wheel mesh FL").GetChild(0).gameObject);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel FR"))
            {
                GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheel FR").Find("wheel mesh FR").GetChild(0).gameObject);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel RL"))
            {
                GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheel RL").Find("wheel mesh RL").GetChild(0).gameObject);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel RR"))
            {
                GameObject.DestroyImmediate(NewVehicle.transform.Find("Wheels").Find("wheel RR").Find("wheel mesh RR").GetChild(0).gameObject);
            }

            NewVehicle.transform.Find("Body Mesh").localPosition = -Vector3.up * (bodyMesh.bounds.extents.y);
            VehicleBody.parent = NewVehicle.transform.Find("Body Mesh");
            NewVehicle.transform.Find("Wheels").localPosition = Vector3.zero;



            //wheels
            if (NewVehicle.transform.Find("Wheels").Find("wheel FL"))
            {
                NewVehicle.transform.Find("Wheels").Find("wheel FL").position = wheelFL.position;
                wheelFL.parent = NewVehicle.transform.Find("Wheels").Find("wheel FL").Find("wheel mesh FL");
                wheelFL.SetSiblingIndex(0);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel FR"))
            {
                NewVehicle.transform.Find("Wheels").Find("wheel FR").position = wheelFR.position;
                wheelFR.parent = NewVehicle.transform.Find("Wheels").Find("wheel FR").Find("wheel mesh FR");
                wheelFR.SetSiblingIndex(0);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel RL"))
            {
                NewVehicle.transform.Find("Wheels").Find("wheel RL").position = wheelRL.position;
                wheelRL.parent = NewVehicle.transform.Find("Wheels").Find("wheel RL").Find("wheel mesh RL");
                wheelRL.SetSiblingIndex(0);
            }
            if (NewVehicle.transform.Find("Wheels").Find("wheel RR"))
            {
                NewVehicle.transform.Find("Wheels").Find("wheel RR").position = wheelRR.position;
                wheelRR.parent = NewVehicle.transform.Find("Wheels").Find("wheel RR").Find("wheel mesh RR");
                wheelRR.SetSiblingIndex(0);
            }

            NewVehicle.GetComponent<SimcadeVehicleController>().skidmarkWidth = wheelMesh.bounds.size.x;
            NewVehicle.GetComponent<SimcadeVehicleController>().wheelRadius = wheelMesh.bounds.extents.y;

        }
    }
}
