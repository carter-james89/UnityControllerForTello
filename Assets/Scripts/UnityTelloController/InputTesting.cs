using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputTesting : MonoBehaviour {
    public float inputPitch, inputYaw, inputRoll, inputElv, flipDirection, flipDirX, speed;
    bool trigger;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //ly = Input.GetAxis("Thrustmaster Throttle Elv");
        //rx = Input.GetAxis("Thrustmaster Throttle Roll");
        //ry = -Input.GetAxis("Thrustmaster Throttle Pitch");
        //lx = Input.GetAxis("Thrustmaster Throttle Yaw");
        flipDirection = Input.GetAxis("Thrustmaster Throttle Flip");
        flipDirX = Input.GetAxis("Thrustmaster Throttle Flip X");
        speed = -Input.GetAxis("Thrustmaster Throttle Speed");
        // trigger = Input.GetButton("Key);

        if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            Debug.Log("yess");
        }
        if (trigger)
            Debug.Log("trigger squeeze");
    }
}
