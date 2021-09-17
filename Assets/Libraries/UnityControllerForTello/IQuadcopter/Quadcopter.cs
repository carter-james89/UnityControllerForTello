using System;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Base class for standard quadcopters, both real and simulated
    /// Satisfies the requirements of <see cref="IQuadcopter"/>, and handles most of the heavy lifting for basic functions
    /// </summary>
    public abstract class Quadcopter : MonoBehaviour, IQuadcopter
    {
        /// <summary>
        /// The source of Inputs from pilot, supplied in <see cref="Initialize(PilotInputs, IAutoPilot)"/>
        /// </summary>
        protected PilotInputs _pilotInputs;
        /// <summary>
        /// The AutoPilot modeule to use, supplied in <see cref="Initialize(PilotInputs, IAutoPilot)"/>
        /// </summary>
        protected IAutoPilot _autoPilot;

        public Action<bool> onAutoPilotStateChanged;

        /// <summary>
        /// Used to visalize the trail the quadcopter has traveled
        /// </summary>
        /// <remarks>
        /// No way to turn this on/off via code right now, just enable/disable the component in the inspector
        /// </remarks>
        [SerializeField]
        protected TrailVisualizer _trailVisualizer;

        /// <summary>
        /// The current status of the quadcopter
        /// </summary>
        [SerializeField]
        protected IQuadcopter.FlightStatus _flightStatus;

        /// <summary>
        /// How long has it been since the last Update, required for <see cref="PidController"/>
        /// </summary>
        /// <remarks>
        /// Exposed in Inspector solely for debuging
        /// </remarks>
        [SerializeField]
        private float _timeSinceLastUpdate;
        /// <summary>
        /// The time of the last update
        /// </summary>
        private float prevDeltaTime = 0;
        /// <summary>
        /// <see cref="prevDeltaTime"/> converted into <see cref="System.TimeSpan"/>
        /// </summary>
        private System.TimeSpan telloDeltaTime;

        /// <summary>
        /// Should the Quad run in headless mode?
        /// Yaw of the craft is ignored, in the case of <see cref="TelloQuadcopter"/>, forward is the direction the Tello was facing when powered on
        /// </summary>
        [SerializeField]
        private bool _headLessMode = false;

        /// <summary>
        /// Launch the quadcopter
        /// </summary>
        public abstract void TakeOff();
        /// <summary>
        /// Land the quadcopter
        /// </summary>
        public abstract void Land();
        /// <summary>
        /// Is this quadcopter a simulator or a real quadcopter
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSimulator();

        /// <summary>
        /// Event that is called whenever the GameObjects <see cref="Transform"/> is changed
        /// </summary>
        /// <remarks>
        /// This needs to be called manually in <see cref="onTransformChanged"/> for all inherited classes
        /// </remarks>
        public Action<Vector3, Quaternion> onTransformChanged;

        /// <summary>
        /// The point the Quad took off from, can be updated on the fly
        /// </summary>
        public Vector3 homePoint { get; private set; }

        /// <summary>
        /// The updates supplied by either the Pilot or the AutoPilot for this frame
        /// </summary>
        protected PilotInputs.PilotInputValues currentInputs;

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        public virtual void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
        {
            _flightStatus = IQuadcopter.FlightStatus.PreLaunch;
            _pilotInputs = pilotInputs;
            _autoPilot = autoPilot;

            if (_trailVisualizer)
            {
                _trailVisualizer.Initialize(this);
            }

            _pilotInputs.takeOff += TakeOff;
            _pilotInputs.land += Land;
            _pilotInputs.toggleAutoPilot += ToggleAutoPilot;
        }

        /// <summary>
        /// Run the quadcopter "frame"
        /// </summary>
        protected void UpdateQuadcopter()
        {
            _timeSinceLastUpdate = Time.time - prevDeltaTime;
            prevDeltaTime = Time.time;
            var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
            telloDeltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

            if (_autoPilot != null)
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
            return rawInputs;
        }

        /// <summary>
        /// Toggled the <see cref="IAutoPilot"/> to the opposite of its current state
        /// </summary>
        protected void ToggleAutoPilot()
        {
            if (_autoPilot != null)
            {
                if (_autoPilot.IsActive())
                {
                    DeactivateAutoPilot();
                }
                else
                {
                    ActivateAutoPilot();
                }
            }
        }
        /// <summary>
        /// If provided via <see cref="Initialize(PilotInputs, IAutoPilot)"/>, activate <see cref="_autoPilot"/>
        /// </summary>
        public void ActivateAutoPilot()
        {
            if (_autoPilot != null)
            {
                if (!_autoPilot.IsActive())
                {
                    _autoPilot.ActivateAutoPilot(this);
                    onAutoPilotStateChanged?.Invoke(true);
                }
                else
                {
                    Debug.Log("Autopilot is already active");
                }
            }
            else
            {
                Debug.LogWarning("No IAutoPilot suppled in Initialize");
            }
        }
        /// <summary>
        /// If provided via <see cref="Initialize(PilotInputs, IAutoPilot)"/>, deactivate <see cref="_autoPilot"/>
        /// </summary>
        public void DeactivateAutoPilot()
        {
            if (_autoPilot != null)
            {
                if (_autoPilot.IsActive())
                {
                    _autoPilot.DeactivateAutoPilot();
                    onAutoPilotStateChanged?.Invoke(false);
                }
                else
                {
                    Debug.Log("Autopilot is not currently active");
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

        protected virtual void OnDestroy()
        {
            if (_pilotInputs)
            {
                _pilotInputs.takeOff -= TakeOff;
                _pilotInputs.land -= Land;
                _pilotInputs.toggleAutoPilot -= ToggleAutoPilot;
            }
        }

        /// <summary>
        /// Call whenever you manually manipulate this GameObject's transform
        /// Raises event to let other systems know the quad has moved
        /// </summary>
        protected void OnTransformUpdated()
        {
            onTransformChanged?.Invoke(transform.position, transform.rotation);
        }

        public IQuadcopter.FlightStatus GetFlightStatus()
        {
            return _flightStatus;
        }

        /// <summary>
        /// Set the home point for the quad, usually at liftoff or when hover first achieved
        /// </summary>
        /// <param name="newHomePoint">The new homepoint</param>
        public void SetHomePoint(Vector3 newHomePoint)
        {
            homePoint = newHomePoint;
        }
    }
}
