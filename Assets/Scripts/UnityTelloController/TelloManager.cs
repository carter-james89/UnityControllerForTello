using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

namespace UnityControllerForTello
{
    public class TelloManager : MonoBehaviour
    {
        public bool drawFlightPath = true;
        private TelloVideoTexture telloVideoTexture;
        public float yaw, pitch, roll;

        public float posX = 0, posY, posZ;
        public float quatW;
        public float quatX;
        public float quatY;
        public float quatZ;
        public Vector3 toEuler;
        public Quaternion onTrackStartRot;

        public bool flying = false, hovering = false;

        List<FlightPoint> flightPoints;
        float trackingOffsetX, trackingOffsetY, trackingOffsetZ;
        float prevPosX, prevPosY, prevPosZ;
        Transform ground, telloGround, telloModel;
        bool tracking = false, firstTrackinFrame = true;
        Vector3 originPoint, originEuler;
        bool updateReceived = false;


        //Tello api
        public float posUncertainty;
        public bool batteryLow;
        public int batteryPercent;
        public int cameraState;
        public bool downVisualState;
        public int telloBatteryLeft;
        public int telloFlyTimeLeft;
        public int flymode;
        public int flyspeed;
        public int flyTime;
        public bool gravityState;
        public int height;
        public int imuCalibrationState;
        public bool imuState;
        public int lightStrength;
        public bool onGround = true;
        public bool powerState;
        public bool pressureState;
        public int temperatureHeight;
        public int wifiDisturb;
        public int wifiStrength;
        public bool windState;


        bool startingProps = false;
        int startUpCount = 0, startUpLimit = 100;

        public delegate void EventDelegate();

        public SceneManager sceneManager;
        InputController inputController;

        public void CustomAwake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            inputController = FindObjectOfType<InputController>();
            try
            {
                telloModel = transform.Find("Tello Model");
                ground = transform.Find("Ground");
                telloGround = transform.Find("Tello Ground");
                //telloHeight = GameObject.Find("tello (Height)").transform;
            }
            catch
            {
                Debug.LogError("Missing a gameObject");
            }
            Tello.onConnection += Tello_onConnection;
            Tello.onUpdate += Tello_onUpdate;
            Tello.onVideoData += Tello_onVideoData;

            if (telloVideoTexture == null)
                telloVideoTexture = FindObjectOfType<TelloVideoTexture>();
        }

        public void CustomStart()
        {
            Tello.startConnecting();
        }

        //This is called when you press 'T'
        public void OnTakeOff()
        {
            Debug.Log("TakeOff!");
            var preFlightPanel = GameObject.Find("Pre Flight Panel");
            if (preFlightPanel)
                preFlightPanel.SetActive(false);
            Tello.takeOff();
        }
        public void StartProps()
        {
            Debug.Log("Start Prop");
            Tello.controllerState.setAxis(.9f, -.2f, -.9f, -.2f);
            //Tello.controllerState.setAxis(-.9f, -.2f, .9f, -.2f);
            //flying = true;
            startingProps = true;
        }

        //This is called when Tello.state.flying is set to true
        void OnFlyBegin()
        {
            Debug.Log("flight begin");
            flying = true;
            //  GetPosData();
            Debug.Log("takeoff pos " + posX + " " + posY + " " + posZ);
        }

        //called from scene manager
        public void CustomUpdate()
        {
            //if (startingProps)
            //{
            //    if (startUpCount < startUpLimit)
            //    {
            //        Tello.controllerState.setAxis(-.9f, -.2f, .9f, -.2f);
            //        startUpCount++;
            //    }
            //    else
            //    {
            //        Debug.Log("Props started");
            //        startingProps = false;
            //    }
            //}
            if (updateReceived)
            {
                UpdateLocalState();

                if (flying & tracking)
                    Tracktello();
                updateReceived = false;
            }


            if (Tello.state.flying & !flying)
                OnFlyBegin();

            if (flying)
            {
                Tello.controllerState.setAxis(inputController.inputYaw, inputController.inputElv, inputController.inputRoll, inputController.inputPitch);
            }
            ////this is currently broken
            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    Debug.Log("set Pic mode");
            //    Tello.setPicVidMode(0);
            //}
        }
        //Called from Scene Manager
        public void BeginTracking()
        {
            Debug.Log("TrackingOffset : " + posX + " " + posY + " " + posZ);
            trackingOffsetX = posX;
            trackingOffsetY = posY;
            trackingOffsetZ = posZ;
            tracking = true;

            originPoint = new Vector3(posX, posY, posZ);
            originEuler = new Vector3(pitch, yaw, roll);
            onTrackStartRot = new Quaternion(quatW, quatX, quatY, quatZ);
            ground.position -= new Vector3(0, height, 0);
            flightPoints = new List<FlightPoint>();
            CreateFlightPoint();
        }

