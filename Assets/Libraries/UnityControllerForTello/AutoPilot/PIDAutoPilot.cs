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
        /// The current <see cref="PIDProfile"/> being used to control the <see cref="PidController"/>s
        /// </summary>
        protected PIDProfile _currentPIDProfile;



        ///// <summary>
        ///// Is <see cref="_quadToControl"/> at <see cref="currentTargetPoint"/>
        ///// </summary>
        //public bool atTarget { get; private set; }


        /// <summary>
        /// The <see cref="IQuadcopter"/> this autopilot is controlling, used for position information
        /// Provided via <see cref="ActivateAutoPilot(IQuadcopter)"/>
        /// </summary>
        protected IQuadcopter _quadToControl;

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
        public virtual void ActivateAutoPilot(IQuadcopter quadcopter)
        {
            if (!enabled)
            {
                Debug.Log("AutoPilot Enabled");
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
        public virtual PilotInputs.PilotInputValues Run(TimeSpan deltaTime)
        {
            PilotInputs.PilotInputValues returnValues = new PilotInputs.PilotInputValues();

            var targetOffset = _quadToControl.GetGameObject().transform.position - transform.position;

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

        /// <summary>
        /// Maniplate this objects <see cref="Transform"/> to match <see cref="_quadToControl"/>
        /// </summary>
        protected void MatchQuadTransform()
        {
            transform.position = _quadToControl.GetGameObject().transform.position;
            SetAutoPilotRot(_quadToControl.GetGameObject().transform.rotation);
        }

        /// <summary>
        /// Set the rotaion of this objects <see cref="Transform"/> to match the provided rotation, global X and global y will be nullified
        /// </summary>
        /// <param name="newRot">The new rotation of this transform</param>
        public void SetAutoPilotRot(Quaternion newRot)
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
