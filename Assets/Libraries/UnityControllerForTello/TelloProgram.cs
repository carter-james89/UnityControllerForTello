using System;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// The program to control the DJI Tello in TelloTracking.unity
    /// This is simply the entry point for the scene, and to expose simple ways to update the program from a single component
    /// No real functionality is located here
    /// </summary>
    public class TelloProgram : MonoBehaviour
    {
        /// <summary>
        /// The quadcopter simulator, will be disabled in <see cref="SceneType.Flight"/>
        /// </summary>
        [SerializeField]
        private Quadcopter telloSimulator;
        /// <summary>
        /// The quadcopter simulator, will be disabled in <see cref="SceneType.Simulation"/>
        /// </summary>
        [SerializeField]
        private Quadcopter tello;

        /// <summary>
        /// The autopilot to provide to the <see cref="quadcopter/>
        /// </summary>
        [SerializeField]
        private WaypointAutoPilot _waypointPilot;

        /// <summary>
        /// Exposed inspector dropdown to easily change <see cref="PIDAutoPilot.translationStyle"/>
        /// </summary>
        public WaypointAutoPilot.TranslationStyle autoPilotTransitionSytle = WaypointAutoPilot.TranslationStyle.Linear;

        /// <summary>
        /// Exposed inspector field to easily change <see cref="PIDAutoPilot.currentTargetPoint"/>
        /// </summary>
        [SerializeField]
        private Transform _waypointPilotTarget;

        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
        [SerializeField]
        private PilotInputs _pilotInupts;

        [SerializeField]
        private WaypointMission _waypointMission;

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

        /// <summary>
        /// The active quadcopter according to <see cref="sceneType"/>
        /// </summary>
        public IQuadcopter quadcopter;

        void Start()
        {
            switch (sceneType)
            {
                case SceneType.Flight:
                    quadcopter = tello.GetComponent<IQuadcopter>();
                    telloSimulator.gameObject.SetActive(false);
                    break;
                case SceneType.Simulation:
                    quadcopter = telloSimulator.GetComponent<IQuadcopter>();
                    tello.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
            quadcopter.Initialize(_pilotInupts, _waypointPilot);

            (quadcopter as Quadcopter).onAutoPilotStateChanged += OnAutoPilotStateChanged;


        }

        private void OnAutoPilotStateChanged(bool state)
        {
            if (state)
            {
                _waypointMission.BeginMission(_waypointPilot);
            }
        }

        private void Update()
        {
            //if (_waypointPilotTarget && _waypointPilot.currentTargetPoint != _waypointPilotTarget)
            //{
            //    _waypointPilot.SetNewTarget(_waypointPilotTarget);
            //}
            //if (autoPilotTransitionSytle != _waypointPilot.translationStyle)
            //{
            //    _waypointPilot.SetTransitionSytle(autoPilotTransitionSytle);
            //}
        }
    } 
}
