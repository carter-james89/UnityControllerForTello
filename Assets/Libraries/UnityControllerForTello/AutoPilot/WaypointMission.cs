using System.Collections.Generic;
using UnityControllerForTello;
using UnityEngine;

public class WaypointMission : MonoBehaviour
{
    [SerializeField]
    private List<Waypoint> _waypoints;

    public Waypoint currentWaypoint { get; private set; }

    private PIDAutoPilot _autoPilot;

    [SerializeField]
    private bool _loopMission;

    public bool missionActive { get; private set; } = false;

    public void BeginMission(PIDAutoPilot autoPilot)
    {
        if (!autoPilot)
        {
            return;
        }
        missionActive = true;

        _autoPilot = autoPilot;
        _autoPilot.onAchievedTarget += OnQuadcopterAtTarget;

        if(_waypoints == null || _waypoints.Count == 0)
        {
            return;
        }
        _autoPilot.SetNewTarget(_waypoints[0].transform);
    }

    private void OnQuadcopterAtTarget(Transform targetTransform)
    {        
        for (int i = 0; i < _waypoints.Count; i++)
        {
            if(_waypoints[i].transform == targetTransform)
            {
                Debug.Log("Quad at Waypoint : " + _waypoints[i].name);
                if (i != _waypoints.Count - 1)
                {
                    Debug.Log("Set Next Waypoint : " + _waypoints[i + 1].name);
                    currentWaypoint = _waypoints[i + 1];
                    _autoPilot.SetNewTarget(currentWaypoint.transform);
                    return;
                }
                else
                {
                    if (_loopMission)
                    {
                        currentWaypoint = _waypoints[0];
                        _autoPilot.SetNewTarget(currentWaypoint.transform);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (_autoPilot)
        {
            _autoPilot.onAchievedTarget -= OnQuadcopterAtTarget;
        }
      
    }
}
