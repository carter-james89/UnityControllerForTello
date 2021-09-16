using System.Collections.Generic;
using UnityEngine;


namespace UnityControllerForTello
{
    /// <summary>
    /// Draws a line of <see cref="TrailPoint"/>s to represent the Trail a <see cref="Quadcopter"/> has traveled
    /// </summary>
    public class TrailVisualizer : MonoBehaviour
    {
        /// <summary>
        /// List of all the flight points placed for the craft
        /// </summary>
        private List<TrailPoint> trailPoints;

        [SerializeField]
        private TrailPoint _trailPoint;

        /// <summary>
        /// How far does the craft have to travel to place a new <see cref="FlightPoint"/>
        /// </summary>
        [SerializeField]
        private float _minDeltaForPoint = .001f;

        /// <summary>
        /// The <see cref="Quadcopter"/> that will have its trail visualized
        /// </summary>
        private Quadcopter _quadToDraw;

        /// <summary>
        /// So you can disable it in inspector to not draw path
        /// TODO : Makes this possible in code
        /// </summary>
        private void Start()
        {
        }

        /// <summary>
        /// Provide a <see cref="Quadcopter"/> to visual the trail
        /// </summary>
        public void Initialize(Quadcopter quadcopterToDraw)
        {
            _quadToDraw = quadcopterToDraw;
            _quadToDraw.onTransformChanged += CreateFlightPoint;
        }

        private void OnDestroy()
        {
            if (_quadToDraw)
            {
                _quadToDraw.onTransformChanged -= CreateFlightPoint;
            }
        }

        /// <summary>
        /// If the new craftPostion is a distance greater than <see cref="_minDeltaForPoint"/> from the last <see cref="FlightPoint"/>,
        /// place a new FlightPoint;
        /// </summary>
        /// <param name="craftPosition">The new craft position in Global Space</param>
        private void CreateFlightPoint(Vector3 craftPosition, Quaternion craftRotation)
        {
            if (enabled)
            {
                if (trailPoints == null)
                {
                    trailPoints = new List<TrailPoint>();
                    trailPoints.Add(InstantiateNewPoint(craftPosition));
                }
                else
                {
                    Vector3 flightPointDif = trailPoints[trailPoints.Count - 1].transform.position - craftPosition;
                    Debug.Log(flightPointDif.magnitude);
                    if (flightPointDif.magnitude > _minDeltaForPoint)
                    {
                        var trailPoint = InstantiateNewPoint(craftPosition);

                        if (trailPoints.Count > 0)
                        {
                            trailPoint.SetPoints(craftPosition, trailPoints[trailPoints.Count - 1].transform.position);
                        }
                        trailPoints.Add(trailPoint);
                    }
                }
            }
        }

        private TrailPoint InstantiateNewPoint(Vector3 position)
        {
            var trailPoint = Instantiate(_trailPoint);
            trailPoint.transform.position = position;
            trailPoint.hideFlags = HideFlags.HideInHierarchy;
            return trailPoint;
        }
    }

}