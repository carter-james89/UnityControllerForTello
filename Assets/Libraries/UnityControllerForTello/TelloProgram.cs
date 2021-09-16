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
        private PIDAutoPilot _autoPilot;

        /// <summary>
        /// Exposed inspector dropdown to easily change <see cref="PIDAutoPilot.translationStyle"/>
        /// </summary>
        public PIDAutoPilot.TranslationStyle autoPilotTransitionSytle = PIDAutoPilot.TranslationStyle.Linear;

        /// <summary>
        /// Exposed inspector field to easily change <see cref="PIDAutoPilot.currentTargetPoint"/>
        /// </summary>
        [SerializeField]
        private Transform _autoPilotTarget;

        /// <summary>
        /// The source of the Pilot inputs for this program
        /// </summary>
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
            quadcopter.Initialize(_pilotInupts, _autoPilot);
        }

        private void Update()
        {
            if (_autoPilotTarget && _autoPilot.currentTargetPoint != _autoPilotTarget)
            {
                _autoPilot.SetNewTarget(_autoPilotTarget);
            }
            if (autoPilotTransitionSytle != _autoPilot.translationStyle)
            {
                _autoPilot.SetTransitionSytle(autoPilotTransitionSytle);
            }
        }
    } 
}
