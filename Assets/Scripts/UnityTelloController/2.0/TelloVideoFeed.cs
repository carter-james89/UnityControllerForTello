using System.Collections;
using System.Collections.Generic;
using TelloLib;
using UnityEngine;

public class TelloVideoFeed : MonoBehaviour
{
    [SerializeField]
    private TelloVideoTexture telloVideoTexture;

    public void InitializeFeed(TelloQuadcopter tello)
    {
        Debug.Log("Initialize video feed");
        Tello.onVideoData += Tello_onVideoData;
    }

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
