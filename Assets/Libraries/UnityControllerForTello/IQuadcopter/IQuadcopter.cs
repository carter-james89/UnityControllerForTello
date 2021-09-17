using System;
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
        /// Subscirbe to an Action that will be raised when the Quadcopter needs to abort
        /// </summary>
        /// <param name="actionToSubscribe">The function to be called</param>
        public void SubscibeToAbort(Action actionToSubscribe);
        /// <summary>
        /// Unsubscribe from an Action that will be raised when the Quadcopter needs to abort
        /// </summary>
        /// <param name="actionToUnsubscribe">The function to unsubscribe, must previously be subscirbed</param>
        public void UnsubscribeFromAbort(Action actionToUnsubscribe);

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        public void Initialize(Func<IInputs.FlightControlValues> defaultInputSource);

        /// <summary>
        /// Override the default source of <see cref="IInputs.FlightControlValues"/>
        /// </summary>
        /// <param name="inputValueSource">The new source of input values</param>
        /// <param name="abortListener">The function that will be called if the quad needs to abort, and return to default input source</param>
        public void OverrideInputSource(Func<IInputs.FlightControlValues> inputValueSource, Action abortListener);

        /// <summary>
        /// Remove the overriden source of input values, and return to default source
        /// </summary>
        /// <param name="inputValueSource">The Input source to be removed</param>
        /// <param name="abortListener">The function that was provided to be called when quad aborts</param>
        public void RemoveInputOverride(Func<IInputs.FlightControlValues> inputValueSource, Action abortListener);

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
        /// Is the quadcopter currently experiencing valid tracking
        /// </summary>
        /// <returns>The state of the tracking</returns>
        public bool IsTracking();

        /// <summary>
        /// Convert a set of <see cref="PilotInputs"/> to headless space in regards to this quadcopter
        /// Headless means the quad's local orientation is not accounted for, and pitch and roll are applied on global coordinates
        /// </summary>
        /// <param name="rawInputs">The inputs to convert</param>
        /// <returns>The inputs required to match the raw inputs in global space</returns>
        public IInputs.FlightControlValues ConvertToHeadlessInputs(IInputs.FlightControlValues rawInputs);


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
