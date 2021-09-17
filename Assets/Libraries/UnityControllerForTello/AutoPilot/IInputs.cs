namespace UnityControllerForTello
{
    /// <summary>
    /// Interface for any source of input to <see cref="IQuadcopter"/>
    /// </summary>
    public interface IInputs
    {
        /// <summary>
        /// The values <see cref="IQuadcopter"/> needs to funtion
        /// </summary>
        public struct FlightControlValues
        {
            public float pitch;
            public float yaw;
            public float roll;
            public float throttle;

            public float speed;

            public bool takeOff;
            public bool land;
            public bool toggleAutoPilot;
        }
        /// <summary>
        /// The source of the inputs
        /// </summary>
        /// <returns>The inputs <see cref="IQuadcopter"/></returns> will attempt to execute
        public FlightControlValues GetInputValues();
    } 
}
