using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDProfile : MonoBehaviour {
    public float PIDxP = .1f, PIDxI = 0, PIDxD = .0f;
    public float PIDyP = .1f, PIDyI = 0, PIDyD = .0f;
    public float PIDzP = .1f, PIDzI = 0, PIDzD = .0f;
    public float yawP, yawI, yawD;
}
