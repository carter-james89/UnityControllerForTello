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

        public float rawYaw, rawElv, rawRoll, rawPitch;
        float flipDir, flipDirX;
        public float speed;

        Transform flipArrow;
        SceneManager sceneManager;

        public bool headLessMode = false;

        public void CustomAwake(SceneManager sceneManager)
        {
            this.sceneManager = sceneManager;
        }
        public void CustomStart()
        {

        }

        public void GetFlightCommmands()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                sceneManager.ToggleAutoPilot(true);
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                sceneManager.TakeOff();
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                sceneManager.PrimeProps();
            }
            else if (Input.GetKeyDown(KeyCode.L))
            {
                sceneManager.Land();
            }
        }
        public Quaternion CheckFlightInputs()
        {          
            //timeSinceLastUpdate = Time.time - prevDeltaTime;
            //prevDeltaTime = Time.time;
            //deltaTime1 = (int)(timeSinceLastUpdate * 1000);

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

            rawYaw = lx;
            rawElv = ly;
            rawRoll = rx;
            rawPitch = ry;
            // return new Quaternion(lx, ly, rx, ry);
            return new Quaternion(rawYaw, rawElv, rawRoll, rawPitch);

            //if (autoPilotTarget & autoPilotActive)
            //{
            //    //if any update is received from pilot, deactivate autopilot
            //    if (ly != 0 || rx != 0 || ry != 0)
            //    {
            //        autoPilotActive = false;
            //    }
            //    else
            //    {
            //        RunAutoPilot(lx);
            //        var distFromTarget = Vector3.Distance(targetDrone.position, autoPilotTarget.position);
            //        atTarget = false;
            //        //Debug.Log(distFromTarget +" from target");
            //        if (distFromTarget < .2f)
            //        {
            //            atTarget = true;
            //            if (currentFlightPath)
            //            {
            //                ReachedPathPoint();
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    SetControllerState(lx, ly, rx, ry);
            //}

            //if (inputType == InputType.ThrustmasterThrottle)
            //    CheckForFlip(flipDir, flipDirX);           
        }
        // public bool atTarget { get; private set; }
        //   FlightPath currentFlightPath;
        //public void BeginFlightPath(FlightPath pathToFly)
        //{
        //    Debug.Log("Begin Flight Path");
        //    currentFlightPath = pathToFly;
        //    autoPilotTarget = currentFlightPath.flightPoints[0];
        //}
        //void ReachedPathPoint()
        //{
        //    Debug.Log("Find next flight point");
        //    int nextTarget = 0;
        //    for (int i = 0; i < currentFlightPath.flightPoints.Count; i++)
        //    {
        //        if (autoPilotTarget == currentFlightPath.flightPoints[i])
        //        {
        //            if (i != currentFlightPath.flightPoints.Count - 1)
        //                nextTarget = i + 1;

        //            autoPilotTarget = currentFlightPath.flightPoints[nextTarget];
        //            break;
        //        }
        //    }
        //}
    }
}
