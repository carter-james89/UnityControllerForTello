using TelloLib;
using UnityEngine;

namespace UnityControllerForTello
{
    /// <summary>
    /// Controller which controls the Game UI, and displays the Tello's video feed to a texture
    /// </summary>
    public class TelloVideoFeed : MonoBehaviour
    {
        /// <summary>
        /// The texture to pass the video data to
        /// </summary>
        [SerializeField]
        private TelloVideoTexture telloVideoTexture;

        /// <summary>
        /// Initialize the vido feed to listen for <see cref="Tello.onVideoData"/>
        /// </summary>
        /// <param name="tello">The Quadcopter to get video from</param>
        public void InitializeFeed(TelloQuadcopter tello)
        {
            Debug.Log("Initialize video feed");
            Tello.onVideoData += Tello_onVideoData;
        }
        /// <summary>
        /// Called from <see cref="Tello.onVideoData"/>
        /// </summary>
        /// <param name="data"></param>
        private void Tello_onVideoData(byte[] data)
        {
            if (telloVideoTexture != null)
            {
                telloVideoTexture.PutVideoData(data);
            }
            else
            {
                Debug.LogWarning("Recieving video, but telloVideoTexture is null, assign in inspector");
            }
        }

        private void OnDestroy()
        {
            Tello.onVideoData -= Tello_onVideoData;
        }
    } 
}
