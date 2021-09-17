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
    public class PIDAutoPilot : AutoPilot
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
       // private System.TimeSpan telloDeltaTime;


        /// <summary>
        /// Calculate the <see cref="IInputs.FlightControlValues"/> needed to make <see cref="_quadToControl"/> match this Objects transform.position
        /// Values are calculated in global space, so they are converted via <see cref="IQuadcopter.ConvertToHeadlessInputs(PilotInputs.FlightControlValues)"/> before being returned
        /// </summary>
        /// <param name="deltaTime">The timespan since Run was called last, required for <see cref="PidController"/></param>
        /// <returns>The appropriate Yaw,Pitch,Roll, to achieve the target, in Headless space in regards to <see cref="_quadToControl"/></returns>
        public override IInputs.FlightControlValues Run()
        {
            _timeSinceLastUpdate = Time.time - prevDeltaTime;
            prevDeltaTime = Time.time;
            var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
            var deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));

            IInputs.FlightControlValues returnValues = new IInputs.FlightControlValues();

            var targetOffset = quadToControl.GetGameObject().transform.position - transform.position;

            _proximityPIDX.ProcessVariable = targetOffset.x;
            double trgtRoll = _proximityPIDX.ControlVariable(deltaTime);

            _proximityPIDY.ProcessVariable = targetOffset.y;
            double trgtElv = _proximityPIDY.ControlVariable(deltaTime);
            _proximityPIDZ.ProcessVariable = targetOffset.z;
            double trgtPitch = _proximityPIDZ.ControlVariable(deltaTime);

            var yawError = quadToControl.GetGameObject().transform.eulerAngles.y - transform.eulerAngles.y;

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
            return quadToControl.ConvertToHeadlessInputs(returnValues);
        }

        protected override void OnAutoPilotActivated()
        {
          
        }

        protected override void OnAutoPilotDeactivated()
        {
            
        }
    }
}
