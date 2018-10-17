using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightPath : MonoBehaviour {
    public List<Transform> flightPoints;

    private void Awake()
    {
        foreach(var point in flightPoints)
        {
            DebugVector.DrawVector(point.transform,Color.red,Color.green,Color.blue,Vector3.one);
        }
    }
}
