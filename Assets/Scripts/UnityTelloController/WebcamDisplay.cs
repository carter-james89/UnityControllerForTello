using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamDisplay : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
            StartCoroutine(GetAuthorizaton());
    }

    IEnumerator GetAuthorizaton()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam | UserAuthorization.Microphone))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
                Debug.Log(devices[i].name);

            var camTexture = new WebCamTexture();// devices[0].ToString(), 1920, 1080, 90);
            GetComponent<MeshRenderer>().material.SetTexture("_MainTex", camTexture);
            camTexture.Play();
        }
        else
        {
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}
