using System;
using TelloLib;
using UnityEngine;


namespace UnityControllerForTello
{
    /// <summary>
    /// Quadcopter class to control the real world DJI Tello
    /// Uses the <see cref="Tello"/> library to send and recieve data from the craft
    /// </summary>
    public class TelloQuadcopter : Quadcopter
    {
        /// <summary>
        /// Video feed to display the camera from the Tello
        /// </summary>
        [SerializeField]
        private TelloVideoFeed _videoFeed;

        /// <summary>
        /// The last frame recieved updated via <see cref="Tello_onUpdate(int)"/>
        /// </summary>
        private int _lastTelloUpdateFrame;

        /// <summary>
        /// Is the Tello tracking accurate this frame?
        /// </summary>
        /// <remarks>
        /// In poor lighting conditions or for no reason at all sometimes the position tracking of the Tello is way off
        /// The deltaposition from the last valid frame is used to determine if there is an unreasonable jump
        /// </remarks>
        private bool validTrackingFrame;

        /// <summary>
        /// The current connection state with the Tello, must be <see cref="Tello.ConnectionState.Connected"/> to control
        /// </summary>
        public Tello.ConnectionState connectionState;

        /// <summary>
        /// How many packkages have we recieved from the Tello
        /// </summary>
        [SerializeField]
        private int _telloFrameCount = 0;

        /// <summary>
        /// The offset of the tracking values when tracking first achieved after liftoff
        /// </summary>
        /// <remarks>
        /// This is a weird bug either with the <see cref="Tello"/> library or the Tello itself
        /// When you take off the position of the Tello is (0,0,0)
        /// Once it achieves its hover, a huge and random offset is applied to the position, which needs to be accounted for for all 
        /// future positioning data
        /// </remarks>
        private Vector3 _trackingFoundOffset;
        /// <summary>
        /// The offset of elevation when tracking first achieved
        /// </summary>
        private float _elevationOffset;

        /// <summary>
        /// Static position of the takeoff position, set at takeoff
        /// </summary>
        [SerializeField]
        private Transform _takeOffGround;
        /// <summary>
        /// Quad representation of where the Tello sensor thinks the ground is, set continuously
        /// </summary>
        [SerializeField]
        private Transform _sensorGround;

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        public override void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
        {
            base.Initialize(pilotInputs, autoPilot);
            ConnectToTello();
        }

        /// <summary>
        /// Attempt to connect to the Tello via <see cref="Tello"/> Library
        /// Must be connected to quadcopter via wifi
        /// </summary>
        public void ConnectToTello()
        {
            Tello.onConnection += Tello_onStateChanged;
            Tello.onUpdate += Tello_onUpdate;
            if (_videoFeed)
            {
                _videoFeed.InitializeFeed(this);
            }
            else
            {
                Debug.LogWarning("No TelloVideoFeed supplied in inspector, will not display video feed from Tello");
            }
            Tello.startConnecting();
        }

        /// <summary>
        /// Called from <see cref="Tello.onConnection"/> when the state of the connection with the Tello is changed
        /// </summary>
        private void Tello_onStateChanged(Tello.ConnectionState newState)
        {
            Debug.Log("Tello State Updated : " + newState);
            if (newState == Tello.ConnectionState.Connected)
            {
                Debug.Log("Connected to Tello, please wait for camera feed");
                Tello.setPicVidMode(1); // 0: picture, 1: video
                Tello.setVideoBitRate((int)TelloController.VideoBitRate.VideoBitRateAuto);
                Tello.requestIframe();
            }
            else if (newState == Tello.ConnectionState.Disconnected)
            {
                Debug.Log("Disconnected from Tello");
            }
        }
        /// <summary>
        /// Called from <see cref="Tello.onUpdate"/> when an update a package is recieved from the Tello
        /// </summary>
        ///<remarks>
        ///<see cref="Tello_onUpdate(int)"/> happens on its own thread, and to interact with unity/inputs we need to use <see cref="Update"/>
        ///This simply records that an update has been recieved from the Tello, and will be handled in the next <see cref="Update"/>
        /// </remarks>
        private void Tello_onUpdate(int cmdID)
        {
            _telloFrameCount++;
            _lastTelloUpdateFrame = Time.frameCount;
        }
        private void Update()
        {
            if (_lastTelloUpdateFrame != Time.frameCount)
            {
                SyncDataWithTello();
                switch (_flightStatus)
                {
                    case IQuadcopter.FlightStatus.Launching:
                        CheckForLaunchComplete();
                        break;
                    case IQuadcopter.FlightStatus.Flying:
                        bool validFrame = false;
                        try
                        {
                            validFrame = SetVirtualTelloPosition();
                        }
                        catch (Exception e)
                        {
                            Debug.Log(e + " : emergency land");
                        }
                        if (!validFrame & _autoPilot.IsActive())
                        {
                            Debug.Log("AutoPilot disabled because Tello Lost Tracking");
                            _autoPilot.DeactivateAutoPilot();
                        }
                        break;
                }
                UpdateQuadcopter(); //need to run in update to get Input Values for this frame
                SendTelloInputs();
            }
        }
        /// <summary>
        /// Sent the Inputs from either the <see cref="PilotInputs"/> or <see cref="IAutoPilot"/> to the Tello
        /// </summary>
        public void SendTelloInputs()
        {
            if (_flightStatus != IQuadcopter.FlightStatus.PreLaunch)
            {
                Tello.controllerState.setAxis(currentInputs.yaw, currentInputs.throttle, currentInputs.roll, currentInputs.pitch);
            }
        }

