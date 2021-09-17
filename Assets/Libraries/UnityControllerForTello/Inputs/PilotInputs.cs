using System;
using UnityEngine;

namespace UnityControllerForTello
{
    public class PilotInputs : MonoBehaviour, IInputs
    {
        public enum InputType { Thrustmaster16000, Keyboard, Rift, ThrustmasterThrottle }
        public InputType inputType = InputType.Keyboard;

        private float _rawYaw;
        private float _rawPitch;
        private float _rawRoll;
        private float _rawThrottle;

        public Action toggleAutoPilot;

        private int _lastInputFrame = 0;
        private IInputs.FlightControlValues frameValues;

        public IInputs.FlightControlValues GetInputValues()
        {
            return CheckFlightInputs();
        }

        /// <summary>
        /// Check the pilot input based on <see cref="InputType"/>
        /// </summary>
        /// <remarks>
        /// Override to add new input methods
        /// </remarks>
        /// <returns></returns>
        protected virtual IInputs.FlightControlValues CheckFlightInputs()
        {
           if(_lastInputFrame != Time.frameCount)
            {
                _lastInputFrame = Time.frameCount;

                frameValues.takeOff = Input.GetKeyDown(KeyCode.T);
                frameValues.land = Input.GetKeyDown(KeyCode.L);
                frameValues.toggleAutoPilot = Input.GetKeyDown(KeyCode.P);

                 if (frameValues.toggleAutoPilot)
                {
                    toggleAutoPilot?.Invoke();
                }

                switch (inputType)
                {
                    case InputType.Keyboard:
                        frameValues.yaw = Input.GetAxis("Keyboard Yaw");
                        frameValues.throttle = Input.GetAxis("Keyboard Elv");
                        frameValues.roll = Input.GetAxis("Keyboard Roll");
                        frameValues.pitch = Input.GetAxis("Keyboard Pitch");
                        break;
                    case InputType.ThrustmasterThrottle:
                        frameValues.throttle = Input.GetAxis("Thrustmaster Throttle Elv");
                        frameValues.roll = Input.GetAxis("Thrustmaster Throttle Roll");
                        frameValues.pitch = -Input.GetAxis("Thrustmaster Throttle Pitch");
                        frameValues.yaw = Input.GetAxis("Thrustmaster Throttle Yaw");
                        // flipDir = Input.GetAxis("Thrustmaster Throttle Flip");
                        // flipDiframeValues.roll = Input.GetAxis("Thrustmaster Throttle Flip X");
                        // frameValues.throttle = -Input.GetAxis("Thrustmaster Throttle frameValues.throttle");
                        if (frameValues.throttle == 0)
                        {
                            frameValues.throttle = .5f;
                        }
                        else if (frameValues.throttle < 0)
                        {
                            frameValues.throttle = 1 + frameValues.throttle;
                            frameValues.throttle /= 2;
                        }
                        else
                        {
                            frameValues.throttle /= 2;
                            frameValues.throttle += .5f;
                        }
                        break;
                    case InputType.Thrustmaster16000:
                        frameValues.throttle = Input.GetAxis("Up");
                        frameValues.roll = Input.GetAxis("Roll");
                        frameValues.pitch = -Input.GetAxis("Pitch");
                        frameValues.yaw = Input.GetAxis("Yaw");
                        break;
                    case InputType.Rift:
                        frameValues.yaw = Input.GetAxis("Oculus Yaw");
                        frameValues.roll = Input.GetAxis("Oculus Roll");
                        frameValues.pitch = -Input.GetAxis("Oculus Pitch");
                        frameValues.throttle = -Input.GetAxis("Oculus Up");
                        break;
                }
            }
            return frameValues;
        }
    }
}