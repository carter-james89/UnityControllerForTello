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

        /// <summary>
        /// Get the inputs based on <see cref="InputType"/>
        /// </summary>
        /// <returns></returns>
        public Quaternion CheckFlightInputs()
        {                 
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

            ///simply to show debug values in inspector
            rawYaw = lx;
            rawElv = ly;
            rawRoll = rx;
            rawPitch = ry;

            return new Quaternion(rawYaw, rawElv, rawRoll, rawPitch);        
        }
    }
}
