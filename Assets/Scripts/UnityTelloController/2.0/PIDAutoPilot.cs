using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDAutoPilot : MonoBehaviour, IAutoPilot
{
    public enum TranslationStyle
    {
        Linear,
        NonLinear,
        Instant,
    }

    public TranslationStyle translationStyle = TranslationStyle.Linear;

    private PidController _proximityPIDX;
    private PidController _proximityPIDY;
    private PidController _proximityPIDZ;
    private PidController _yawPID;

    [SerializeField]
    private float _linearSpeed = .5f;
    [SerializeField]
    private float _nonLinearSpeed = .5f;

    [SerializeField]
    private PIDProfile _PIDprofile;

    private float _originalDistToTarget;
    private Vector3 _originalQuadPos;


    private float _achieveTargetDist = .1f;
    public Action onAchievedTarget;
    public bool atTarget { get; private set; }

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
    public bool IsActive()
    {
        return enabled;
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
            MatchQuadTransform();

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
        //Debug.Log("set pid values to " + newPIDprofile.name);
        _proximityPIDX = new PidController(newPIDprofile.PIDxP, newPIDprofile.PIDxI, newPIDprofile.PIDxD, 1, -1);
        _proximityPIDY = new PidController(newPIDprofile.PIDyP, newPIDprofile.PIDyI, newPIDprofile.PIDyD, 1, -1);
        _proximityPIDZ = new PidController(newPIDprofile.PIDzP, newPIDprofile.PIDzI, newPIDprofile.PIDzD, 1, -1);
        _yawPID = new PidController(newPIDprofile.yawP, newPIDprofile.yawI, newPIDprofile.yawD, 1, -1);
        _proximityPIDX.SetPoint = 0;
        _proximityPIDY.SetPoint = 0;
        _proximityPIDZ.SetPoint = 0;
        _yawPID.SetPoint = 0;
    }


    /// <summary>
    /// Calculate the <see cref="PilotInputs.PilotInputValues"/> needed to reach current target
    /// Values are calculated in global space, so they are converted via <see cref="IQuadcopter.ConvertToHeadlessInputs(PilotInputs.PilotInputValues)"/> before being returned
    /// </summary>
    /// <param name="deltaTime"></param>
    /// <returns>The appropriate Yaw,Pitch,Roll, to achieve the target, in Headless space in regards to <see cref="_quadToControl"/></returns>
    public PilotInputs.PilotInputValues Run(System.TimeSpan deltaTime)
    {
        PilotInputs.PilotInputValues returnValues = new PilotInputs.PilotInputValues();
        if (currentTargetPoint)
        {
            switch (translationStyle)
            {
                case TranslationStyle.Linear:
                    var currentDist = Vector3.Distance(transform.position, currentTargetPoint.position);
                    var distTraveled = _originalDistToTarget - currentDist;
                    var fractTraveled = distTraveled / _originalDistToTarget;
                    transform.position = Vector3.Lerp(_originalQuadPos, currentTargetPoint.position, fractTraveled + (Time.deltaTime * _linearSpeed));
                    break;
                case TranslationStyle.NonLinear:
                    transform.position = Vector3.Lerp(transform.position, currentTargetPoint.position, Time.deltaTime * _nonLinearSpeed);
                    break;
                case TranslationStyle.Instant:
                    transform.position = currentTargetPoint.position;
                    break;
                default:
                    break;
            }

            var targetOffset = _quadToControl.GetGameObject().transform.position - transform.position;

            if (targetOffset.magnitude > _achieveTargetDist || _quadToControl.IsSimulator())//i think this was becasue real quads have hard time holding a position
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
                return _quadToControl.ConvertToHeadlessInputs(returnValues);
            }

            if (targetOffset.magnitude < _achieveTargetDist && !atTarget)
            {
                onAchievedTarget?.Invoke();
                atTarget = true;
            }
        }
     
        returnValues.yaw = 0;
        returnValues.pitch = 0;
        returnValues.roll = 0;
        returnValues.throttle = 0;
        return returnValues;
    }

    public void SetNewTarget(Transform newTarget)
    {
        if (enabled)
        {
            Debug.Log("Set new target point");
            MatchQuadTransform();
            currentTargetPoint = newTarget;
            _originalQuadPos = transform.position;
            _originalDistToTarget = Vector3.Distance(_originalQuadPos, currentTargetPoint.position);
            SetAutoPilotRot(currentTargetPoint.rotation);
            atTarget = false;
        }
        else
        {
            Debug.LogWarning("Cannot set autopilot target before autopilot activatd, activate AutoPilot with 'P'");
        }
    }

    private void MatchQuadTransform()
    {
        transform.position = _quadToControl.GetGameObject().transform.position;
        SetAutoPilotRot(_quadToControl.GetGameObject().transform.rotation);
    }

    private void SetAutoPilotRot(Quaternion newRot)
    {
        var tempEuler = newRot.eulerAngles;
        tempEuler.x = 0;
        transform.rotation = Quaternion.Euler(tempEuler);
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
