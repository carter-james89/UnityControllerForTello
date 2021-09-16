using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Interface for all Quadcopters, real or simulated 
    /// Provides the basic functions to controlling a quadcopter
    /// </summary>
    /// <remarks>
    /// For most cases, you can just inherit from <see cref="Quadcopter"/>, but this interface exists 
    /// if that class is insufficient
    /// </remarks>
    public interface IQuadcopter
    {
        public enum FlightStatus
        {
            /// <summary>
            /// Quat is on the landing pad, ready for takeoff
            /// </summary>
            PreLaunch,
            /// <summary>
            /// Props are being activated, able to take off manually
            /// </summary>
            PrimingProps,
            /// <summary>
            /// Quad has left the ground and is traveling to set height
            /// Cannont be controlled by User
            /// </summary>
            Launching,
            /// <summary>
            /// Quad is in flight mode, can be controlled by user or autopilot
            /// </summary>
            Flying,
            /// <summary>
            /// Quad is autonomously landing, cannot be controlled by user
            /// </summary>
            Landing
        }

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        public void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot);

        /// <summary>
        /// Set the Home Point the Quad will attempt to return to
        /// Usually set at launch
        /// </summary>
        public void SetHomePoint(Vector3 newHomePoint);

        /// <summary>
        /// Get the <see cref="GameObject"/> this quadcopter exists on
        /// </summary>
        public GameObject GetGameObject();

        /// <summary>
        /// Is this quadcopter a simulator or a real quadcopter?
        /// </summary>
        /// <returns></returns>
        public bool IsSimulator();

        /// <summary>
        /// What is the current <see cref="FlightStatus"/> of the quad
        /// </summary>
        /// <returns>The current flight status</returns>
        public FlightStatus GetFlightStatus();

        /// <summary>
        /// Convert a set of <see cref="PilotInputs"/> to headless space in regards to this quadcopter
        /// Headless means the quad's local orientation is not accounted for, and pitch and roll are applied on global coordinates
        /// </summary>
        /// <param name="rawInputs">The inputs to convert</param>
        /// <returns>The inputs required to match the raw inputs in global space</returns>
        public PilotInputs.PilotInputValues ConvertToHeadlessInputs(PilotInputs.PilotInputValues rawInputs);

        /// <summary>
        /// If this quadcopter has an <see cref="IAnimationClipSource"/>, activate it
        /// </summary>
        public void ActivateAutoPilot();
        /// <summary>
        /// If this quadcopter has an <see cref="IAnimationClipSource"/>, deactivate it
        /// </summary>
        public void DeactivateAutoPilot();

        /// <summary>
        /// Launch the quadcopter
        /// </summary>
        public void TakeOff();

        /// <summary>
        /// Land the quadcopter
        /// </summary>
        public void Land();
    } 
}
