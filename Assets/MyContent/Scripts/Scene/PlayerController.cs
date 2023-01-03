using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public PlayerGroup[] PlayerGroups;
    public NetworkPlayer OwningPlayer;
    public bool canDraw = false;
    public bool canSeeGroup = false;
    public bool canSeeAll = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            this.photonView.RPC("HideOtherPlayersFormOwningPlayer", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.photonView.RPC("ShowOtherPlayersFormOwningPlayer", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            this.photonView.RPC("ChangeDrawModeForPlayers", RpcTarget.All, false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.photonView.RPC("ChangeDrawModeForPlayers", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            this.photonView.RPC("SpawnDrawIndicatorForPlayers", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            this.photonView.RPC("ShowHandsForPlayers", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            this.photonView.RPC("ShowHandsForPlayers", RpcTarget.All, false);
        }
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(canDraw);
        }
        else
        {
            this.canDraw = (bool)stream.ReceiveNext();
        }
    }

    [PunRPC]
    private void ChangeDrawModeForPlayers(bool canIDraw)
    {
        OwningPlayer.DisableOrEnableParticlesRPC(canIDraw);
    }

    [PunRPC]
    private void HideOtherPlayersFormOwningPlayer(bool HideGroup)
    {
        OwningPlayer.FindAndHideOtherPlayers(HideGroup);
    }

    [PunRPC]
    private void ShowOtherPlayersFormOwningPlayer(bool ShowGroup)
    {
        OwningPlayer.FindAndShowOtherPlayers(ShowGroup);
    }

    
    [PunRPC]
    private void SpawnDrawIndicatorForPlayers()
    {
        OwningPlayer.SpawnDrawIndicator();
    }

    [PunRPC]
    private void ShowHandsForPlayers(bool show)
    {
        if(show)
        {
            OwningPlayer.ShowPlayerHands();
        }
        else
        {
            OwningPlayer.HidePlayerHands();
        } 
    }
}
