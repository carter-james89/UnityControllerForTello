using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// The controller for the simulated drone which is active in <see cref="SceneManager.sceneType"/>
    /// Lives on the "Drone Simulator" GameObject
    /// </summary>
    public class DroneSimulator : MonoBehaviour
    {
        private Rigidbody rigidBody;
        private InputController inputController;
        public float inputDrag, drag;
        public Camera followCam;
        private SceneManager sceneManager;
        

        public void CustomStart(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
            rigidBody = GetComponent<Rigidbody>();
            inputController = sceneManager.inputController;
        }

        public void CustomUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button1))
            {
                ResetSimulator();
            }
        }
        public void TakeOff()
        {
            transform.position += new Vector3(0, .8f, 0);
            gameObject.GetComponent<Rigidbody>().useGravity = true;
            sceneManager.SetHomePoint(transform.position);
            sceneManager.flightStatus = SceneManager.FlightStatus.Flying;
        }
        public void FixedUpdate()
        {
            if(sceneManager.flightStatus == SceneManager.FlightStatus.Flying)
            {
                rigidBody.AddForce(transform.up * 9.81f);
                bool receivingInput = false;
                var pitchInput = sceneManager.pitch;
                rigidBody.AddForce(transform.forward * pitchInput);
                if (System.Math.Abs(pitchInput) > 0)
                {
                    receivingInput = true;
                }
                var elvInput = sceneManager.elv;
                rigidBody.AddForce(transform.up * elvInput);
                if (System.Math.Abs(elvInput) > 0)
                {
                    receivingInput = true;
                }
                var rollInput = sceneManager.roll;
                rigidBody.AddForce(transform.right * rollInput);
                if (System.Math.Abs(rollInput) > 0)
                {

                    receivingInput = true;
                }

                var yawInput = sceneManager.yaw;
                rigidBody.AddTorque(transform.up * yawInput);
                if (System.Math.Abs(yawInput) > 0)
                {

                    receivingInput = true;
                }

                if (receivingInput & rigidBody.drag != inputDrag)
                {
                    rigidBody.drag = inputDrag;
                    rigidBody.angularDrag = inputDrag ;
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
            transform.position = sceneManager.telloManager.transform.position;
            rigidBody.velocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;

        }
    }
}