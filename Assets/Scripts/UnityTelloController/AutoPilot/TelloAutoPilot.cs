using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityControllerForTello
{
    public class TelloAutoPilot : MonoBehaviour
    {
        Transform targetDrone;
        PidController proximityPIDX, proximityPIDY, proximityPIDZ, yawPID;
        public PIDProfile PIDprofile;
        SceneManager sceneManager;
        public Transform targetPoint;
        Transform currentTargetPoint;

        Vector3 pointAssignedEuler, pointAssignedPos;
        float pointAssignedTime;
        float targetDist;
        public float targetSpeed;

        private void Awake()
        {
            sceneManager = FindObjectOfType<SceneManager>();
            targetDrone = GameObject.Find("Target Tello").transform;
            targetDrone.gameObject.SetActive(false);
            enabled = false;
        }


        public Quaternion RunAutoPilot(System.TimeSpan deltaTime)
        {
            if(currentTargetPoint != targetPoint)
            {
                Debug.Log("Set new target point");
                currentTargetPoint = targetPoint;
                targetDist = Vector3.Distance(sceneManager.activeDrone.position,currentTargetPoint.position);
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
               // targetDrone.eulerAngles = Vector3.Lerp(pointAssignedEuler, currentTargetPoint.eulerAngles, fracJourney);
            }
            //  Debug.Log("Run autopilot with time " + prevDeltaTime);


           
            var targetOffset = sceneManager.activeDrone.position - targetDrone.position;
            // offsetFromTarget = targetOffset;

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

            var yawError = sceneManager.activeDrone.eulerAngles.y - targetDrone.eulerAngles.y;

            if (yawError < -180)
                yawError = 360 - System.Math.Abs(yawError);
            else if (yawError > 180)
                yawError = -(360 - yawError);

            //yawError = Quaternion.eulerAngles(yawErrorRot).y;
            // yawError = Vector3.Angle(targetDrone.forward, autoPilotTarget.forward);
            yawPID.ProcessVariable = yawError;
            double trgtYaw = yawPID.ControlVariable(deltaTime);

            return new Quaternion((float)trgtYaw, (float)trgtElv, (float)trgtRoll, (float)trgtPitch);
        }
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
        void ToggleAutoPilotOn()
        {
            if (!enabled)
            {
                Debug.Log("AutoPilot Enabled");
                targetDrone.gameObject.SetActive(true);
                UpdatePIDValues(PIDprofile);
                enabled = true;
            }
        }
        void ToggleAutoPilotOff()
        {
            if (enabled)
            {
                Debug.Log("AutoPilot Disabled");
                targetDrone.gameObject.SetActive(false);
                enabled = false;
            }
        }
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
    }

}