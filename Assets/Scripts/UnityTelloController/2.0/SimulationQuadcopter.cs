using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationQuadcopter : MonoBehaviour, IQuadcopter
{
    private Rigidbody rigidBody;
    [SerializeField]
    private float inputDrag;
    [SerializeField]
    private float  drag;
    public Camera followCam;

    private IQuadcopter.FlightStatus _flightStatus;

    private PilotInputs _pilotInputs;
    private IAutoPilot _autoPilot;

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
    {
        _pilotInputs = pilotInputs;
        _autoPilot = autoPilot;
        rigidBody = GetComponent<Rigidbody>();
    }

    public void Update()
    {
     
    }

    public void FixedUpdate()
    {
            //rigidBody.AddForce(transform.up * 9.81f);
            //bool receivingInput = false;
            //var pitchInput = sceneManager.pitch;
            //rigidBody.AddForce(transform.forward * pitchInput);
            //if (System.Math.Abs(pitchInput) > 0)
            //{
            //    receivingInput = true;
            //}
            //var elvInput = sceneManager.elv;
            //rigidBody.AddForce(transform.up * elvInput);
            //if (System.Math.Abs(elvInput) > 0)
            //{
            //    receivingInput = true;
            //}
            //var rollInput = sceneManager.roll;
            //rigidBody.AddForce(transform.right * rollInput);
            //if (System.Math.Abs(rollInput) > 0)
            //{

            //    receivingInput = true;
            //}

            //var yawInput = sceneManager.yaw;
            //rigidBody.AddTorque(transform.up * yawInput);
            //if (System.Math.Abs(yawInput) > 0)
            //{

            //    receivingInput = true;
            //}

            //if (receivingInput & rigidBody.drag != inputDrag)
            //{
            //    rigidBody.drag = inputDrag;
            //    rigidBody.angularDrag = inputDrag;
            //}
            //else if (!receivingInput & rigidBody.drag != drag)
            //{
            //    rigidBody.drag = drag;
            //    rigidBody.angularDrag = drag * .9f;
            //}
    }

    public void ResetSimulator()
    {
       // transform.position = sceneManager.telloManager.transform.position;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

    }

    public void Land()
    {
        throw new System.NotImplementedException();
    }

    public void TakeOff()
    {
        Debug.Log("Simulator TakeOff");
        transform.position += new Vector3(0, .8f, 0);
        gameObject.GetComponent<Rigidbody>().useGravity = true;
       // sceneManager.SetHomePoint(transform.position);
        _flightStatus = IQuadcopter.FlightStatus.Flying;
    }

    public IQuadcopter.FlightStatus GetFlightStatus()
    {
        throw new System.NotImplementedException();
    }

    public bool IsSimulator()
    {
        return true;
    }
}
