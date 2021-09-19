using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Base class for standard quadcopters, both real and simulated
    /// Satisfies the requirements of <see cref="IQuadcopter"/>, and handles most of the heavy lifting for basic functions
    /// </summary>
    public abstract class Quadcopter : MonoBehaviour, IQuadcopter
    {
        /// <summary>
        /// The current inputs for this Frame from either <see cref="defaultInputSource"/> or <see cref="overrideInputSource"/>, set in <see cref="ProcessInputs"/>
        /// </summary>
        protected IInputs.FlightControlValues currentInputs;

        [SerializeField]
        protected GameObject groundSensorPoint;

        /// <summary>
        /// Used to visalize the trail the quadcopter has traveled
        /// </summary>
        /// <remarks>
        /// No way to turn this on/off via code right now, just enable/disable the component in the inspector
        /// </remarks>
        [SerializeField]
        protected TrailVisualizer _trailVisualizer;

        /// <summary>
        /// The current status of the quadcopter
        /// </summary>
        [SerializeField]
        protected IQuadcopter.FlightStatus _flightStatus;

        /// <summary>
        /// Should the Quad run in headless mode?
        /// Yaw of the craft is ignored, in the case of <see cref="TelloQuadcopter"/>, forward is the direction the Tello was facing when powered on
        /// </summary>
        [SerializeField]
        private bool _headLessMode = false;

        /// <summary>
        /// Launch the quadcopter
        /// </summary>
        public abstract void TakeOff();
        /// <summary>
        /// Land the quadcopter
        /// </summary>
        public abstract void Land();
        /// <summary>
        /// Is this quadcopter a simulator or a real quadcopter
        /// </summary>
        /// <returns></returns>
        public abstract bool IsSimulator();

        /// <summary>
        /// Event that is called whenever the GameObjects <see cref="Transform"/> is changed
        /// </summary>
        /// <remarks>
        /// This needs to be called manually in <see cref="onTransformChanged"/> for all inherited classes
        /// </remarks>
        public Action<Vector3, Quaternion> onTransformChanged;

        /// <summary>
        /// The point the Quad took off from, can be updated on the fly
        /// </summary>
        public Vector3 homePoint { get; private set; }


        /// <summary>
        /// The updates supplied by either the Pilot or the AutoPilot for this frame
        /// </summary>
        protected Func<IInputs.FlightControlValues> defaultInputSource;
        /// <summary>
        /// The updates supplied by either the Pilot or the AutoPilot for this frame
        /// </summary>
        protected Func<IInputs.FlightControlValues> overrideInputSource;

        /// <summary>
        /// Initialize the autopilot, and provide the depenencies it needs. 
        /// </summary>
        /// <param name="pilotInputs">Where to find the inputs from the pilot</param>
        /// <param name="autoPilot">The autopilot module used, activated via <see cref="ActivateAutoPilot"/></param>
        public virtual void Initialize(Func<IInputs.FlightControlValues> defaultInputSource)
        {
            _flightStatus = IQuadcopter.FlightStatus.PreLaunch;

            this.defaultInputSource = defaultInputSource;

            if (_trailVisualizer)
            {
                _trailVisualizer.Initialize(this);
            }
        }

        public float deltaHeight;
        private float _prevHeight = 0;
        [SerializeField]
        private float heightOffset = 0;
        [SerializeField]
        private float elvInput;

        [SerializeField]
        private float assumedHeightOffset = 0;

        public float vertSpeed;

        private List<GameObject> sensorPoints;

        public void SetVirtualPosition(Vector2 xzPos, float height, Quaternion calculatedRotation, float vertSpeed)
        {
            float floatHeight = (float)Math.Round(height, 1);
            //Debug.Log("Recording Static Height Delta Change " + floatHeight);
            //RaycastHit hit;
            //// Does the ray intersect any objects excluding the player layer
            //if (Physics.Raycast(rigidBody.transform.position, rigidBody.transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
            //{
            //    Debug.DrawRay(rigidBody.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            //}
            //Debug.Log(height);
            this.vertSpeed = vertSpeed;

            deltaHeight = _prevHeight - floatHeight;
            _prevHeight = floatHeight;
            elvInput = currentInputs.throttle;

            var tempPos = new Vector3(xzPos.x, floatHeight + heightOffset, xzPos.y);// rigidBody.transform.position;

            var newGroundSensorPoint = CreateSensorPoint();

            if (_flightStatus == IQuadcopter.FlightStatus.Flying)
                if (Math.Abs(deltaHeight) > .1 && Math.Abs(elvInput) <= .05f)
                {
                    Debug.Log("Known height updated " + deltaHeight);
                    // if (Math.Abs(currentInputs.throttle) < .05)
                    // {
                    heightOffset += deltaHeight;
                    // }
                    tempPos.y = floatHeight + heightOffset + assumedHeightOffset;
                }
                else if (Math.Abs(elvInput) <= .05f)//Math.Abs(vertSpeed) <= .1 && Math.Abs(elvInput) <= .05f)// && Math.Abs(deltaHeight) > 0)
                {
                    assumedHeightOffset += deltaHeight;
                    tempPos.y = floatHeight + heightOffset + assumedHeightOffset;
                    Debug.Log("Recording Static Height Delta Change " + deltaHeight);
                }

            //else if (Math.Abs(vertSpeed) > .1 && Math.Abs(elvInput) > .1f)
            //{
            //    ResetOffset();
            //}
            //if (Math.Abs(currentInputs.throttle) < .05)
            //{
            //    heightOffset += deltaHeight;
            //    tempPos.y = height + heightOffset;
            //}

            //var groundpoint = transform.position - (Vector3.down * tempPos.y);


            transform.position = tempPos;

            transform.rotation = calculatedRotation;


            newGroundSensorPoint.transform.position = transform.position + (Vector3.down * height);

            if (assumedHeightOffset > .1)
                newGroundSensorPoint.GetComponent<MeshRenderer>().material.color = Color.red;

            else if (heightOffset > .1)
                newGroundSensorPoint.GetComponent<MeshRenderer>().material.color = Color.green;

            if (Math.Abs(vertSpeed) > .1 || Math.Abs(elvInput) > .05f)
            {
                newGroundSensorPoint.GetComponent<MeshRenderer>().material.color = Color.black;
            }
        }

        private GameObject CreateSensorPoint()
        {
            if (sensorPoints == null)
            {
                sensorPoints = new List<GameObject>();
            }
            var newGroundSensorPoint = Instantiate(groundSensorPoint);
            sensorPoints.Add(newGroundSensorPoint);
            return newGroundSensorPoint;
        }
        public void DestroySensorPoints()
        {
            if (sensorPoints != null)
            {
                foreach (var item in sensorPoints)
                {
                    item.SetActive(false);
                }
            }
        }


        //public void SetVirtualPosition(Vector2 xzPos, float height, Quaternion calculatedRotation)
        //{
        //    deltaHeight = _prevHeight - height;
        //    _prevHeight = height;
        //    _elvInput = currentInputs.throttle;

        //  //  var tempPos = transform.position;
        ////    float calculatedHeight = height;
        //    if (Math.Abs(deltaHeight) > .1)
        //    {
        //        if (Math.Abs(currentInputs.throttle) < .05)
        //        {
        //            heightOffset += deltaHeight;
        //        }
        //       //height + heightOffset;
        //    }

        //    transform.position = new Vector3(xzPos.x, height + heightOffset, xzPos.y);
        //    transform.rotation = calculatedRotation;

        //    var newGroundSensorPoint = Instantiate(groundSensorPoint);
        //    newGroundSensorPoint.transform.position = transform.position + (Vector3.down * height);
        //    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * height, Color.yellow);
        //    OnTransformUpdated();
        //}
        public void ResetOffset()
        {
            // heightOffset = 0;
            assumedHeightOffset = 0;
        }
        public void ResetKnownOffset()
        {
            heightOffset = 0;
        }

        /// <summary>
        /// Proccess the inputs from either <see cref="defaultInputSource"/> or <see cref="overrideInputSource"/>
        /// Also executes commands
        /// </summary>
        protected void ProcessInputs()
        {
            var defaultInputs = defaultInputSource.Invoke();
            if (
              defaultInputs.yaw != 0 ||
               defaultInputs.pitch != 0 ||
              defaultInputs.roll != 0 ||
               defaultInputs.throttle != 0 ||
               defaultInputs.takeOff ||
               defaultInputs.land)
            {
                if (overrideInputSource != null)
                {
                    Debug.LogWarning("Inputs detected from Default Input Source, borting override");
                    abort?.Invoke();
                    if (overrideInputSource != null)
                    {
                        overrideInputSource = null;
                        Debug.LogWarning("RemoveInputOverride was not removed via Abort, this should have been done by the Action supplied to OverrideInputSource");
                    }
                }
            }

            currentInputs = overrideInputSource == null ? _headLessMode ? ConvertToHeadlessInputs(defaultInputs) : defaultInputs : overrideInputSource.Invoke();
            if (currentInputs.takeOff && _flightStatus == IQuadcopter.FlightStatus.PreLaunch)
            {
                TakeOff();
            }
            else if (currentInputs.land)
            {
                Land();
            }
        }
        /// <summary>
        /// Calculate the Roll and Pitch values to acheive the desired headless direction
        /// </summary>
        /// <remarks>
        /// Transform.Right/Forward has no positional data, which is why Vector3.Project works between headlessDir and Transform.Right/Forward
        /// </remarks>
        public virtual IInputs.FlightControlValues ConvertToHeadlessInputs(IInputs.FlightControlValues rawInputs)
        {
            var headLessDir = new Vector3(rawInputs.roll, 0, rawInputs.pitch);

            LineRenderer line = GetComponent<LineRenderer>();
            line.SetPosition(0, transform.position);
            line.SetPosition(1, transform.position + headLessDir);

            var headLessDirX = Vector3.Project(headLessDir, transform.right);
            rawInputs.roll = headLessDirX.magnitude;
            var headLessDirz = Vector3.Project(headLessDir, transform.forward);
            rawInputs.pitch = headLessDirz.magnitude;

            //Sign the values
            var dotProduct = Vector3.Dot(headLessDirz, transform.forward);
            if (dotProduct < 0)
            {
                rawInputs.pitch = -rawInputs.pitch;
            }
            dotProduct = Vector3.Dot(headLessDirX, transform.right);
            if (dotProduct < 0)
            {
                rawInputs.roll = -rawInputs.roll;
            }
            return rawInputs;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// Call whenever you manually manipulate this GameObject's transform
        /// Raises event to let other systems know the quad has moved
        /// </summary>
        protected void OnTransformUpdated()
        {
            onTransformChanged?.Invoke(transform.position, transform.rotation);
        }

        public IQuadcopter.FlightStatus GetFlightStatus()
        {
            return _flightStatus;
        }

        /// <summary>
        /// Set the home point for the quad, usually at liftoff or when hover first achieved
        /// </summary>
        /// <param name="newHomePoint">The new homepoint</param>
        public void SetHomePoint(Vector3 newHomePoint)
        {
            homePoint = newHomePoint;
        }

        /// <summary>
        /// Override <see cref="defaultInputSource"/>, this quad will now query the provided input source for its <see cref="IInputs.FlightControlValues"/>
        /// </summary>
        /// <param name="inputValueSource">The source of the inputs to use</param>
        /// <param name="abortListener">What should be raised if the quad aborts and returns to <see cref="defaultInputSource"/></param>
        public void OverrideInputSource(Func<IInputs.FlightControlValues> inputValueSource, Action abortListener)
        {
            overrideInputSource = inputValueSource;
            SubscibeToAbort(abortListener);
        }
        /// <summary>
        /// Removed the provided <see cref="IInputs.FlightControlValues"/> source, and return the quad to <see cref="defaultInputSource"/>
        /// </summary>
        /// <param name="inputValueSource">The Input Source to remove, must match the current <see cref="overrideInputSource"/></param>
        /// <param name="abortListener">The action that was provided as an Abort listener, must match the one provided in <see cref="overrideInputSource"/></param>
        public void RemoveInputOverride(Func<IInputs.FlightControlValues> inputValueSource, Action abortListener)
        {
            if (inputValueSource == overrideInputSource)
            {
                overrideInputSource = null;
                UnsubscribeFromAbort(abortListener);
            }
        }
        /// <summary>
        /// Is the quad currently in a valid state of tracking?
        /// </summary>
        /// <returns></returns>
        public abstract bool IsTracking();

        /// <summary>
        /// Action to raise if the quad needs to Abort and return to <see cref="defaultInputSource"/>
        /// </summary>
        protected Action abort;
        /// <summary>
        /// Subscribe to <see cref="abort"/>, this is raised whenever <see cref="overrideInputSource"/> is overridden by <see cref="defaultInputSource"/>
        /// or if the quad looses tracking
        /// </summary>
        /// <param name="actionToSubscribe">The function to be called when <see cref="abort"/> is raised</param>
        public void SubscibeToAbort(Action actionToSubscribe)
        {
            abort += actionToSubscribe;
        }
        /// <summary>
        /// Unsubscribe from <see cref="abort"/>
        /// The function to be unsubscribed must have been previously subscribed
        /// </summary>
        /// <param name="actionToUnsubscribe">The function to unsubscribe</param>
        public void UnsubscribeFromAbort(Action actionToUnsubscribe)
        {
            if (abort.GetInvocationList().ToList().Contains(actionToUnsubscribe))
            {
                abort -= actionToUnsubscribe;
            }
            else
            {
                Debug.LogWarning("Trying to Unsubscribe an action from Abort that was never Subscribed, this should not happen");
            }
        }
    }
}
