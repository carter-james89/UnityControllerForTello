using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Interface for all Autopilot modules, designed to work with <see cref="IQuadcopter"/>
    /// </summary>
    public interface IAutoPilot
    {
        /// <summary>
        /// Activate the autopilot
        /// </summary>
        /// <param name="quadcopter">
        /// The quadcopter this autopilot will control
        /// Requires this for positional data
        /// </param>
        public void ActivateAutoPilot(IQuadcopter quadcopter);

        /// <summary>
        /// Deactivate the autopilot
        /// </summary>
        public void DeactivateAutoPilot();

        /// <summary>
        /// Return the GameObject this autopilot is attached to
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject();

        /// <summary>
        /// Run the calculations on the autopilot to determine new inputs
        /// </summary>
        /// <param name="deltaTime">The amount of time passed since the last time run was called, required for PIDs</param>
        /// <returns>The input values required to achieve the autopilots desired translation and rotation</returns>
        public PilotInputs.PilotInputValues Run(System.TimeSpan deltaTime);

        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns>
        public bool IsActive();
    }

}