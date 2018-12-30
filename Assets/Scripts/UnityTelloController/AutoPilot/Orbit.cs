using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {
    bool orbit = false;
    Transform target;
	// Use this for initialization
	void Start () {
        target = transform.Find("Target");
        DebugVector.DrawVector(target, Color.red, Color.green, Color.blue, Vector3.one);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
        {
            orbit = true;
            transform.position = FindObjectOfType<UnityControllerForTello.TelloManager>().transform.position;
           // FindObjectOfType<UnityControllerForTello.InputController>().autoPilotTarget = target;
        }
            

        if (orbit)
        {
            transform.Rotate(Vector3.up,Time.deltaTime *15);
        }
	}
}
