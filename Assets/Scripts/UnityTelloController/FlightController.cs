using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;
using UnityEngine.Events;

namespace UnityControllerForTello
{
    public class FlightController : MonoBehaviour
    {
        //public enum InputType { Thrustmaster16000, Keyboard, Rift, ThrustmasterThrottle }
        //public InputType inputType = InputType.Keyboard;

        //enum FlipDir { Forward, Left, Backward, Right, ForwardRight, ForwardLeft, BackRight, BackLeft, None }

        //public float inputPitch, inputYaw, inputRoll, inputElv, flipDir, flipDirX, speed;

        //Transform flipArrow;
        //TelloManager manager;
        //TelloManager.EventDelegate takeOffDel;

        //Transform newObject, targetDrone;


        //public void CustomAwake(TelloManager manager)
        //{
        //    //flipArrow = GameObject.Find("Flip Arrow").transform;
        //    // flipArrow.gameObject.SetActive(false);
        //    this.manager = manager;

        //     newObject = new GameObject().transform;
        //    newObject.SetParent(transform);
        //    newObject.localPosition = Vector3.zero;
        //    newObject.localScale = Vector3.one;
        //    DebugVector.DrawVector(FindObjectOfType<DroneSimulator>().transform, Color.red, Color.green, Color.blue, Vector3.one);
        //}

        //public void CustomStart()
        //{
        //    targetDrone = transform;
        //    if(manager.sceneManager.sceneType == SceneManager.SceneType.SimOnly)
        //    {
        //        targetDrone = manager.sceneManager.simulator.transform;
        //    }
        //}

        //public bool headLess = false;

        //void CheckInputs()
        //{
        //    // Debug.Log("check inputs");           
        //    float lx = 0f;
        //    float ly = 0f;
        //    float rx = 0f;
        //    float ry = 0f;

        //    switch (inputType)
        //    {
        //        case InputType.Keyboard:
        //            lx = Input.GetAxis("Keyboard Yaw");
        //            ly = Input.GetAxis("Keyboard Elv");
        //            rx = Input.GetAxis("Keyboard Roll");
        //            ry = Input.GetAxis("Keyboard Pitch");
        //            break;
        //        case InputType.ThrustmasterThrottle:
        //            ly = Input.GetAxis("Thrustmaster Throttle Elv");
        //            rx = Input.GetAxis("Thrustmaster Throttle Roll");
        //            ry = -Input.GetAxis("Thrustmaster Throttle Pitch");
        //            lx = Input.GetAxis("Thrustmaster Throttle Yaw");
        //            flipDir = Input.GetAxis("Thrustmaster Throttle Flip");
        //            flipDirX = Input.GetAxis("Thrustmaster Throttle Flip X");
        //            speed = -Input.GetAxis("Thrustmaster Throttle Speed");
        //            break;
        //        case InputType.Thrustmaster16000:
        //            ly = Input.GetAxis("Up");
        //            rx = Input.GetAxis("Roll");
        //            ry = -Input.GetAxis("Pitch");
        //            lx = Input.GetAxis("Yaw");
        //            break;
        //        case InputType.Rift:
        //            lx = Input.GetAxis("Oculus Yaw");
        //            rx = Input.GetAxis("Oculus Roll");
        //            ry = -Input.GetAxis("Oculus Pitch");
        //            ly = -Input.GetAxis("Oculus Up");
        //            break;
        //    }

        //    if (speed == 0)
        //    {
        //        speed = .5f;
        //    }
        //    else if (speed < 0)
        //    {
        //        speed = 1 + speed;
        //        speed /= 2;
        //    }
        //    else
        //    {
        //        speed /= 2;
        //        speed += .5f;
        //    }

        //    if (inputType != InputType.ThrustmasterThrottle)
        //        speed = 1;

        //    if (headLess)
        //    {
        //        var xDir = new Vector3(rx, 0,0);
        //        var yDir = new Vector3(0, 0,ry);

        //        var headLessDir = (xDir + yDir) / 2;
        //       // newObject.eulerAngles = headLessDir;

