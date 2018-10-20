using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightPath : MonoBehaviour {
    public List<Transform> flightPoints;
    public enum AtTargetType{Distance, OffsetZ}
    private void Start()
    {
        for(int i = 0;i < flightPoints.Count;i++)
        {
            DebugVector.DrawVector(flightPoints[i].transform,Color.red,Color.green,Color.blue,Vector3.one);

            if (i != 0)
            {
                flightPoints[i].LookAt(flightPoints[i -1], Vector3.up);
                
            }
            else
            {
                flightPoints[i].LookAt(flightPoints[flightPoints.Count - 1], Vector3.up);
            }
            flightPoints[i].localEulerAngles += new Vector3(0, 180, 0);
        }
    }
}