        void Tracktello()
        {
            var telloPosY = posY - originPoint.y;
            var telloPosX = posX - originPoint.x;
            var telloPosZ = posZ - originPoint.z;

            var currentPos = new Vector3(telloPosX, telloPosY, telloPosZ);
            Vector3 dif = flightPoints[flightPoints.Count - 1].transform.position - currentPos;
            var xDif = dif.x;
            var yDif = dif.y;
            var zDif = dif.z;
            if (Mathf.Abs(xDif) < 2 & Mathf.Abs(yDif) < 2 & Mathf.Abs(zDif) < 2 & dif.magnitude > .05f)
            {
                transform.position = currentPos;
                CreateFlightPoint();
                telloGround.position = transform.position - new Vector3(0, height, 0);
            }
            yaw = yaw * (180 / Mathf.PI);
            transform.eulerAngles = new Vector3(0, yaw, 0);
            pitch = pitch * (180 / Mathf.PI);
            roll = roll * (180 / Mathf.PI);
            telloModel.localEulerAngles = new Vector3(pitch - 90, 0, roll);
        }

        void CreateFlightPoint()
        {
            var newPoint = Instantiate(GameObject.Find("FlightPoint")).GetComponent<FlightPoint>();
            newPoint.transform.position = transform.position;
            newPoint.CustomStart();

            if (flightPoints.Count > 0 & drawFlightPath)
            {
                newPoint.SetPointOne(flightPoints[flightPoints.Count - 1].transform.position);
            }
            flightPoints.Add(newPoint);
        }
        private void Tello_onConnection(Tello.ConnectionState newState)
        {
            if (newState == Tello.ConnectionState.Connected)
            {
                Debug.Log("Connected to Tello, please wait for camera feed");
                // Tello.queryAttAngle();
                Tello.setMaxHeight(50);

                Tello.setPicVidMode(1); // 0: picture, 1: video
                Tello.setVideoBitRate((int)TelloController.VideoBitRate.VideoBitRateAuto);
                //Tello.setEV(0);
                Tello.requestIframe();
            }
        }
        //Dealing with telloLib
        private void Tello_onUpdate(int cmdID)
        {
            updateReceived = true;

        }
        //This just saves all the tello variables locally for viewing in the inspector
        public void UpdateLocalState()
        {
            var state = Tello.state;

            posX = Tello.state.posY;
            posY = -Tello.state.posZ;
            posZ = Tello.state.posX;

            quatW = state.quatW;
            quatX = state.quatW;
            quatY = state.quatW;
            quatZ = state.quatW;

            var eulerInfo = state.toEuler();

            pitch = (float)eulerInfo[0];
            roll = (float)eulerInfo[1];
            yaw = (float)eulerInfo[2];

            toEuler = new Vector3(pitch, roll, yaw);

            posUncertainty = state.posUncertainty;
            batteryLow = state.batteryLow;
            batteryPercent = state.batteryPercentage;
            cameraState = state.cameraState;
            downVisualState = state.downVisualState;
            telloBatteryLeft = state.droneBatteryLeft;
            telloFlyTimeLeft = state.droneFlyTimeLeft;
            flymode = state.flyMode;
            flyspeed = state.flySpeed;
            flyTime = state.flyTime;
            gravityState = state.gravityState;
            height = state.height;
            imuCalibrationState = state.imuCalibrationState;
            imuState = state.imuState;
            lightStrength = state.lightStrength;
            onGround = state.onGround;
            powerState = state.powerState;
            pressureState = state.pressureState;
            temperatureHeight = state.temperatureHeight;
            wifiDisturb = state.wifiDisturb;
            wifiStrength = state.wifiStrength;
            windState = state.windState;
        }

        public void CustomOnApplicationQuit()
        {
            Tello.onConnection -= Tello_onConnection;
            Tello.onUpdate -= Tello_onUpdate;
            Tello.onVideoData -= Tello_onVideoData;
            Tello.stopConnecting();
        }

        private void Tello_onVideoData(byte[] data)
        {
            if (telloVideoTexture != null)
                telloVideoTexture.PutVideoData(data);
        }
    }
}