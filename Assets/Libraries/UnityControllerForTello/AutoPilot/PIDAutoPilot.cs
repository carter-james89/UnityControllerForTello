using System;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// A basic autopilot which uses <see cref="PidController"/> to fly to a given point using the offset
    /// between <see cref="IQuadcopter"/> and this object's transform.position.
    /// This <see cref="Transform"/> will interpolate between the start position and the final position depending on <see cref="TranslationStyle"/>
    /// and the quads only job is to try to reach this transform
    /// </summary>
    public class PIDAutoPilot : MonoBehaviour, IAutoPilot
    {
        /// <summary>
        /// How should the target drone move towardes its endpoint
        /// </summary>
        public enum TranslationStyle
        {
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, the start point will be <see cref="_quadToControl"/>'s position,
            /// and the end point will be the provided <see cref="Transforms"/> position.
            /// This objects's transform will interpolate between those two points in a linear fashion according to <see cref="_linearSpeed"/>
            /// </summary>
            Linear,
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, the start point will be <see cref="_quadToControl"/>'s position,
            /// and the end point will be the provided <see cref="Transforms"/> position.
            /// This objects's transform will interpolate between those two points in a non-linear fashion which will slow itself down
            /// as it approaches its final position. This is controlled by <see cref="_nonLinearSpeed"/>
            /// </summary>
            NonLinear,
            /// <summary>
            /// When <see cref="SetNewTarget(Transform)"/> is called, this objects transform will jump to that position, with no interpolation
            /// between a start and end point
            Instant,
        }
        /// <summary>
        /// The <see cref="TranslationStyle"/> that will be used to move this objects transform, can be updated on the fly via <see cref="SetTransitionSytle(TranslationStyle)"/>
        /// </summary>
        public TranslationStyle translationStyle { get; private set; } = TranslationStyle.Linear;
        /// <summary>
        /// Update the <see cref="TranslationStyle"/> of the PIDAutoPilot Target
        /// Will automatically update <see cref="PIDProfile"/> to the approprate Profile for the Mode
        /// Any custom <see cref="PIDProfile"/> assigned via <see cref="UpdatePIDProfile(PIDProfile)"/> will be overwritten
        /// </summary>
        public void SetTransitionSytle(TranslationStyle newStyle)
        {
            translationStyle = newStyle;
            switch (translationStyle)
            {
                case TranslationStyle.Linear:
                    _currentPIDProfile = _linearPIDProfile;
                    break;
                case TranslationStyle.NonLinear:
                    _currentPIDProfile = _nonLinearPIDProfile;
                    break;
                case TranslationStyle.Instant:
                    _currentPIDProfile = _instantPIDProfile;
                    break;
                default:
                    break;
            }
            UpdatePIDProfile(_currentPIDProfile);
            if (currentTargetPoint)
            {
                SetNewTarget(currentTargetPoint);
            }
        }
        /// <summary>
        /// Controls the roll input to close global X offset
        /// </summary>
        private PidController _proximityPIDX;
        /// <summary>
        /// Controls the elevation input to close global Y offset
        /// </summary>
        private PidController _proximityPIDY;
        /// <summary>
        /// Controls the pitch input to close global Z offset
        /// </summary>
        private PidController _proximityPIDZ;
        /// <summary>
        /// Controls the yaw input to match the rotaiton of this transform's rotation around Y axis
        /// </summary>
        private PidController _yawPID;

        /// <summary>
        /// <see cref="PIDProfile"/> to be used in <see cref="TranslationStyle.Linear"/>
        /// </summary>
        [SerializeField]
        private PIDProfile _linearPIDProfile;
        /// <summary>
        /// <see cref="PIDProfile"/> to be used in <see cref="TranslationStyle.NonLinear"/>
        /// </summary>
        [SerializeField]
        private PIDProfile _nonLinearPIDProfile;
        /// <summary>
        /// <see cref="PIDProfile"/> to be used in <see cref="TranslationStyle.Instant"/>
        /// </summary>
        [SerializeField]
        private PIDProfile _instantPIDProfile;
        /// <summary>
        /// The current <see cref="PIDProfile"/> being used to control the <see cref="PidController"/>s
        /// </summary>
        private PIDProfile _currentPIDProfile;

        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentTargetPoint"/>
        /// when in <see cref="TranslationStyle.Linear"/>
        /// </summary>
        [SerializeField]
        private float _linearSpeed = .5f;
        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentTargetPoint"/>
        /// when in <see cref="TranslationStyle.NonLinear"/>
        /// </summary>
        [SerializeField]
        private float _nonLinearSpeed = .5f;

        /// <summary>
        /// The distance between the <see cref="_quadToControl"/> and <see cref="currentTargetPoint"/> when <see cref="SetNewTarget(Transform)"/>
        /// </summary>
        private float _originalDistToTarget;
        /// <summary>
        /// The original position of <see cref="_quadToControl"/> when <see cref="SetNewTarget(Transform)"/>, used as the starting point for interpolation
        /// </summary>
        private Vector3 _originalQuadPos;

        /// <summary>
        /// The distance need to consider <see cref="_quadToControl"/> at <see cref="currentTargetPoint"/>
        /// </summary>
        private float _achieveTargetDist = .1f;
        /// <summary>
        /// Event to be raised when <see cref="_achieveTargetDist"/> is reached
        /// </summary>
        public Action onAchievedTarget;

        /// <summary>
        /// Is <see cref="_quadToControl"/> at <see cref="currentTargetPoint"/>
        /// </summary>
        public bool atTarget { get; private set; }

        /// <summary>
        /// The current target <see cref="transform"/> is heading towardes 
        /// </summary>
        public Transform currentTargetPoint { get; private set; }
        
        /// <summary>
        /// The <see cref="IQuadcopter"/> this autopilot is controlling, used for position information
        /// Provided via <see cref="ActivateAutoPilot(IQuadcopter)"/>
        /// </summary>
        private IQuadcopter _quadToControl;

        /// <summary>
        /// Get the <see cref="GameObject"/> this component belongs to
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns> /// <summary>
        public bool IsActive()
        {
            return enabled;
        }

        private void Awake()
        {
            DeactivateAutoPilot();
        }

        /// <summary>
        /// Activate the autopilot, will not do anything until <see cref="currentTargetPoint"/> is set via <see cref="SetNewTarget(Transform)"/>
        /// </summary>
        /// <param name="quadcopter">The quadcopter to control</param>
        public void ActivateAutoPilot(IQuadcopter quadcopter)
        {
            if (!enabled)
            {
                Debug.Log("AutoPilot Enabled");
                SetTransitionSytle(translationStyle);

                _quadToControl = quadcopter;
                gameObject.SetActive(true);
                MatchQuadTransform();

                enabled = true;
            }
        }
        /// <summary>
        /// Update the PID values for the controller, stored in <see cref="PIDProfile"/>
        /// </summary>
        /// <param name="newPIDprofile">The new profile to use</param>
        /// <remarks>
        /// If supplying a custom <see cref="PIDProfile"/>, it will be overwritten when <see cref="SetTransitionSytle(TranslationStyle)"/>
        /// </remarks>
        public void UpdatePIDProfile(PIDProfile newPIDprofile)
        {
            Debug.Log("set pid values to " + newPIDprofile.name);
            _currentPIDProfile = newPIDprofile;

            _proximityPIDX = new PidController(_currentPIDProfile.PIDxP, _currentPIDProfile.PIDxI, _currentPIDProfile.PIDxD, 1, -1);
            _proximityPIDY = new PidController(_currentPIDProfile.PIDyP, _currentPIDProfile.PIDyI, _currentPIDProfile.PIDyD, 1, -1);
            _proximityPIDZ = new PidController(_currentPIDProfile.PIDzP, _currentPIDProfile.PIDzI, _currentPIDProfile.PIDzD, 1, -1);
            _yawPID = new PidController(_currentPIDProfile.yawP, _currentPIDProfile.yawI, _currentPIDProfile.yawD, 1, -1);
            _proximityPIDX.SetPoint = 0;
            _proximityPIDY.SetPoint = 0;
            _proximityPIDZ.SetPoint = 0;
            _yawPID.SetPoint = 0;
        }

        /// <summary>
        /// Calculate the <see cref="PilotInputs.PilotInputValues"/> needed to make <see cref="_quadToControl"/> match this Objects transform.position
        /// Values are calculated in global space, so they are converted via <see cref="IQuadcopter.ConvertToHeadlessInputs(PilotInputs.PilotInputValues)"/> before being returned
        /// </summary>
        /// <param name="deltaTime">The timespan since Run was called last, required for <see cref="PidController"/></param>
        /// <returns>The appropriate Yaw,Pitch,Roll, to achieve the target, in Headless space in regards to <see cref="_quadToControl"/></returns>
        public PilotInputs.PilotInputValues Run(TimeSpan deltaTime)
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

        /// <summary>
        /// Set a new Target for <see cref="_quadToControl"/> to try and achieve
        /// </summary>
        /// <param name="newTarget">The target to match position and rotation</param>
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

        /// <summary>
        /// Maniplate this objects <see cref="Transform"/> to match <see cref="_quadToControl"/>
        /// </summary>
        private void MatchQuadTransform()
        {
            transform.position = _quadToControl.GetGameObject().transform.position;
            SetAutoPilotRot(_quadToControl.GetGameObject().transform.rotation);
        }

        /// <summary>
        /// Set the rotaion of this objects <see cref="Transform"/> to match the provided rotation, global X and global y will be nullified
        /// </summary>
        /// <param name="newRot">The new rotation of this transform</param>
        private void SetAutoPilotRot(Quaternion newRot)
        {
            var tempEuler = newRot.eulerAngles;
            tempEuler.x = 0;
            tempEuler.z = 0;
            transform.rotation = Quaternion.Euler(tempEuler);
        }

        /// <summary>
        /// If currently active, deactivate the autopilot
        /// </summary>
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
}
