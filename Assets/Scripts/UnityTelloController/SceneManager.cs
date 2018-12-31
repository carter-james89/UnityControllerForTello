using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

namespace UnityControllerForTello
{
    public class SceneManager : SingletonMonoBehaviour<SceneManager>
    {
        public enum SceneType { FlyOnly, SimOnly, FlySim }
        public SceneType sceneType;
        public Tello.ConnectionState connectionState;
        public TelloManager telloManager { get; private set; }
        public DroneSimulator simulator { get; private set; }

        public bool headLessMode;

        public Transform activeDrone;

        Camera display2Cam;

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

            //so we can roll/pitch tello model without the camera moving on those axis
            display2Cam = transform.Find("Tracking Camera (Display 2)").GetComponent<Camera>();
            if (sceneType != SceneType.SimOnly)
            {
                telloManager.CustomAwake();
                display2Cam.transform.SetParent(telloManager.transform);
            }
            else
                telloManager.gameObject.SetActive(false);

            inputController = FindObjectOfType<InputController>();
            if (!inputController)
                Debug.LogError("Missing an input controller");
            else
                inputController.CustomAwake(this);

            //Simulator
            simulator = FindObjectOfType<DroneSimulator>();
            if (!simulator)
                Debug.LogError("No tello simulator found");
            if (sceneType == SceneType.FlyOnly)
            {
                simulator.gameObject.SetActive(false);
                activeDrone = telloManager.gameObject.transform;
            }
            else if (sceneType == SceneType.SimOnly)
            {
                activeDrone = simulator.gameObject.transform;
                display2Cam.transform.SetParent(simulator.transform);
            }
        }

        private void Start()
        {
            inputController.CustomStart();
            if (sceneType != SceneType.SimOnly)
            {
                //  telloManager.CustomStart();
                telloManager.ConnectToTello();
            }
            if (sceneType != SceneType.FlyOnly)
                simulator.CustomStart(this);
        }
        private void Update()
        {
            //if in sim run the frame, else called from telloUpdate in flyonly
            if(sceneType == SceneType.SimOnly)
            {
                RunFrame();
            }
        }

        public Quaternion finalInputs { get; private set; }
        public float elv;
        public float yaw;
        public float pitch;
        public float roll;
        public void RunFrame()
        {
            connectionState = Tello.connectionState;

            var inputs = inputController.CheckInputs();
            bool receivedInput = true;
            if (inputs.w == 0 & inputs.x == 0 & inputs.y == 0 & inputs.z == 0)
                receivedInput = false;
            if (receivedInput & autoPilot.enabled)
            {
                Debug.Log("AutoPilot disabled due to user input");
                autoPilot.ToggleAutoPilot(false);
            }

            if (autoPilot.enabled)
            {
                inputs = autoPilot.RunAutoPilot();
            }
            finalInputs = CalulateFinalInputs(inputs.x, inputs.y, inputs.z, inputs.w);
            yaw = finalInputs.x;
            elv = finalInputs.y;
            roll = finalInputs.z;
            pitch = finalInputs.w;

            //yaw = inputs.x;
            //elv = inputs.y;
            //roll = inputs.z;
            //pitch = inputs.w;

            //switch (sceneType)
            //{
            //    case SceneType.FlyOnly:
            //        telloManager.SendTelloInputs(finalInputs);
            //        break;
            //    case SceneType.SimOnly:

            //        break;             
            //}

            //if()

            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    inputController.ToggleAutoPilot(!inputController.autoPilotActive);
            //}
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    inputController.BeginFlightPath(FindObjectOfType<FlightPath>());
            //}

            //if (Tello.connected & sceneType != SceneType.SimOnly)
            //{
            //    if (Input.GetKeyDown(KeyCode.T))
            //    {
            //        telloManager.OnTakeOff();
            //    }
            //    else if (Input.GetKeyDown(KeyCode.V))
            //    {
            //        telloManager.StartProps();
            //    }
            //    else if (Input.GetKeyDown(KeyCode.L))
            //    {
            //        telloManager.OnLand();
            //    }
            //    else if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Space))
            //        BeginTracking();

            //    telloManager.CustomUpdate();
            //}
            //if (sceneType != SceneType.FlyOnly)
            //{
            //    simulator.CustomUpdate();
            //}
        }

        public void TakeOff()
        {
            telloManager.AutoTakeOff();
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
            headLessMode = true;
            autoPilot.ToggleAutoPilot(active);
        }
        //if fly mode, called from Tello_onUpdate
        //if sim mode, called from update every couple of seconds.
        //public void CheckFlightInputs()
        //{
        //    inputController.CheckInputs();
        //}

        void BeginTracking()
        {
            Debug.Log("Begin Tracking");
            telloManager.BeginTracking();
            if (sceneType != SceneType.FlyOnly)
                simulator.ResetSimulator();
        }

        Quaternion CalulateFinalInputs(float yaw, float elv, float roll, float pitch)
        {
            if (headLessMode)
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