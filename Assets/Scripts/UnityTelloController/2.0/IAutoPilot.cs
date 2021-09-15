using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAutoPilot
{
    public void ActivateAutoPilot(IQuadcopter quadcopter);

    public void DeactivateAutoPilot();

    public GameObject GetGameObject();

    public PilotInputs.PilotInputValues CalculateInputs(System.TimeSpan deltaTime);

    public bool IsActive();
}
