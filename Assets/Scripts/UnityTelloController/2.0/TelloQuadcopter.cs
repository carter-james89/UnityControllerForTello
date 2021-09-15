using System.Collections;
using System.Collections.Generic;
using TelloLib;
using UnityEngine;

public class TelloQuadcopter : MonoBehaviour, IQuadcopter
{
    [SerializeField]
    private TelloVideoTexture telloVideoTexture;

    private IQuadcopter.FlightStatus flightStatus;



    public Tello.ConnectionState connectionState;
    [SerializeField]
    private int _telloFrameCount = 0;
    [SerializeField]
    private float _timeSinceLastUpdate;
    private float prevDeltaTime = 0;
    private System.TimeSpan telloDeltaTime;


    private PilotInputs _pilotInputs;
    private IAutoPilot _autoPilot;


    //  public bool telloUpdateReceived { get; private set; }

    public void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
    {
        _pilotInputs = pilotInputs;
        _autoPilot = autoPilot;
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
        connectionState = Tello.connectionState;
        //Frame info
        _telloFrameCount++;
        _timeSinceLastUpdate = Time.time - prevDeltaTime;
        prevDeltaTime = Time.time;
        var deltaTime1 = (int)(_timeSinceLastUpdate * 1000);
        telloDeltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));
        //inputs
        var pilotInputs = _pilotInputs.pilotInputValues;


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
    public void Land()
    {
        //var preFlightPanel = GameObject.Find("Pre Flight Panel");
        //if (preFlightPanel)
        //    preFlightPanel.SetActive(false);
        Tello.takeOff();
        flightStatus = IQuadcopter.FlightStatus.Launching;
    }

    public void TakeOff()
    {
        throw new System.NotImplementedException();
    }

    public IQuadcopter.FlightStatus GetFlightStatus()
    {
        throw new System.NotImplementedException();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public bool IsSimulator()
    {
        return false;
    }
}
