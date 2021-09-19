using System;
using System.Collections.Generic;
using UnityControllerForTello;
using UnityEngine;

public class WaypointMission : MonoBehaviour
{
    [SerializeField]
    private List<Waypoint> _waypoints;

    public Waypoint currentWaypoint { get; private set; }

    public Waypoint startWaypoint;

    private WaypointAutoPilot _autoPilot;

    [SerializeField]
    private bool _loopMission;

    public bool IsFinalWaypoint(Waypoint waypointToCheck)
    {
        if(waypointToCheck == _waypoints[_waypoints.Count - 1])
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool missionActive { get; private set; } = false;

    private int _waypointCount;

    public void OnMissionBegun(WaypointAutoPilot autoPilot)
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
        _waypointCount = 0;
        _autoPilot = autoPilot;
        _autoPilot.onWaypointAchieved += OnQuadcopterAtTarget;
        _autoPilot.onWaypointSet += OnNewWaypointSet;
        if (startWaypoint)
        {
            autoPilot.SetNewWaypoint(startWaypoint);
        }
        else
            HeadToNextWaypoint();
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
        if (missionActive && newWaypointSet != currentWaypoint && newWaypointSet != startWaypoint)
        {
            Debug.Log("A waypoint has been set from outside the Mission : " + newWaypointSet);
            EndMission();
        }
    }

    private void OnQuadcopterAtTarget(Waypoint targetTransform)
    {
        Debug.Log("Quad at Waypoint : " + _waypointCount);

        if (_waypointCount != _waypoints.Count)
        {
            Debug.Log("Set Next Waypoint : " + _waypoints[_waypointCount].name);
            HeadToNextWaypoint();
            return;
        }
        else
        {
            if (_loopMission)
            {
                _waypointCount = 0;
                HeadToNextWaypoint();
            }
            else
            {
                EndMission();
            }
        }
    }

    private void HeadToNextWaypoint()
    {
        currentWaypoint = _waypoints[_waypointCount];
        _autoPilot.SetNewWaypoint(currentWaypoint);
        _waypointCount++;
    }

    private void OnDestroy()
    {
        EndMission();
    }
}
