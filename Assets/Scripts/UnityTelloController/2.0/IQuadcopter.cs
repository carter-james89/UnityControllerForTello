using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public GameObject GetGameObject();

    public bool IsSimulator();

    public FlightStatus GetFlightStatus();

    public PilotInputs.PilotInputValues ConvertToHeadlessInputs(PilotInputs.PilotInputValues rawInputs);


    public void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot);


    public void TakeOff();

    public void Land();
}
