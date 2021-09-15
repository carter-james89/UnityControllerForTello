using System.Collections;
using System.Collections.Generic;
using TelloLib;
using UnityEngine;

public class TelloQuadcopter : Quadcopter
{
    [SerializeField]
    private TelloVideoTexture telloVideoTexture;

    private IQuadcopter.FlightStatus flightStatus;

    private bool validTrackingFrame;

    public Tello.ConnectionState connectionState;
    [SerializeField]
    private int _telloFrameCount = 0;

    private List<FlightPoint> flightPoints;

    private float elevationOffset;

    private Vector3 prevDeltaPos;

    public bool drawFlightPath = true;

    private Transform flightPointsParent;

    //  public bool telloUpdateReceived { get; private set; }

    public override void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
    {
        base.Initialize(pilotInputs,autoPilot);
        ConnectToTello();
    }

    /// <summary>
    /// Attempt to connect to the quadcopter via <see cref="Tello"/>
    /// Must be connected to quadcopter via wifi
    /// </summary>
    public void ConnectToTello()
    {
        Tello.onConnection += Tello_onConnection;
        Tello.onUpdate += Tello_onUpdate;
        Tello.onVideoData += Tello_onVideoData;
        Tello.startConnecting();
    }

    private void Tello_onConnection(Tello.ConnectionState newState)
    {
        if (newState == Tello.ConnectionState.Connected)
        {
            Debug.Log("Connected to Tello, please wait for camera feed");
            Tello.setPicVidMode(1); // 0: picture, 1: video
            Tello.setVideoBitRate((int)TelloController.VideoBitRate.VideoBitRateAuto);
            Tello.requestIframe();
        }
    }
    //Dealing with telloLib
    private void Tello_onUpdate(int cmdID)
    {
        //telloUpdateReceived = true;
        _telloFrameCount++;
        //Frame info


        SyncDataWithTello();

        switch (flightStatus)
        {
            case IQuadcopter.FlightStatus.Launching:
                CheckForLaunchComplete();
                break;
            case IQuadcopter.FlightStatus.Flying:
                bool validFrame = SetTelloPosition();
                if (!validFrame & _autoPilot.IsActive())
                {
                    Debug.Log("AutoPilot disabled because Tello Lost Tracking");
                    _autoPilot.DeactivateAutoPilot();
                }
                break;
        }
        //inputs
        UpdateQuadcopter();
    }

    public void CheckForLaunchComplete()
    {
        //if (flymode == 6)
        //{
        //    originPoint = GetCurrentPos();
        //   // Debug.Log("Y Offset " + originPoint + " tello frame count " + telloFrameCount);
        //    originEuler = new Vector3(pitch, yaw, roll);
        //    // onTrackStartRot = new Quaternion(quatW, quatX, quatY, quatZ);
        //    ground.position -= new Vector3(0, height * .1f, 0);
        //    flightPoints = new List<FlightPoint>();
        //    CreateFlightPoint();

        //    //Debug.Log("tello height set to " + height * .1f);
        //    telloGround.position = transform.position - new Vector3(0, height * .1f, 0);
        //    elevationOffset = height * .1f;
        //    sceneManager.SetHomePoint(new Vector3(0, height * .1f, 0));
        //    flightStatus = IQuadcopter.FlightStatus.Flying;
        //}
    }
    private Vector3 GetCurrentPos()
    {
        //var telloPosY = posY - originPoint.y;
        //var telloPosX = posX - originPoint.x;
        //var telloPosZ = posZ - originPoint.z;

        //return new Vector3(telloPosX, telloPosY, telloPosZ);
        return Vector3.zero;
    }

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
    }

    public bool SetTelloPosition()
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
            transform.position += new Vector3(0, elevationOffset, 0);
            prevDeltaPos = dif;
            Vector3 flightPointDif = flightPoints[flightPoints.Count - 1].transform.position - currentPos;
            if (flightPointDif.magnitude > .001f)
            {
                CreateFlightPoint();
            }
            yaw = yaw * (180 / Mathf.PI);
            transform.eulerAngles = new Vector3(0, yaw, 0);
            pitch = pitch * (180 / Mathf.PI);
            roll = roll * (180 / Mathf.PI);
            transform.localEulerAngles = new Vector3(pitch - 90, 0, roll);
        }
        else
        {
            Debug.Log("Tracking lost " + _telloFrameCount);
            validTrackingFrame = false;
        }
        return validTrackingFrame;
    }

    private void Tello_onVideoData(byte[] data)
    {
        if (telloVideoTexture != null)
        {
            telloVideoTexture.PutVideoData(data);
        }
        else
        {
            Debug.LogWarning("Recieving video, but telloVideoTexture is null, assign in inspector");
        }
    }
    public override void Land()
    {
        //var preFlightPanel = GameObject.Find("Pre Flight Panel");
        //if (preFlightPanel)
        //    preFlightPanel.SetActive(false);
        Tello.takeOff();
        flightStatus = IQuadcopter.FlightStatus.Launching;
    }

    private void CreateFlightPoint()
    {
        var newPoint = Instantiate(GameObject.Find("FlightPoint")).GetComponent<FlightPoint>();
        newPoint.transform.position = transform.position;
        newPoint.transform.SetParent(flightPointsParent);
        newPoint.CustomStart();

        if (flightPoints.Count > 0 & drawFlightPath)
        {
            newPoint.SetPointOne(flightPoints[flightPoints.Count - 1].transform.position);
        }
        flightPoints.Add(newPoint);
    }

    public override void TakeOff()
    {
        throw new System.NotImplementedException();
    }



    public override bool IsSimulator()
    {
        return false;
    }

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

    public float posX = 0, posY, posZ;
    public float quatW;
    public float quatX;
    public float quatY;
    public float quatZ;

    public float yaw, pitch, roll;

    public Vector3 toEuler;
}
