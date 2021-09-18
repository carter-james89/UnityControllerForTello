using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// An autopilot which will attempt to fly <see cref="AutoPilot.quadToControl"/> to a <see cref="Waypoint"/> in 3D space
    /// </summary>
    public class WaypointAutoPilot : PIDAutoPilot
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
            if (currentWaypoint)
            {
                SetNewWaypoint(currentWaypoint);
            }
        }

        public bool atWaypoint { get; private set; }

        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentWaypoint"/>
        /// when in <see cref="TranslationStyle.Linear"/>
        /// </summary>
        [SerializeField]
        private float _linearSpeed = .5f;
        /// <summary>
        /// The speed at which this transform will interpolate between <see cref="_originalQuadPos"/> and <see cref="currentWaypoint"/>
        /// when in <see cref="TranslationStyle.NonLinear"/>
        /// </summary>
        [SerializeField]
        private float _nonLinearSpeed = .5f;

        /// <summary>
        /// The distance between the <see cref="_quadToControl"/> and <see cref="currentWaypoint"/> when <see cref="SetNewTarget(Transform)"/>
        /// </summary>
        private float _originalDistToTarget;
        /// <summary>
        /// The original position of <see cref="_quadToControl"/> when <see cref="SetNewTarget(Transform)"/>, used as the starting point for interpolation
        /// </summary>
        private Vector3 _originalQuadPos;

        /// <summary>
        /// The distance need to consider <see cref="_quadToControl"/> at <see cref="currentWaypoint"/>
        /// </summary>
        private float _achieveTargetDist = .15f;
        /// <summary>
        /// Event to be raised when <see cref="_achieveTargetDist"/> is reached
        /// </summary>
        public Action<Waypoint> onWaypointAchieved;

        /// <summary>
        /// Event to be raised when new <see cref="_achieveTargetDist"/> is set
        /// </summary>
        public Action<Waypoint> onWaypointSet;

        /// <summary>
        /// The current target <see cref="transform"/> is heading towardes 
        /// </summary>
        public Waypoint currentWaypoint { get; private set; }

        protected override void OnAutoPilotActivated()
        {
            SetTransitionSytle(translationStyle);
            base.OnAutoPilotActivated();
        }
        /// <summary>
        /// Calculate the <see cref="PilotInputs.FlightControlValues"/> needed to make <see cref="_quadToControl"/> match this Objects transform.position
        /// Values are calculated in global space, so they are converted via <see cref="IQuadcopter.ConvertToHeadlessInputs(PilotInputs.FlightControlValues)"/> before being returned
        /// </summary>
        /// <param name="deltaTime">The timespan since Run was called last, required for <see cref="PidController"/></param>
        /// <returns>The appropriate Yaw,Pitch,Roll, to achieve the target, in Headless space in regards to <see cref="_quadToControl"/></returns>
        public override IInputs.FlightControlValues Run()
        {
            if (currentWaypoint)
            {
                switch (translationStyle)
                {
                    case TranslationStyle.Linear:
                        var currentDist = Vector3.Distance(transform.position, currentWaypoint.transform.position);
                        var distTraveled = _originalDistToTarget - currentDist;
                        var fractTraveled = distTraveled / _originalDistToTarget;
                        transform.position = Vector3.Lerp(_originalQuadPos, currentWaypoint.transform.position, fractTraveled + (Time.deltaTime * _linearSpeed));
                        break;
                    case TranslationStyle.NonLinear:
                        transform.position = Vector3.Lerp(transform.position, currentWaypoint.transform.position, Time.deltaTime * _nonLinearSpeed);
                        break;
                    case TranslationStyle.Instant:
                        transform.position = currentWaypoint.transform.position;
                        break;
                    default:
                        break;
                }

                var distToFinalTarget = Vector3.Distance(quadToControl.GetGameObject().transform.position, currentWaypoint.transform.position);
                if (distToFinalTarget < _achieveTargetDist && !atWaypoint)
                {
                    (quadToControl as SimulationQuadcopter).ResetOffset();
                    atWaypoint = true;
                    onWaypointAchieved?.Invoke(currentWaypoint);
                }
            }
            return base.Run();
        }


        /// <summary>
        /// Set a new Target for <see cref="_quadToControl"/> to try and achieve
        /// </summary>
        /// <param name="newWaypoint">The target to match position and rotation</param>
        public void SetNewWaypoint(Waypoint newWaypoint)
        {
            if (enabled)
            {
                Debug.Log("Set new target point");
                atWaypoint = false;
                MatchQuadTransform();
                currentWaypoint = newWaypoint;
                _originalQuadPos = transform.position;
                _originalDistToTarget = Vector3.Distance(_originalQuadPos, currentWaypoint.transform.position);
                SetAutoPilotRot(currentWaypoint.transform.rotation);
                onWaypointSet?.Invoke(newWaypoint);
            }
            else
            {
                Debug.LogWarning("Cannot set autopilot target before autopilot activatd, activate AutoPilot with 'P'");
            }
        }
    }
}
