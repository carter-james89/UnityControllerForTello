using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Quadcopter : MonoBehaviour, IQuadcopter
{
    protected PilotInputs _pilotInputs;
    protected IAutoPilot _autoPilot;

    [SerializeField]
    protected IQuadcopter.FlightStatus _flightStatus;

    [SerializeField]
    private float _timeSinceLastUpdate;
    private float prevDeltaTime = 0;
    private System.TimeSpan telloDeltaTime;

    [SerializeField]
    private bool _headLessMode = false;

    public abstract void TakeOff();
    public abstract void Land();
    public abstract bool IsSimulator();

    protected PilotInputs.PilotInputValues currentInputs;
    protected void UpdateQuadcopter()
    {
        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        telloDeltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

        if(_autoPilot != null)
        {
            if (_autoPilot.IsActive() && _pilotInputs.UserInputingValues())
            {
                Debug.Log("Pilot Input Disabled AutoPilot");
                _autoPilot.DeactivateAutoPilot();
            }
        }
    
        _pilotInputs.GetFlightCommmands();

        currentInputs = _autoPilot.IsActive() ? _autoPilot.Run(telloDeltaTime) : _headLessMode ? ConvertToHeadlessInputs(_pilotInputs.pilotInputValues) : _pilotInputs.pilotInputValues;
    }
    /// <summary>
    /// Calculate the Roll and Pitch values to acheive the desired headless direction
    /// </summary>
    /// <remarks>
    /// Transform.Right/Forward has no positional data, which is why Vector3.Project works between headlessDir and Transform.Right/Forward
    /// </remarks>
    public virtual PilotInputs.PilotInputValues ConvertToHeadlessInputs(PilotInputs.PilotInputValues rawInputs)
    {
        var headLessDir = new Vector3(rawInputs.roll, 0, rawInputs.pitch);

        LineRenderer line = GetComponent<LineRenderer>();
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position + headLessDir);

        var headLessDirX = Vector3.Project(headLessDir, transform.right);
        rawInputs.roll = headLessDirX.magnitude;
        var headLessDirz = Vector3.Project(headLessDir, transform.forward);
        rawInputs.pitch = headLessDirz.magnitude;

        //Sign the values
        var dotProduct = Vector3.Dot(headLessDirz, transform.forward);
        if (dotProduct < 0)
        {
            rawInputs.pitch = -rawInputs.pitch;
        }
        dotProduct = Vector3.Dot(headLessDirX, transform.right);
        if (dotProduct < 0)
        {
            rawInputs.roll = -rawInputs.roll;
        }

        //if (autoPilot.enabled)
        //    inputController.speed = 1;

        //elv *= inputController.speed;
        //roll *= inputController.speed;
        //pitch *= inputController.speed;
        //yaw *= inputController.speed;
        return rawInputs;
    }

    public virtual void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
    {
        _flightStatus = IQuadcopter.FlightStatus.PreLaunch;
        _pilotInputs = pilotInputs;
        _autoPilot = autoPilot;

        _pilotInputs.takeOff += TakeOff;
        _pilotInputs.land += Land;
        _pilotInputs.toggleAutoPilot += ToggleAutoPilot;
    }

    protected void ToggleAutoPilot()
    {
        if (_autoPilot != null)
        {
            if (_autoPilot.IsActive())
            {
                _autoPilot.DeactivateAutoPilot();
            }
            else
            {
                _autoPilot.ActivateAutoPilot(this);
            }
        }
        else
        {
            Debug.LogWarning("No IAutoPilot suppled in Initialize");
        }
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }



    public IQuadcopter.FlightStatus GetFlightStatus()
    {
        return _flightStatus;
    }

}
