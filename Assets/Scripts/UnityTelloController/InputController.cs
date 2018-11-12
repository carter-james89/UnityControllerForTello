using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

namespace UnityControllerForTello
{
    public class InputController : MonoBehaviour
    {
        public enum InputType { Thrustmaster16000, Keyboard, Rift, ThrustmasterThrottle }
        public InputType inputType = InputType.Keyboard;

        enum FlipDir { Forward, Left, Backward, Right, ForwardRight, ForwardLeft, BackRight, BackLeft, None }

        public float inputPitch, inputYaw, inputRoll, inputElv, flipDir, flipDirX, speed;

        Transform flipArrow;
        SceneManager sceneManager;

        public PIDProfile PIDprofile;

        Transform newObject, targetDrone;

        public float yawError;
        public Vector3 offsetFromTarget;

        public bool headLess = false;

        public void CustomAwake(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }
        public void CustomStart()
        {
            targetDrone = sceneManager.telloManager.transform;
            if (sceneManager.sceneType == SceneManager.SceneType.SimOnly)
            {
                targetDrone = sceneManager.simulator.transform;
            }

            //newObject = new GameObject().transform;
            //newObject.position = Vector3.zero;
        }

        public bool autoPilotActive { get; private set; } = false;
        public Transform autoPilotTarget;
        PidController proximityPIDX, proximityPIDY, proximityPIDZ, yawPID;

