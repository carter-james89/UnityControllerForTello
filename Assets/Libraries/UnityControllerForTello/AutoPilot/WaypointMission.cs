using System;
using System.Collections.Generic;
using UnityControllerForTello;
using UnityEngine;

public class WaypointMission : MonoBehaviour
{
    [SerializeField]
    private List<Waypoint> _waypoints;

    public Waypoint currentWaypoint { get; private set; }

    private WaypointAutoPilot _autoPilot;

    [SerializeField]
    private bool _loopMission;

    public bool missionActive { get; private set; } = false;

    public void BeginMission(WaypointAutoPilot autoPilot)
    {
        if (!autoPilot)
        {
            return;
        }
        if (_waypoints == null || _waypoints.Count == 0)
        {
            return;
        }
        missionActive = true;

        _autoPilot = autoPilot;
        _autoPilot.onWaypointAchieved += OnQuadcopterAtTarget;
        _autoPilot.onWaypointSet += OnNewWaypointSet;

        currentWaypoint = _waypoints[0];
        _autoPilot.SetNewWaypoint(_waypoints[0]);
    }

    public void EndMission()
    {
        if (missionActive)
        {
            Debug.Log("End Mission : " + name);
            missionActive = false;
            _autoPilot.onWaypointAchieved -= OnQuadcopterAtTarget;
            _autoPilot.onWaypointSet -= OnNewWaypointSet;
        }  
    }

    private void OnNewWaypointSet(Waypoint newWaypointSet)
    {
        if (missionActive && newWaypointSet != currentWaypoint)
        {
            Debug.Log("A waypoint has been set from outside the Mission : " + newWaypointSet);
            EndMission();
        }
    }

    private void OnQuadcopterAtTarget(Waypoint targetTransform)
    {        
        for (int i = 0; i < _waypoints.Count; i++)
        {
            if(_waypoints[i] == targetTransform)
            {
                Debug.Log("Quad at Waypoint : " + _waypoints[i].name);
                if (i != _waypoints.Count - 1)
                {
                    Debug.Log("Set Next Waypoint : " + _waypoints[i + 1].name);
                    currentWaypoint = _waypoints[i + 1];
                    _autoPilot.SetNewWaypoint(currentWaypoint);
                    return;
                }
                else
                {
                    if (_loopMission)
                    {
                        currentWaypoint = _waypoints[0];
                        _autoPilot.SetNewWaypoint(currentWaypoint);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        EndMission();
    }
}
