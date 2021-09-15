using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotInputs : MonoBehaviour
{
    public enum InputType { Thrustmaster16000, Keyboard, Rift, ThrustmasterThrottle }
    public InputType inputType = InputType.Keyboard;

    private int _lastInputFrame = 0;
    private PilotInputValues _currentInputValues;
    public PilotInputValues pilotInputValues
    {
        get
        {
            if (Time.frameCount != _lastInputFrame) //only check inputs once a frame, as they dont change within a single frame
            {
                _currentInputValues = CheckFlightInputs();
                _lastInputFrame = Time.frameCount;
            }
            return _currentInputValues;
        }
    }

    private float _rawYaw;
    private float _rawPitch;
    private float _rawRoll;
    private float _rawThrottle;

    public Action takeOff;
    public Action land;
    public Action primeProps;

    public Action toggleAutoPilot;

    public bool UserInputingValues()
    {
        if(
            pilotInputValues.yaw != 0 ||
             pilotInputValues.pitch != 0 ||
            pilotInputValues.roll != 0 ||
             Input.GetKey(KeyCode.L))
        {
            return true;
        }
        return false;
    }

    public void GetFlightCommmands()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            toggleAutoPilot?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("takeoff at pilotinput");
            takeOff?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.V))
        {
            primeProps?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            land?.Invoke();
        }
    }

    public struct PilotInputValues
    {
        public float pitch;
        public float yaw;
        public float roll;
        public float throttle;
    }

    /// <summary>
    /// Check the pilot input based on <see cref="InputType"/>
    /// </summary>
    /// <remarks>
    /// Override to add new input methods
    /// </remarks>
    /// <returns></returns>
    protected virtual PilotInputValues CheckFlightInputs()
    {

        PilotInputValues inputValue = new PilotInputValues();

        switch (inputType)
        {
            case InputType.Keyboard:
                inputValue.yaw = Input.GetAxis("Keyboard Yaw");
                inputValue.throttle = Input.GetAxis("Keyboard Elv");
                inputValue.roll = Input.GetAxis("Keyboard Roll");
                inputValue.pitch = Input.GetAxis("Keyboard Pitch");
                break;
            case InputType.ThrustmasterThrottle:
                inputValue.throttle = Input.GetAxis("Thrustmaster Throttle Elv");
                inputValue.roll = Input.GetAxis("Thrustmaster Throttle Roll");
                inputValue.pitch = -Input.GetAxis("Thrustmaster Throttle Pitch");
                inputValue.yaw = Input.GetAxis("Thrustmaster Throttle Yaw");
                // flipDir = Input.GetAxis("Thrustmaster Throttle Flip");
                // flipDiinputValue.roll = Input.GetAxis("Thrustmaster Throttle Flip X");
                // inputValue.throttle = -Input.GetAxis("Thrustmaster Throttle inputValue.throttle");
                if (inputValue.throttle == 0)
                {
                    inputValue.throttle = .5f;
                }
                else if (inputValue.throttle < 0)
                {
                    inputValue.throttle = 1 + inputValue.throttle;
                    inputValue.throttle /= 2;
                }
                else
                {
                    inputValue.throttle /= 2;
                    inputValue.throttle += .5f;
                }
                break;
            case InputType.Thrustmaster16000:
                inputValue.throttle = Input.GetAxis("Up");
                inputValue.roll = Input.GetAxis("Roll");
                inputValue.pitch = -Input.GetAxis("Pitch");
                inputValue.yaw = Input.GetAxis("Yaw");
                break;
            case InputType.Rift:
                inputValue.yaw = Input.GetAxis("Oculus Yaw");
                inputValue.roll = Input.GetAxis("Oculus Roll");
                inputValue.pitch = -Input.GetAxis("Oculus Pitch");
                inputValue.throttle = -Input.GetAxis("Oculus Up");
                break;
        }

        return inputValue;
    }
}
