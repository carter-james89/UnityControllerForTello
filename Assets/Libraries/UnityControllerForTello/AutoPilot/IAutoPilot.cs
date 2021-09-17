using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Interface for all Autopilot modules, designed to work with <see cref="IQuadcopter"/>
    /// </summary>
    /// <remarks>
    /// <see cref="AutoPilot"/> should be a valid solution for most autopilots, this exists if drastic changes are needed
    /// </remarks>
    public interface IAutoPilot : IInputs
    {
        /// <summary>
        /// Prepare the autopilot for activation
        /// </summary>
        /// <param name="quadToControl">The quadcopter this autopilot will control</param>
        public void Initialize(IQuadcopter quadToControl);

        /// <summary>
        /// Set the autopilot to its opposite state
        /// </summary>
        public void ToggleAutoPilot();

        /// <summary>
        /// Activate the autopilot
        /// </summary>
        public void ActivateAutoPilot();

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
      //  public PilotInputs.FlightControlValues Run(System.TimeSpan deltaTime);

        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns>
        public bool IsActive();
    }

}