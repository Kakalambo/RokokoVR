using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine.SceneManagement;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 100;
        roomOptions.IsVisible = true;
        roomOptions.BroadcastPropsChangeToAll = true;

#if UNITY_EDITOR

        if (FindObjectOfType<ModeController>().mode == ModeController.Mode.Server)
        {
            PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.JoinRoom("Room 1");
        }
        
        return;
#endif

#if UNITY_ANDROID
        PhotonNetwork.JoinRoom("Room 1");
        return;
#endif

        PhotonNetwork.JoinRoom("Room 1");
        //PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions, TypedLobby.Default);

    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        //SceneManager.LoadScene("RokokoVR");
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");
        PhotonNetwork.LoadLevel("UnderInfluence");
    }
}
