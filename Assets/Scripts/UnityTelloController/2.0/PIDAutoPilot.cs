using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDAutoPilot : MonoBehaviour, IAutoPilot
{
    private PidController _proximityPIDX;
    private PidController _proximityPIDY;
    private PidController _proximityPIDZ;
    private PidController _yawPID;

    [SerializeField]
    private PIDProfile _PIDprofile;

    /// <summary>
    /// The current target <see cref="transform"/> is heading towardes 
    /// </summary>
    public Transform currentTargetPoint { get; private set; }

    [SerializeField]
    private Transform _targetPoint;

    private IQuadcopter _quadToControl;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    private void Awake()
    {
        DeactivateAutoPilot();
    }

    public void ActivateAutoPilot(IQuadcopter quadcopter)
    {
        if (!enabled)
        {
            Debug.Log("AutoPilot Enabled");
            _quadToControl = quadcopter;
            gameObject.SetActive(true);
            transform.position = quadcopter.GetGameObject().transform.position;
            UpdatePIDValues(_PIDprofile);
            enabled = true;
        }
    }
    /// <summary>
    /// Update the PID values for the controller, stored in <see cref="PIDProfile"/>
    /// </summary>
    /// <param name="newPIDprofile">The new profile to use</param>
    private void UpdatePIDValues(PIDProfile newPIDprofile)
    {
        Debug.Log("set pid values to " + newPIDprofile.name);
        _proximityPIDX = new PidController(newPIDprofile.PIDxP, newPIDprofile.PIDxI, newPIDprofile.PIDxD, 1, -1);
        _proximityPIDY = new PidController(newPIDprofile.PIDyP, newPIDprofile.PIDyI, newPIDprofile.PIDyD, 1, -1);
        _proximityPIDZ = new PidController(newPIDprofile.PIDzP, newPIDprofile.PIDzI, newPIDprofile.PIDzD, 1, -1);
        _yawPID = new PidController(newPIDprofile.yawP, newPIDprofile.yawI, newPIDprofile.yawD, 1, -1);
        _proximityPIDX.SetPoint = 0;
        _proximityPIDY.SetPoint = 0;
        _proximityPIDZ.SetPoint = 0;
        _yawPID.SetPoint = 0;
    }

    public PilotInputs.PilotInputValues RunAutoPilot(System.TimeSpan deltaTime)
    {
        PilotInputs.PilotInputValues returnValues = new PilotInputs.PilotInputValues();
        if (currentTargetPoint)
        {
          
            //var distCovered = (Time.time - pointAssignedTime) * targetSpeed;

            // float fracJourney = distCovered / targetDist;
            // transform.position = Vector3.Lerp(pointAssignedPos, currentTargetPoint.position, fracJourney);
            transform.position = Vector3.Lerp(transform.position, currentTargetPoint.position, Time.deltaTime * .5f);//, fracJourney);


            var targetOffset = _quadToControl.GetGameObject().transform.position - transform.position;

            if (targetOffset.magnitude > .1f || _quadToControl.IsSimulator())//i think this was becasue real quads have hard time holding a position
            {
                _proximityPIDX.ProcessVariable = targetOffset.x;
                double trgtRoll = _proximityPIDX.ControlVariable(deltaTime);

                _proximityPIDY.ProcessVariable = targetOffset.y;
                double trgtElv = _proximityPIDY.ControlVariable(deltaTime);
                _proximityPIDZ.ProcessVariable = targetOffset.z;
                double trgtPitch = _proximityPIDZ.ControlVariable(deltaTime);

                var yawError = _quadToControl.GetGameObject().transform.eulerAngles.y - transform.eulerAngles.y;

                if (yawError < -180)
                    yawError = 360 - System.Math.Abs(yawError);
                else if (yawError > 180)
                    yawError = -(360 - yawError);

                _yawPID.ProcessVariable = yawError;
                double trgtYaw = _yawPID.ControlVariable(deltaTime);

                returnValues.yaw = (float)trgtYaw;
                returnValues.pitch = (float)trgtPitch;
                returnValues.roll = (float)trgtRoll;
                returnValues.throttle = (float)trgtElv;
                return returnValues;
            }
        }
        returnValues.yaw = 0;
        returnValues.pitch = 0;
        returnValues.roll = 0;
        returnValues.throttle = 0;
        return returnValues;
    }

    private float _quadToTargetDist;
    private Vector3 _onTargetSetQuadPos;

    public void SetNewTarget(Transform newTarget)
    {
        if (enabled)
        {
            Debug.Log("Set new target point");
            currentTargetPoint = newTarget;
            _quadToTargetDist = Vector3.Distance(_quadToControl.GetGameObject().transform.position, currentTargetPoint.position);
            //pointAssignedEuler = _quadToControl.GetGameObject().transform.eulerAngles;
            _onTargetSetQuadPos = _quadToControl.GetGameObject().transform.position;
            //pointAssignedTime = Time.time;
            transform.eulerAngles = currentTargetPoint.eulerAngles;
        }
        else
        {
            Debug.LogWarning("Cannot set autopilot target before autopilot activatd, activate AutoPilot with 'P'");
        }
    }



    public void DeactivateAutoPilot()
    {
        if (enabled)
        {
            Debug.Log("AutoPilot Disabled");
            gameObject.SetActive(false);
            enabled = false;
        }
    }
}
