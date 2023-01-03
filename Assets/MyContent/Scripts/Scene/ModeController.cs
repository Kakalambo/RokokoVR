using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

public class ModeController : MonoBehaviour
{
    public Mode mode;

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SwitchModeForAndroid();
    }

    private void SwitchModeForAndroid()
    {

#if UNITY_EDITOR
        if (mode == Mode.Client)
        {
            mode = Mode.Server;
        }
        return;
#endif

#if UNITY_ANDROID
        mode = Mode.Client;
#endif

    }
    public void SetUpForMode()
    {
        SceneControl sceneControl = FindObjectOfType<SceneControl>();
        SceneClient sceneClient = FindObjectOfType<SceneClient>();
        Shader_Client shaderClient = FindObjectOfType<Shader_Client>();
        Shader_Control shaderControl = FindObjectOfType<Shader_Control>();

        PhotonView p;
        PhotonView p2;

        switch (mode)
        {
            case Mode.Server:
                p = sceneControl.gameObject.GetComponent<PhotonView>();
                p2 = shaderControl.gameObject.GetComponent<PhotonView>();

                p.ObservedComponents.Clear();
                p.ObservedComponents.Add(sceneControl);

                p2.ObservedComponents.Clear();
                p2.ObservedComponents.Add(shaderControl);

                sceneControl.enabled = true;
                sceneClient.enabled = false;

                shaderControl.enabled = true;
                shaderClient.enabled = false;

                break;

            case Mode.Client:
                p = sceneControl.gameObject.GetComponent<PhotonView>();
                p2 = shaderControl.gameObject.GetComponent<PhotonView>();

                p.ObservedComponents.Clear();
                p.ObservedComponents.Add(sceneClient);

                p2.ObservedComponents.Clear();
                p2.ObservedComponents.Add(shaderClient);

                sceneControl.enabled = false;
                sceneClient.enabled = true;

                shaderControl.enabled = false;
                shaderClient.enabled = true;
                break;

            case Mode.Stream:

                p = sceneControl.gameObject.GetComponent<PhotonView>();
                p2 = shaderControl.gameObject.GetComponent<PhotonView>();

                p.ObservedComponents.Clear();
                p.ObservedComponents.Add(sceneClient);

                p2.ObservedComponents.Clear();
                p2.ObservedComponents.Add(shaderClient);

                sceneControl.enabled = false;
                sceneClient.enabled = true;

                shaderControl.enabled = false;
                shaderClient.enabled = true;

                GameObject.Find("StreamCamera").SetActive(true);
                GameObject.Find("Hierarchy_UI").SetActive(false);
                GameObject.Find("[VRModule]").SetActive(false);
                break;
        }

        sceneControl.GetComponent<PhotonView>().ObservedComponents.Add(sceneControl.gameObject.GetComponent<PlayerController>());
    }

    public enum Mode {Server, Client, Stream}
}
