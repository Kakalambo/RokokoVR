using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private SpawnPlace[] spawnPlaces;
    // Start is called before the first frame update
    void Start()
    {
        FindObjectOfType<ModeController>().SetUpForMode();
        SpawnNetworkPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

  
    void SpawnNetworkPlayer()
    {

#if UNITY_EDITOR
        return;
#endif

#if UNITY_ANDROID
        int count = FindObjectsOfType<NetworkPlayer>().Length;
        GameObject g = PhotonNetwork.Instantiate("PlayerCameraRig", spawnTransform.position, spawnTransform.rotation);
        NetworkPlayer n = g.GetComponent<NetworkPlayer>();
        n.group = spawnPlaces[count].group;
        n.spawnPlace = spawnPlaces[count];
        n.SetupForGroupRPC();
#endif
    }
}
