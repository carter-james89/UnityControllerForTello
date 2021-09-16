using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// A point representing a postion in global space a <see cref="Quadcopter"/> traveled
    /// Draws a line to the point before it in accordance to <see cref="TrailVisualizer"/>
    /// </summary>
    /// <remarks>
    /// Is hidden in inspector via <see cref="TrailVisualizer.InstantiateNewPoint(UnityEngine.Vector3)"/>
    /// </remarks>
    public class TrailPoint : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        public void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
        }

        public void SetPoints(Vector3 point0Pos, Vector3 point1Pos)
        {
            _lineRenderer.SetPosition(0, point0Pos);
            _lineRenderer.SetPosition(1, point1Pos);
        }
    } 
}
