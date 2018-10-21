using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour {
    bool orbit = false;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.R))
            orbit = true;

        if (orbit)
        {
            transform.Rotate(Vector3.up,Time.deltaTime *15);
        }
	}
}
