using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

namespace UnityControllerForTello
{
    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        /// <summary>
        /// Different Scene Modes
        /// </summary>
        public enum SceneType {
            /// <summary>
            /// Control the Tello
            /// </summary>
            FlyOnly, 
            /// <summary>
            /// Run the simulator
            /// </summary>
            SimOnly 
        }
        /// <summary>
        /// Is the scene configured to control the Tello, or the simulator
        /// </summary>
        public SceneType sceneType;
        /// <summary>
        /// Different status of the quatcopter
        /// </summary>
        public enum FlightStatus { 
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
            Landing }
        /// <summary>
        /// The current status of the quad.
        /// </summary>
        public FlightStatus flightStatus = FlightStatus.PreLaunch;

        public Tello.ConnectionState connectionState;
        public TelloManager telloManager { get; private set; }
        public DroneSimulator simulator { get; private set; }

        [HideInInspector]
        public Transform activeDrone;

        private Camera display2Cam;


        private float timeSinceLastUpdate;
        private float prevDeltaTime = 0;
        private System.TimeSpan telloDeltaTime;
        private float telloFrameCount = 0;

        public Quaternion finalInputs { get; private set; }
        public float elv;
        public float yaw;
        public float pitch;
        public float roll;

        TelloAutoPilot autoPilot;
        public InputController inputController { get; private set; }

        private void Pause()
        {
            Tello.land();
        }

        override protected void Awake()
        {
            base.Awake();
            telloManager = transform.Find("Tello Manager").GetComponent<TelloManager>();
            autoPilot = GetComponent<TelloAutoPilot>();

            if (!telloManager)
                Debug.LogError("No Tello Manager Found");

            inputController = FindObjectOfType<InputController>();

            //so we can roll/pitch tello model without the camera moving on those axis
            var trackingCamObject = transform.Find("Tracking Camera (Display 2)");
            if (trackingCamObject)
                display2Cam = trackingCamObject.GetComponent<Camera>();
            if (sceneType != SceneType.SimOnly)
            {
                telloManager.CustomAwake(this,inputController);
                if (display2Cam)
                    display2Cam.transform.SetParent(telloManager.transform);
            }
            else
                telloManager.gameObject.SetActive(false);

          
            if (!inputController)
                Debug.LogError("Missing an input controller");
            else
                inputController.CustomAwake(this);

            //Simulator
            simulator = FindObjectOfType<DroneSimulator>();
            if (!simulator)
                Debug.Log("No tello simulator found");
            if (sceneType == SceneType.FlyOnly)
            {
                if (simulator)
                    simulator.gameObject.SetActive(false);
                activeDrone = telloManager.gameObject.transform;
            }
            else if (sceneType == SceneType.SimOnly)
            {
                Debug.Log("Begin Sim");
                activeDrone = simulator.gameObject.transform;
                display2Cam.transform.SetParent(simulator.transform);
            }
        }

        private void Start()
        {
            inputController.CustomStart();
            if (sceneType != SceneType.SimOnly)
            {
                telloManager.ConnectToTello();
            }
            if (sceneType != SceneType.FlyOnly)
                simulator.CustomStart(this);
        }
        private void Update()
        {
            inputController.GetFlightCommmands();
            if (flightStatus == FlightStatus.PreLaunch)
                inputController.CheckFlightInputs();
            //if in sim run the frame, else called from telloUpdate in flyonly
            if (sceneType == SceneType.SimOnly)
            {
                RunFrame();
            }
            else if (sceneType == SceneType.FlyOnly)
            {
                telloManager.CheckForUpdate();
            }
        }

        public void RunFrame()
        {
            connectionState = Tello.connectionState;
            //Frame info
            telloFrameCount++;
            timeSinceLastUpdate = Time.time - prevDeltaTime;
            prevDeltaTime = Time.time;
            var deltaTime1 = (int)(timeSinceLastUpdate * 1000);
            telloDeltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));
            //inputs
            var inputs = inputController.CheckFlightInputs();
            bool receivedInput = true;
            if (inputs.w == 0 & inputs.x == 0 & inputs.y == 0 & inputs.z == 0)
                receivedInput = false;
            if (receivedInput & autoPilot.enabled)
            {
                Debug.Log("AutoPilot disabled due to user input");
                ToggleAutoPilot(false);
            }
            //if we are flying the tello
            if (sceneType != SceneType.SimOnly)
            {
                switch (flightStatus)
                {
                    case FlightStatus.Launching:
                        telloManager.CheckForLaunchComplete();
                        break;
                    case FlightStatus.Flying:
                        bool validFrame = telloManager.SetTelloPosition();
                        if (!validFrame & autoPilot.enabled)
                        {
                            Debug.Log("AutoPilot disabled because Tello Lost Tracking");
                            ToggleAutoPilot(false);
                        }
                        break;
                }
            }
            if (autoPilot.enabled)
            {
                inputs = autoPilot.RunAutoPilot(telloDeltaTime);
            }
            finalInputs = CalulateFinalInputs(inputs.x, inputs.y, inputs.z, inputs.w);
            yaw = finalInputs.x;
            elv = finalInputs.y;
            roll = finalInputs.z;
            pitch = finalInputs.w;
        }

        public void TakeOff()
        {
            if (flightStatus == FlightStatus.PreLaunch)
            {
                Debug.Log("AutoTakeoff");
                switch (sceneType)
                {
                    case SceneType.FlyOnly:
                        telloManager.AutoTakeOff();
                        break;
                    case SceneType.SimOnly:
                        simulator.TakeOff();
                        break;
                    default:
                        break;
                }
            }
        }
        public void PrimeProps()
        {
            telloManager.PrimeProps();
        }
        public void Land()
        {
            telloManager.OnLand();
        }
        public void ToggleAutoPilot(bool active)
        {
            autoPilot.ToggleAutoPilot(active);
        }
        public void SetHomePoint(Vector3 globalPos)
        {
            if (autoPilot)
                autoPilot.SetHomePoint(globalPos);
        }

        private Quaternion CalulateFinalInputs(float yaw, float elv, float roll, float pitch)
        {
            bool autoPilotActive = false;
            if (autoPilot)
                autoPilotActive = autoPilot.enabled;

            if (inputController.headLessMode || autoPilotActive)
            {
                var xDir = new Vector3(roll, 0, 0);
                var yDir = new Vector3(0, 0, pitch);

                var headLessDir = transform.position + (xDir + yDir);

                var headLessDirX = Vector3.Project(headLessDir, activeDrone.right.normalized);
                roll = headLessDirX.magnitude;
                var headLessDirz = Vector3.Project(headLessDir, activeDrone.forward.normalized);
                pitch = headLessDirz.magnitude;

                var crossProduct = Vector3.Dot(headLessDirz, activeDrone.forward.normalized);

                if (crossProduct < 0)
                {
                    // roll = -roll;
                    pitch = -pitch;
                }
                crossProduct = Vector3.Dot(headLessDirX, activeDrone.right.normalized);

                if (crossProduct < 0)
                {
                    roll = -roll;
                    // pitch = -pitch;
                }
            }

            if (autoPilot.enabled)
                inputController.speed = 1;

            elv *= inputController.speed;
            roll *= inputController.speed;
            pitch *= inputController.speed;
            yaw *= inputController.speed;
            return new Quaternion(yaw, elv, roll, pitch);
        }

        void OnApplicationQuit()
        {
            if (sceneType != SceneType.SimOnly)
            {
                telloManager.CustomOnApplicationQuit();
            }
        }
    }
}