        //        var headLessDirX = Vector3.Project(headLessDir, targetDrone.right.normalized);
        //        rx = headLessDirX.x;
        //        var headLessDirY = Vector3.Project(headLessDir, targetDrone.forward.normalized);
        //        ry = headLessDirY.z;

        //        var crossProduct = Vector3.Dot(Vector3.forward, targetDrone.forward.normalized);

        //        if(crossProduct < 0)
        //        {
        //            rx = -rx;
        //            ry = -ry;
        //        }
        //    }

        //    //if (speed < 0)
        //    //    speed = .1f;
        //    inputElv = ly * speed;
        //    inputRoll = rx * speed;
        //    inputPitch = ry * speed;
        //    inputYaw = lx * speed;

        //    if (inputType == InputType.ThrustmasterThrottle)
        //        CheckForFlip(flipDir, flipDirX);
        //    // if (lx != 0 || ly != 0 || rx != 0 || ry != 0)
        //    Tello.controllerState.setAxis(inputYaw, inputElv, inputRoll, inputPitch);
        //}
        //public void CustomUpdate()
        //{
        //    CheckInputs();
        //}

        //public void ManualStart()
        //{
        //    Tello.controllerState.setAxis(-1, -.66f, 1, -.66f);
        //}

        //void CheckForFlip(float flipDir, float flipDirX)
        //{
        //    var dir = FlipDir.None;
        //    if (flipDir != 0 || flipDirX != 0)
        //    {
        //        // if (!flipArrow.gameObject.activeInHierarchy)
        //        //     flipArrow.gameObject.SetActive(true);

        //        if (flipDir > 0)
        //        {
        //            dir = FlipDir.Forward;
        //            if (flipDirX > 0)
        //                dir = FlipDir.ForwardRight;
        //            else if (flipDirX < 0)
        //                dir = FlipDir.ForwardLeft;
        //        }
        //        else if (flipDir < 0)
        //        {
        //            dir = FlipDir.Backward;
        //            if (flipDirX > 0)
        //                dir = FlipDir.BackRight;
        //            else if (flipDirX < 0)
        //                dir = FlipDir.BackLeft;
        //        }

        //        if (flipDirX > 0)
        //        {
        //            dir = FlipDir.Right;
        //            if (flipDir > 0)
        //                dir = FlipDir.ForwardRight;
        //            else if (flipDir < 0)
        //                dir = FlipDir.BackRight;
        //        }
        //        else if (flipDirX < 0)
        //        {
        //            dir = FlipDir.Left;
        //            if (flipDir > 0)
        //                dir = FlipDir.ForwardLeft;
        //            else if (flipDir < 0)
        //                dir = FlipDir.BackLeft;
        //        }

        //        //Debug.Log(dir.ToString());
        //    }
        //    //else if (flipArrow.gameObject.activeInHierarchy)
        //    //    flipArrow.gameObject.SetActive(false);

        //    //var tempEuler = flipArrow.localEulerAngles;
        //    //switch (dir)
        //    //{
        //    //    case FlipDir.Forward:
        //    //        tempEuler.z = 90;
        //    //        break;
        //    //    case FlipDir.Backward:
        //    //        tempEuler.z = -90;
        //    //        break;
        //    //    case FlipDir.Left:
        //    //        tempEuler.z = 180;
        //    //        break;
        //    //    case FlipDir.Right:
        //    //        tempEuler.z = 0;
        //    //        break;

        //    //    case FlipDir.ForwardRight:
        //    //        tempEuler.z = 45;
        //    //        break;
        //    //    case FlipDir.BackRight:
        //    //        tempEuler.z = -45;
        //    //        break;
        //    //    case FlipDir.ForwardLeft:
        //    //        tempEuler.z = 135;
        //    //        break;
        //    //    case FlipDir.BackLeft:
        //    //        tempEuler.z = 225;
        //    //        break;
        //    //}
        //    //flipArrow.localEulerAngles = tempEuler;

        //    //if (dir != FlipDir.None)
        //    //{
        //    //    if (Input.GetKeyDown(KeyCode.Joystick1Button0))
        //    //    {
        //    //        Debug.Log("Flip: " + dir);
        //    //        Tello.doFlip(1);
        //    //    }
        //    //}
        //}
    }
}
