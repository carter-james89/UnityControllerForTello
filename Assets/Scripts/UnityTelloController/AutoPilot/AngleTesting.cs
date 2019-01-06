using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngleTesting : MonoBehaviour {
    public Transform trans0, trans1;
    public float angle;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        angle = trans0.eulerAngles.y - trans1.eulerAngles.y;
        if (angle < -180)
            angle = 360 - System.Math.Abs(angle);
        else if (angle > 180)
            angle = -(360 - angle);

        //  angle = Vector3.Dot(trans0.forward, trans1.forward);
    }
}
