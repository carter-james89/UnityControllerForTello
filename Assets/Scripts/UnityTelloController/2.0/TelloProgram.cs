using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelloProgram : MonoBehaviour
{
    [SerializeField]
    private GameObject telloSimulator;
    [SerializeField]
    private GameObject tello;

    [SerializeField]
    private PIDAutoPilot _autoPilot;

    [SerializeField]
    private PIDProfile _PIDprofile;

    [SerializeField]
    private Transform _autoPilotTarget;

    [SerializeField]
    private PilotInputs _pilotInupts;



    /// <summary>
    /// Different Scene Modes
    /// </summary>
    public enum SceneType
    {
        /// <summary>
        /// Control the Tello
        /// </summary>
        Flight,
        /// <summary>
        /// Run the simulator
        /// </summary>
        Simulation
    }

    /// <summary>
    /// Is the scene configured to control the Tello, or the simulator
    /// </summary>
    public SceneType sceneType;

    public IQuadcopter quadcopter;

    void Start()
    {

        switch (sceneType)
        {
            case SceneType.Flight:
                quadcopter = tello.GetComponent<IQuadcopter>();
                telloSimulator.SetActive(false);
                break;
            case SceneType.Simulation:
                quadcopter = telloSimulator.GetComponent<IQuadcopter>();
                tello.SetActive(false);
                break;
            default:
                break;
        }

        quadcopter.Initialize(_pilotInupts, _autoPilot);
    }

    private void Update()
    {
        if (_autoPilot.currentTargetPoint != _autoPilotTarget)
        {
            _autoPilot.SetNewTarget(_autoPilotTarget);
        }
    }
}
