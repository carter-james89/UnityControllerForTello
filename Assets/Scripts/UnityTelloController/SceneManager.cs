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

        private static bool isLoaded = false;

        Camera display2Cam;
        public InputController inputController { get; private set; }

        private void Pause()
        {
            Tello.land();
        }

        override protected void Awake()
        {
            base.Awake();
            telloManager = transform.Find("Tello Manager").GetComponent<TelloManager>();
            if (!telloManager)
                Debug.LogError("No Tello Manager Found");

            display2Cam = transform.Find("Tracking Camera (Display 2)").GetComponent<Camera>();
            //TelloManager
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
            }
            else if (sceneType == SceneType.SimOnly)
            {
                display2Cam.transform.SetParent(simulator.transform);
            }
        }

        private void Start()
        {
            inputController.CustomStart();

            if (sceneType != SceneType.SimOnly)
            {
                telloManager.CustomStart();
            }
            if (sceneType != SceneType.FlyOnly)
                simulator.CustomStart(this);

         //   inputController.ToggleAutoPilot(true);
        }

        private void Update()
        {
            connectionState = Tello.connectionState;
            inputController.CheckInputs();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                inputController.ToggleAutoPilot(!inputController.autoPilotActive);
            }

            if (Tello.connected & sceneType != SceneType.SimOnly)
            {
                if (Input.GetKeyDown(KeyCode.T))
                {
                    telloManager.OnTakeOff();
                }
                else if (Input.GetKeyDown(KeyCode.V))
                {
                    telloManager.StartProps();
                }
                else if (Input.GetKeyDown(KeyCode.L))
                {
                    Tello.land();
                }
                else if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Space))
                    BeginTracking();

                telloManager.CustomUpdate();
            }
            if (sceneType != SceneType.FlyOnly)
            {
                simulator.CustomUpdate();
            }

        }

        void BeginTracking()
        {
            Debug.Log("Begin Tracking");
            telloManager.BeginTracking();
            if (sceneType != SceneType.FlyOnly)
                simulator.ResetSimulator();
        }

        private void FixedUpdate()
        {
            if (sceneType != SceneType.FlyOnly)
                simulator.CustomFixedUpdate();
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