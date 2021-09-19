using System;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// A simulator Quadcopter meant to replicate the DJI Tello
    /// Can be used to test <see cref="IAutoPilot"/> or any new features without destroying Tello
    /// </summary>
    /// <remarks>
    /// Tried my best to tune the simulator to match real life Tello, but dont expect PID tunings for simulator to work for Tello
    /// </remarks>
    public class SimulationQuadcopter : Quadcopter
    {
        /// <summary>
        /// Rigidbody to control the physics of the simulator
        /// </summary>
        private Rigidbody rigidBody;
        /// <summary>
        /// "Aerodynamic" drag when the user is inputing control values
        /// </summary>
        [SerializeField]
        private float inputDrag;
        /// <summary>
        /// "Aerodynamic" drag when the user is not inputing control values
        /// </summary>
        [SerializeField]
        private float drag;

        public float timeSpeed = 1;



        public override void Initialize(Func<IInputs.FlightControlValues> defaultInputSource)
        {
            base.Initialize(defaultInputSource);

            var physicsCalculator = new GameObject("Simulation Physics Simulation");
            rigidBody = physicsCalculator.AddComponent<Rigidbody>();

            Time.timeScale = timeSpeed;
        }

        //public float deltaHeight;
        //private float _prevHeight = 0;
        //[SerializeField]
        //private float heightOffset = 0;

        //public float elvInput;

        //public void ResetOffset()
        //{
        //    heightOffset = 0;
        //}

        private void Update()
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rigidBody.transform.position, rigidBody.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            {
                Debug.DrawRay(rigidBody.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            }
            SetVirtualPosition(new Vector3(rigidBody.transform.position.x, rigidBody.transform.position.z), hit.distance,rigidBody.transform.rotation, rigidBody.velocity.y);
            //deltaHeight = _prevHeight - hit.distance;
            //_prevHeight = hit.distance;
            //elvInput = currentInputs.throttle;

            //var tempPos = rigidBody.transform.position;

            //if (Math.Abs(deltaHeight) > .1)
            //{
            //    if (Math.Abs(currentInputs.throttle) < .05)
            //    {
            //        heightOffset += deltaHeight;
            //    }
            //    tempPos.y = hit.distance + heightOffset;
            //}

            //var newGroundSensorPoint = Instantiate(groundSensorPoint);
            //newGroundSensorPoint.transform.position = transform.position + (Vector3.down * hit.distance);

            //transform.position = tempPos;
            //transform.rotation = rigidBody.transform.rotation;
            ProcessInputs();
        }

        /// <summary>
        /// All the physics for the simulator
        /// </summary>
        /// <remarks>
        /// Tried my best to tune the simulator to match real life Tello, but dont expect PID tunings for simulator to work for Tello
        /// </remarks>
        public void FixedUpdate()
        {
            if (_flightStatus != IQuadcopter.FlightStatus.PreLaunch)
            {
                rigidBody.AddForce(transform.up * 9.81f);
                bool receivingInput = false;
                var pitchInput = currentInputs.pitch;
                rigidBody.AddForce(rigidBody.transform.forward * pitchInput);
                if (System.Math.Abs(pitchInput) > 0)
                {
                    receivingInput = true;
                }
                var elvInput = currentInputs.throttle;
                rigidBody.AddForce(rigidBody.transform.up * elvInput);
                if (System.Math.Abs(elvInput) > 0)
                {
                    receivingInput = true;
                }
                var rollInput = currentInputs.roll;
                rigidBody.AddForce(rigidBody.transform.right * rollInput);
                if (System.Math.Abs(rollInput) > 0)
                {

                    receivingInput = true;
                }

                var yawInput = currentInputs.yaw;
                rigidBody.AddTorque(rigidBody.transform.up * yawInput);
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

                OnTransformUpdated();
            }
        }

        public override void Land()
        {
            //TODO: write something to land the simulator, not really important
        }

        /// <summary>
        /// Move the simulator into <see cref="IQuadcopter.FlightStatus.Flying"/> mode, and activate physics
        /// </summary>
        public override void TakeOff()
        {
            Debug.Log("Simulator TakeOff");
            rigidBody.transform.position += new Vector3(0, .8f, 0);
            transform.position = rigidBody.transform.position;
            Update();
            ResetKnownOffset();
            rigidBody.useGravity = true;
            SetHomePoint(transform.position);
            _flightStatus = IQuadcopter.FlightStatus.Flying;
        }

        /// <summary>
        /// This is a simulator
        /// </summary>
        /// <returns>True</returns>
        public override bool IsSimulator()
        {
            return true;
        }

        /// <summary>
        /// The simulator will never loose tracking
        /// </summary>
        /// <returns>True</returns>
        public override bool IsTracking()
        {
            return true;
        }
    }
}