        public void ToggleAutoPilot(bool active)
        {
            if (active)
            {
                if (PIDprofile)
                {
                    //if (!sceneManager.telloManager.tracking)
                    //    sceneManager.telloManager.BeginTracking();
                    autoPilotActive = true;
                    if (autoPilotActive)
                    {
                        if (proximityPIDX == null)
                        {

                            UpdatePIDValues(PIDprofile);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("AutoPilot Disabled");
                autoPilotActive = false;
            }
        }
        public int deltaTime1;
        float timeSinceLastUpdate;
        float prevDeltaTime = 0;

        void UpdatePIDValues(PIDProfile newPIDprofile)
        {
            Debug.Log("set pid values to " + newPIDprofile.name);
            proximityPIDX = new PidController(newPIDprofile.PIDxP, newPIDprofile.PIDxI, newPIDprofile.PIDxD, 1, -1);
            proximityPIDY = new PidController(newPIDprofile.PIDyP, newPIDprofile.PIDyI, newPIDprofile.PIDyD, 1, -1);
            proximityPIDZ = new PidController(newPIDprofile.PIDzP, newPIDprofile.PIDzI, newPIDprofile.PIDzD, 1, -1);
            yawPID = new PidController(newPIDprofile.yawP, newPIDprofile.yawI, newPIDprofile.yawD, 1, -1);
            proximityPIDX.SetPoint = 0;
            proximityPIDY.SetPoint = 0;
            proximityPIDZ.SetPoint = 0;
            yawPID.SetPoint = 0;
        }

        bool highPIDMode = true;
        void RunAutoPilot(float yaw)
        {
            //  Debug.Log("Run autopilot with time " + prevDeltaTime);
            // System.TimeSpan deltaTime = new System.TimeSpan(0,0,0,0,(int)(Time.deltaTime * 1000)); //0, 0, 0, (int)Time.deltaTime);

            System.TimeSpan deltaTime = new System.TimeSpan(0, 0, 0, 0, (deltaTime1));
            var targetOffset = targetDrone.position - autoPilotTarget.position;
            offsetFromTarget = targetOffset;

            if (PIDprofile.name == "Fly To Target (High)")
            {
                if (offsetFromTarget.magnitude < .3f & highPIDMode)
                {

                    if (checkRoutine == null)
                    {
                        Debug.Log("In range check");
                        checkRoutine = StartCoroutine("CheckInRange");
                    }


                }
                else if (offsetFromTarget.magnitude > .3f & !highPIDMode)
                {
                    //if (checkRoutine != null)
                    //{
                    //    Debug.Log("stop routine");
                    //    StopCoroutine(checkRoutine);
                    //    checkRoutine = null;
                    //}

                    highPIDMode = true;
                    UpdatePIDValues(PIDprofile);
                }
            }

            //Debug.Log((int)Time.deltaTime);

            proximityPIDX.ProcessVariable = targetOffset.x;
            double trgtRoll = proximityPIDX.ControlVariable(deltaTime);
            //if(double.IsNaN(trgtRoll))
            //{
            //    trgtRoll = 0;
            //}

            proximityPIDY.ProcessVariable = targetOffset.y;
            double trgtElv = proximityPIDY.ControlVariable(deltaTime);
            //if(double.IsNaN(trgtElv))
            //{
            //    trgtElv = 0;
            //}

            proximityPIDZ.ProcessVariable = targetOffset.z;
            double trgtPitch = proximityPIDZ.ControlVariable(deltaTime);
            //if(double.IsNaN(trgtPitch))
            //{
            //    trgtPitch = 0;
            //}

            yawError = targetDrone.eulerAngles.y - autoPilotTarget.eulerAngles.y;

            if (yawError < -180)
                yawError = 360 - System.Math.Abs(yawError);
            else if (yawError > 180)
                yawError = -(360 - yawError);

            //yawError = Quaternion.eulerAngles(yawErrorRot).y;
            // yawError = Vector3.Angle(targetDrone.forward, autoPilotTarget.forward);
            yawPID.ProcessVariable = yawError;
            double trgtYaw = yawPID.ControlVariable(deltaTime);

            //  trgtYaw = yaw;

            SetControllerState((float)trgtYaw, (float)trgtElv, (float)trgtRoll, (float)trgtPitch);
        }
        Coroutine checkRoutine;
        void SetControllerState(float yaw, float elv, float roll, float pitch)
        {
            //if(float.IsNaN(elv))
            //    elv = 0;
            //if(float.IsNaN(yaw))
            //    yaw = 0;
            //if(float.IsNaN(pitch))
            //    pitch = 0;
            //if(float.IsNaN(roll))
            //    roll = 0;

            if (headLess)
            {
                var xDir = new Vector3(roll, 0, 0);
                var yDir = new Vector3(0, 0, pitch);

                var headLessDir = transform.position + (xDir + yDir);

                var headLessDirX = Vector3.Project(headLessDir, targetDrone.right.normalized);
                roll = headLessDirX.magnitude;
                var headLessDirz = Vector3.Project(headLessDir, targetDrone.forward.normalized);
                pitch = headLessDirz.magnitude;

                var crossProduct = Vector3.Dot(headLessDirz, targetDrone.forward.normalized);

                if (crossProduct < 0)
                {
                    // roll = -roll;
                    pitch = -pitch;
                }
                crossProduct = Vector3.Dot(headLessDirX, targetDrone.right.normalized);

                if (crossProduct < 0)
                {
                    roll = -roll;
                    // pitch = -pitch;
                }
            }

            //if (speed < 0)
            //    speed = .1f;
            inputElv = elv * speed;
            inputRoll = roll * speed;
            inputPitch = pitch * speed;
            inputYaw = yaw * speed;
        }

        IEnumerator CheckInRange()
        {
            Debug.Log("checking in range");
            yield return new WaitForSeconds(5);
            if (offsetFromTarget.magnitude < .3f & highPIDMode)
            {
                highPIDMode = false;
                UpdatePIDValues(GameObject.Find("Fly To Target (Low)").GetComponent<PIDProfile>());
            }
            else
            {
                Debug.Log("not in range");
            }
            checkRoutine = null;
        }
        public void CheckInputs()
        {
            timeSinceLastUpdate = Time.time - prevDeltaTime;
            prevDeltaTime = Time.time;
            deltaTime1 = (int)(timeSinceLastUpdate * 1000);

            //  Debug.Log(timeSinceLastUpdate * 1000);
            // Debug.Log("check inputs");           
            float lx = 0f;
            float ly = 0f;
            float rx = 0f;
            float ry = 0f;

            switch (inputType)
            {
                case InputType.Keyboard:
                    lx = Input.GetAxis("Keyboard Yaw");
                    ly = Input.GetAxis("Keyboard Elv");
                    rx = Input.GetAxis("Keyboard Roll");
                    ry = Input.GetAxis("Keyboard Pitch");
                    break;
                case InputType.ThrustmasterThrottle:
                    ly = Input.GetAxis("Thrustmaster Throttle Elv");
                    rx = Input.GetAxis("Thrustmaster Throttle Roll");
                    ry = -Input.GetAxis("Thrustmaster Throttle Pitch");
                    lx = Input.GetAxis("Thrustmaster Throttle Yaw");
                    flipDir = Input.GetAxis("Thrustmaster Throttle Flip");
                    flipDirX = Input.GetAxis("Thrustmaster Throttle Flip X");
                    speed = -Input.GetAxis("Thrustmaster Throttle Speed");
                    break;
                case InputType.Thrustmaster16000:
                    ly = Input.GetAxis("Up");
                    rx = Input.GetAxis("Roll");
                    ry = -Input.GetAxis("Pitch");
                    lx = Input.GetAxis("Yaw");
                    break;
                case InputType.Rift:
                    lx = Input.GetAxis("Oculus Yaw");
                    rx = Input.GetAxis("Oculus Roll");
                    ry = -Input.GetAxis("Oculus Pitch");
                    ly = -Input.GetAxis("Oculus Up");
                    break;
            }

            if (speed == 0)
            {
                speed = .5f;
            }
            else if (speed < 0)
            {
                speed = 1 + speed;
                speed /= 2;
            }
            else
            {
                speed /= 2;
                speed += .5f;
            }

            if (inputType != InputType.ThrustmasterThrottle)
                speed = 1;

            if (autoPilotTarget & autoPilotActive)
            {
                if (ly != 0 || rx != 0 || ry != 0)
                {
                    autoPilotActive = false;
                }
                else
                {
                    RunAutoPilot(lx);

                    var distFromTarget = Vector3.Distance(targetDrone.position, autoPilotTarget.position);
                    atTarget = false;
                    //Debug.Log(distFromTarget +" from target");
                    if (distFromTarget < .2f)
                    {
                        atTarget = true;
                        if (currentFlightPath)
                        {
                            ReachedPathPoint();
                        }
                    }
                }
            }
            else
            {
                SetControllerState(lx, ly, rx, ry);
            }

            //if (inputType == InputType.ThrustmasterThrottle)
            //    CheckForFlip(flipDir, flipDirX);           
        }
        public bool atTarget { get; private set; }
        FlightPath currentFlightPath;
        public void BeginFlightPath(FlightPath pathToFly)
        {
            Debug.Log("Begin Flight Path");
            currentFlightPath = pathToFly;
            autoPilotTarget = currentFlightPath.flightPoints[0];
        }
        void ReachedPathPoint()
        {
            Debug.Log("Find next flight point");
            int nextTarget = 0;
            for (int i = 0; i < currentFlightPath.flightPoints.Count; i++)
            {
                if (autoPilotTarget == currentFlightPath.flightPoints[i])
                {
                    if (i != currentFlightPath.flightPoints.Count - 1)
                        nextTarget = i + 1;

                    autoPilotTarget = currentFlightPath.flightPoints[nextTarget];
                    break;
                }
            }
        }
    }
}
