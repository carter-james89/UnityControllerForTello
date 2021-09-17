using System;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Base class which provides functionality for most autopilots
    /// </summary>
    public abstract class AutoPilot : MonoBehaviour, IAutoPilot
    {
        /// <summary>
        /// The <see cref="IQuadcopter"/> this autopilot will manipulate
        /// </summary>
        protected IQuadcopter quadToControl { get; private set; }

        /// <summary>
        /// Is the autopilot currently active?
        /// </summary>
        private bool autoPilotActive = false;

        private void Awake()
        {
            DeactivateAutoPilot();
        }

        /// <summary>
        /// Prepare the autopilot for activation
        /// </summary>
        /// <param name="quadToControl">The quadcopter for this Autopilot to control</param>
        public void Initialize(IQuadcopter quadToControl)
        {
            if (quadToControl == null)
            {
                Debug.LogError("Provided IQuadcopter was null");
                return;
            }
            this.quadToControl = quadToControl;
        }

        /// <summary>
        /// Get the <see cref="IInputs.FlightControlValues"/> from <see cref="Run"/>
        /// </summary>
        /// <returns>The inputs that will maniuplate the Quad in the desired manner</returns>
        public IInputs.FlightControlValues GetInputValues()
        {
            var returnValues = Run();
            returnValues.land = false;
            returnValues.takeOff = false;
            returnValues.toggleAutoPilot = false;
            return returnValues;
        }

        /// <summary>
        /// The calculations used to manipulate <see cref="quadToControl"/> in the desired way
        /// </summary>
        /// <returns>The inputs that will maniuplate the Quad in the desired manner</returns>
        public abstract IInputs.FlightControlValues Run();

        /// <summary>
        /// Toggle the autopilot to the opposite state that it currently is
        /// </summary>
        public virtual void ToggleAutoPilot()
        {
            if (autoPilotActive)
            {
                DeactivateAutoPilot();
            }
            else
            {
                ActivateAutoPilot();
            }
        }

        /// <summary>
        /// Activated the autopilot, <see cref="quadToControl"/> input source will be changed to <see cref="GetInputValues"/>
        /// </summary>
        public void ActivateAutoPilot()
        {
            if (!autoPilotActive)
            {
                Debug.Log("AutoPilot Enabled");
                autoPilotActive = true;
                gameObject.SetActive(true);
                MatchQuadTransform();
                quadToControl.OverrideInputSource(GetInputValues, DeactivateAutoPilot);
                quadToControl.SubscibeToAbort(DeactivateAutoPilot);
                OnAutoPilotActivated();
            }
        }
        /// <summary>
        /// Called when autopilot has become active
        /// </summary>
        protected abstract void OnAutoPilotActivated();

        /// <summary>
        /// Deactivate the autopilot, <see cref="quadToControl"/> input values for <see cref="quadToControl"/> will be returned to default
        /// </summary>
        public void DeactivateAutoPilot()
        {
            if (autoPilotActive)
            {
                Debug.Log("AutoPilot Disabled");
                autoPilotActive = false;
                quadToControl.RemoveInputOverride(GetInputValues, DeactivateAutoPilot);
                quadToControl.UnsubscribeFromAbort(DeactivateAutoPilot);
                OnAutoPilotDeactivated();
            }
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Called when autopilot has been deactivated
        /// </summary>
        protected abstract void OnAutoPilotDeactivated();
        /// <summary>
        /// Get the <see cref="GameObject"/> this component belongs to
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject()
        {
            return gameObject;
        }
        /// <summary>
        /// Is the autopilot currently active
        /// </summary>
        /// <returns>The state of the autopilot</returns> /// <summary>
        public bool IsActive()
        {
            return autoPilotActive;
        }

        /// <summary>
        /// Maniplate this objects <see cref="Transform"/> to match <see cref="quadToControl"/>
        /// </summary>
        protected void MatchQuadTransform()
        {
            transform.position = quadToControl.GetGameObject().transform.position;
            SetAutoPilotRot(quadToControl.GetGameObject().transform.rotation);
        }

        /// <summary>
        /// Set the rotaion of this objects <see cref="Transform"/> to match the provided rotation, global X and global y will be nullified
        /// </summary>
        /// <param name="newRot">The new rotation of this transform</param>
        public void SetAutoPilotRot(Quaternion newRot)
        {
            var tempEuler = newRot.eulerAngles;
            tempEuler.x = 0;
            tempEuler.z = 0;
            transform.rotation = Quaternion.Euler(tempEuler);
        }

        private void OnDestroy()
        {
            DeactivateAutoPilot();
        }
    }
}
