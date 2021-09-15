using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// The autonomous flight controller which controlls <see cref="TelloManager"/>
    /// </summary>
    public class TelloAutoPilot : MonoBehaviour
    {
        private SceneManager sceneManager;

        /// <summary>
        /// The target the Tello will be attempting to match
        /// </summary>
        private Transform targetDrone;
        private PidController proximityPIDX;
        private PidController proximityPIDY;
        private PidController proximityPIDZ;
        private PidController yawPID;

        /// <summary>
        /// The PID Profile to use for the PID Controllers
        /// </summary>
        public PIDProfile PIDprofile;

        public Transform targetPoint;

        /// <summary>
        /// The current target <see cref="targetDrone"/> is heading towardes 
        /// </summary>
        private Transform currentTargetPoint;


        /// <summary>
        /// The designated home point, set via <see cref="SetHomePoint(Vector3)"/>
        /// </summary>
        private Transform homePoint;

        private Vector3 pointAssignedEuler, pointAssignedPos;
        private float pointAssignedTime;
        private float targetDist;
        public float targetSpeed;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            targetDrone = GameObject.Find("Target Tello").transform;
            targetDrone.gameObject.SetActive(false);
            enabled = false;
        }

        public void SetHomePoint(Vector3 position)
        {
            Debug.Log("Home point updated");
            homePoint = new GameObject("Home Point").transform;
            homePoint.position = position;
            DebugVector.DrawVector(homePoint, Color.red, Color.green, Color.blue, Vector3.one);
        }

        public Quaternion RunAutoPilot(System.TimeSpan deltaTime)
        {
            if (currentTargetPoint != targetPoint)
            {
                Debug.Log("Set new target point");
                currentTargetPoint = targetPoint;
                targetDist = Vector3.Distance(sceneManager.activeDrone.position, currentTargetPoint.position);
                pointAssignedEuler = sceneManager.activeDrone.eulerAngles;
                pointAssignedPos = sceneManager.activeDrone.position;
                pointAssignedTime = Time.time;
                targetDrone.eulerAngles = currentTargetPoint.eulerAngles;
            }
            if (currentTargetPoint)
            {
                var distCovered = (Time.time - pointAssignedTime) * targetSpeed;

                float fracJourney = distCovered / targetDist;
                targetDrone.position = Vector3.Lerp(pointAssignedPos, currentTargetPoint.position, fracJourney);
                // targetDrone.position = Vector3.Lerp(targetDrone.position, currentTargetPoint.position, Time.deltaTime * .5f);//, fracJourney);
            }

            var targetOffset = sceneManager.activeDrone.position - targetDrone.position;

            if (targetOffset.magnitude > .1f || sceneManager.sceneType == SceneManager.SceneType.SimOnly)
            {
                proximityPIDX.ProcessVariable = targetOffset.x;
                double trgtRoll = proximityPIDX.ControlVariable(deltaTime);

                proximityPIDY.ProcessVariable = targetOffset.y;
                double trgtElv = proximityPIDY.ControlVariable(deltaTime);
                proximityPIDZ.ProcessVariable = targetOffset.z;
                double trgtPitch = proximityPIDZ.ControlVariable(deltaTime);

                var yawError = sceneManager.activeDrone.eulerAngles.y - targetDrone.eulerAngles.y;

                if (yawError < -180)
                    yawError = 360 - System.Math.Abs(yawError);
                else if (yawError > 180)
                    yawError = -(360 - yawError);

                yawPID.ProcessVariable = yawError;
                double trgtYaw = yawPID.ControlVariable(deltaTime);
                return new Quaternion((float)trgtYaw, (float)trgtElv, (float)trgtRoll, (float)trgtPitch);
            }
            else
                return new Quaternion(0, 0, 0, 0);
        }

        /// <summary>
        /// Enable or disable the Autopilot
        /// Autopilot will be auto disabled whenever user input is detected
        /// </summary>
        /// <param name="active">true or false</param>
        public void ToggleAutoPilot(bool active)
        {
            if (active)
            {
                ToggleAutoPilotOn();
            }
            else
            {
                ToggleAutoPilotOff();
            }
        }
        private void ToggleAutoPilotOn()
        {
            if (!enabled)
            {
                Debug.Log("AutoPilot Enabled");
                targetDrone.gameObject.SetActive(true);
                targetDrone.position = sceneManager.activeDrone.position;
                UpdatePIDValues(PIDprofile);
                enabled = true;
            }
        }
        private void ToggleAutoPilotOff()
        {
            if (enabled)
            {
                Debug.Log("AutoPilot Disabled");
                targetDrone.gameObject.SetActive(false);
                enabled = false;
            }
        }

        /// <summary>
        /// Update the PID values for the controller, stored in <see cref="PIDProfile"/>
        /// </summary>
        /// <param name="newPIDprofile">The new profile to use</param>
        private void UpdatePIDValues(PIDProfile newPIDprofile)
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
    }
}