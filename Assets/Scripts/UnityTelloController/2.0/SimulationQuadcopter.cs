using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationQuadcopter : Quadcopter
{
    private Rigidbody rigidBody;
    [SerializeField]
    private float inputDrag;
    [SerializeField]
    private float  drag;
    public Camera followCam;

    public override void Initialize(PilotInputs pilotInputs, IAutoPilot autoPilot)
    {
        base.Initialize(pilotInputs, autoPilot);
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        UpdateQuadcopter();
    }

    public void FixedUpdate()
    {
        if (_flightStatus != IQuadcopter.FlightStatus.PreLaunch)
        {
            rigidBody.AddForce(transform.up * 9.81f);
            bool receivingInput = false;
            var pitchInput = currentInputs.pitch;
            rigidBody.AddForce(transform.forward * pitchInput);
            if (System.Math.Abs(pitchInput) > 0)
            {
                receivingInput = true;
            }
            var elvInput = currentInputs.throttle;
            rigidBody.AddForce(transform.up * elvInput);
            if (System.Math.Abs(elvInput) > 0)
            {
                receivingInput = true;
            }
            var rollInput = currentInputs.roll;
            rigidBody.AddForce(transform.right * rollInput);
            if (System.Math.Abs(rollInput) > 0)
            {

                receivingInput = true;
            }

            var yawInput = currentInputs.yaw;
            rigidBody.AddTorque(transform.up * yawInput);
            if (System.Math.Abs(yawInput) > 0)
            {

                receivingInput = true;
            }

            if (receivingInput & rigidBody.drag != inputDrag)
            {
                rigidBody.drag = inputDrag;
                rigidBody.angularDrag = inputDrag;
            }
            else if (!receivingInput & rigidBody.drag != drag)
            {
                rigidBody.drag = drag;
                rigidBody.angularDrag = drag * .9f;
            } 
        }
    }

    public void ResetSimulator()
    {
       // transform.position = sceneManager.telloManager.transform.position;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;

    }

    public override void Land()
    {
        throw new System.NotImplementedException();
    }

    public override void TakeOff()
    {
        Debug.Log("Simulator TakeOff");
        transform.position += new Vector3(0, .8f, 0);
        gameObject.GetComponent<Rigidbody>().useGravity = true;
       // sceneManager.SetHomePoint(transform.position);
        _flightStatus = IQuadcopter.FlightStatus.Flying;
    }


    public override bool IsSimulator()
    {
        return true;
    }
}
