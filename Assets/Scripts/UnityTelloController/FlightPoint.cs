using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightPoint : MonoBehaviour {
    LineRenderer lineRenderer;
	public void CustomStart()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0,transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }

    public void SetPointOne(Vector3 point1Pos)
    {
        lineRenderer.SetPosition(1,point1Pos);
    }
}
