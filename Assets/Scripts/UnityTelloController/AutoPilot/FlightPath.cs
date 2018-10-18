using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightPath : MonoBehaviour {
    public List<Transform> flightPoints;
    public enum AtTargetType{Distance, OffsetZ}
    private void Awake()
    {
        for(int i = 0;i < flightPoints.Count;i++)
        {
            DebugVector.DrawVector(flightPoints[i].transform,Color.red,Color.green,Color.blue,Vector3.one);

            if(i != flightPoints.Count - 1)
            {
                transform.LookAt(flightPoints[i+1],Vector3.up);
            }
            else
            {
                transform.LookAt(flightPoints[0],Vector3.up);
            }
        }
    }
}