        /// <summary>
        /// Check to see if the Tello has finished its auto takeoff
        /// </summary>
        /// <remarks>
        /// This is a weird but either with the <see cref="Tello"/> library or the Tello itself
        /// When you take off the position of the Tello is (0,0,0)
        /// Once it achieves its hover hover, a huge and random offset is applied to the position, which needs to be accounted for
        /// Also difficult to determin when this happens. <see cref="flymode"/> used to work but as of 3.0 it isnt realiable unless you also check for <see cref="flying"/>
        /// And even that isnt great as there is a long delay
        /// </remarks>
        public void CheckForLaunchComplete()
        {
            if (flymode == 6 && flying)
            {
                Debug.Log("launch complete");
                _trackingFoundOffset = new Vector3(posX, posY, posZ);

                SetSensorGround();
                if (_takeOffGround)
                {
                    _takeOffGround.transform.position = transform.position - new Vector3(0, height * .1f, 0);
                }

                _elevationOffset = height * .1f;
                SetHomePoint(new Vector3(0, height * .1f, 0));
                _flightStatus = IQuadcopter.FlightStatus.Flying;
            }
        }
        /// <summary>
        /// Set the <see cref="sensorGround"/> position to viusalize where the Tello thinks the ground is
        /// </summary>
        public void SetSensorGround()
        {
            if (_sensorGround)
            {
                _sensorGround.position = transform.position - new Vector3(0, height * .1f, 0);
            }
        }

        /// <summary>
        /// Get the current position of the Tello, taking into account the <see cref="_trackingFoundOffset"/> described in <see cref="CheckForLaunchComplete"/>
        /// </summary>
        /// <returns>The Global Postion of the Tello</returns>
        private Vector3 GetCurrentPos()
        {
            return new Vector3(posX - _trackingFoundOffset.x, posY - _trackingFoundOffset.y, posZ - _trackingFoundOffset.z);
        }

        /// <summary>
        /// Store all the information from the Tello package locally
        /// </summary>
        /// <remmarks>
        /// Not all values are guaranteed to work or be accurate, they come from Tello and <see cref="Tello"/> library
        /// </remmarks>
        public void SyncDataWithTello()
        {
            connectionState = Tello.connectionState;

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
            flying = state.flying;

            hover = state.droneHover;
        }

        /// <summary>
        /// Set the position of the virtual Tello in the Unity environment
        /// </summary>
        /// <returns>Is this an accurate frame, <see cref="validTrackingFrame"/> </returns>
        public bool SetVirtualTelloPosition()
        {
            validTrackingFrame = true;
            var currentPos = GetCurrentPos();
            Vector3 dif = currentPos - transform.position;
            var xDif = dif.x;
            var yDif = dif.y;
            var zDif = dif.z;

            //valid tello frame
            if (Mathf.Abs(xDif) < 2 & Mathf.Abs(yDif) < 2 & Mathf.Abs(zDif) < 2)
            {
                transform.position = currentPos;
                transform.position += new Vector3(0, _elevationOffset, 0);
                yaw = yaw * (180 / Mathf.PI);
                pitch = (pitch * (180 / Mathf.PI));
                roll = roll * (180 / Mathf.PI);
                transform.localEulerAngles = new Vector3(-pitch, yaw, roll);

                OnTransformUpdated();
            }
            else
            {
                Debug.Log("Tracking lost " + _telloFrameCount);
                validTrackingFrame = false;
            }
            return validTrackingFrame;
        }

        /// <summary>
        /// Launch the Tello via its auto liftoff feature
        /// </summary>
        public override void TakeOff()
        {
            if (connectionState == Tello.ConnectionState.Connected)
            {
                Debug.Log("TakeOff");
                Tello.takeOff();
                _flightStatus = IQuadcopter.FlightStatus.Launching;
            }
            else
            {
                Debug.LogWarning("Not connected to tello prior to takeoff command : " + connectionState);
            }
        }
        /// <summary>
        /// Land the Tello via its auto land feature
        /// </summary>
        public override void Land()
        {
            if (connectionState == Tello.ConnectionState.Connected)
            {
                Debug.Log("Land");
                Tello.land();
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
                _flightStatus = IQuadcopter.FlightStatus.Landing;
            }
            else
            {
                Debug.LogWarning("Not connected to Tello at time of land command : " + connectionState);
            }
        }

        public override bool IsSimulator()
        {
            return false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Tello.onConnection -= Tello_onStateChanged;
            Tello.onUpdate -= Tello_onUpdate;
        }

        //Tello api, public so they can be seen in inspector
        public bool flying;
        public bool hover;
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
        public float posX = 0, posY, posZ;
        public float quatW;
        public float quatX;
        public float quatY;
        public float quatZ;
        public float yaw, pitch, roll;
        public Vector3 toEuler;
    } 
}
