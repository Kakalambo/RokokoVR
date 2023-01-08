using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun, IPunObservable
{
    public NetworkPlayer OwningPlayer;
    

    [Header("Player Variables")]
    public bool ShowAllPlayers = true;
    public bool ShowOwnGroup = true;
    public bool ShowOtherGroup = true;
    public bool SeeOwnHands = true;

    [Header("Draw Variables")]
    public bool CanDraw = true;
    public bool PauseDraw = false;
    public int DrawIndicatorCount = 0;


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
        if (Input.GetKeyDown(KeyCode.N))
        {
            this.photonView.RPC("HideOtherPlayersFormOwningPlayer", RpcTarget.All, false);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            this.photonView.RPC("ShowOtherPlayersFormOwningPlayer", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            this.photonView.RPC("ShowOtherPlayersFormOwningPlayer", RpcTarget.All, false);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            this.photonView.RPC("ChangeDrawModeForPlayers", RpcTarget.All, false);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            this.photonView.RPC("ChangeDrawModeForPlayers", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            this.photonView.RPC("PausePlayerParticleSystems", RpcTarget.All, false);
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.photonView.RPC("PausePlayerParticleSystems", RpcTarget.All, true);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UpgradeDrawIndicatorsToMasterClient();
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
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpgradePlayerGroupsToMasterClient();
        }
    }

    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            
        }
        else
        {
           
        }
    }

    public void UpdateStatesForNewPlayerRPC()
    {
        this.photonView.RPC("UpdateStatesForNewPlayerMasterClient", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void UpdateStatesForNewPlayerMasterClient()
    {
        this.photonView.RPC("UpdateStatesForNewPlayer", RpcTarget.All, PauseDraw, CanDraw, ShowAllPlayers, ShowOwnGroup, SeeOwnHands);
    }

    [PunRPC]
    private void UpdateStatesForNewPlayer(bool pauseDraw, bool candraw, bool showallplayers, bool showowngroup, bool seeownhands)
    {
        PauseDraw = pauseDraw;
        CanDraw = candraw;
        ShowAllPlayers = showallplayers;
        ShowOwnGroup = showowngroup;
        SeeOwnHands = seeownhands;

        if (OwningPlayer)
        {
            if (PauseDraw)
            OwningPlayer.PlayOrPauseParticlesRPC(false);

            if (!CanDraw)
                this.photonView.RPC("ChangeDrawModeForPlayers", RpcTarget.All, false);

            if (!ShowAllPlayers && !ShowOwnGroup)
            {
                this.photonView.RPC("HideOtherPlayersFormOwningPlayer", RpcTarget.All, true);
            }
            else if (!ShowAllPlayers)
            {
                this.photonView.RPC("HideOtherPlayersFormOwningPlayer", RpcTarget.All, false);
            }
            else if (ShowAllPlayers)
            {
                this.photonView.RPC("ShowOtherPlayersFormOwningPlayer", RpcTarget.All, true);
            }

            if (SeeOwnHands)
            {
                this.photonView.RPC("ShowHandsForPlayers", RpcTarget.All, true);
            }
            else
            {
                this.photonView.RPC("ShowHandsForPlayers", RpcTarget.All, false);
            }

        }
    }

    [PunRPC]
    private void PausePlayerParticleSystems(bool play)
    {

        PauseDraw = !play;

        if (!play)
        {
            CanDraw = false;
        }
        if (OwningPlayer)
        {
            OwningPlayer.PlayOrPauseParticlesRPC(play);
        }
    }

    [PunRPC]
    private void ChangeDrawModeForPlayers(bool canIDraw)
    {
        CanDraw = canIDraw;

        if (canIDraw)
            PauseDraw = false;

        if (OwningPlayer)
        {
            OwningPlayer.DisableOrEnableParticlesRPC(canIDraw); ;
        }   
    }

    [PunRPC]
    private void HideOtherPlayersFormOwningPlayer(bool HideGroup)
    {
        if (HideGroup)
        {
            ShowAllPlayers = false;
            ShowOwnGroup = false;
            ShowOtherGroup = false;
        }
        else
        {
            ShowAllPlayers = false;
            ShowOtherGroup = false;
        }

        if (OwningPlayer)
        {
            OwningPlayer.FindAndHideOtherPlayers(HideGroup);
        }   
    }

    [PunRPC]
    private void ShowOtherPlayersFormOwningPlayer(bool AlsoShowOtherGroup)
    {
        if (AlsoShowOtherGroup)
        {
            ShowAllPlayers = true;
            ShowOwnGroup = true;
            ShowOtherGroup = true;
        }
        else
        {
            ShowOwnGroup = true;

            if (ShowOtherGroup)
                ShowAllPlayers = true;
        }

        if (OwningPlayer)
        {
            OwningPlayer.FindAndShowOtherPlayers(ShowOtherGroup);
        }  
    }

    
    [PunRPC]
    private void SpawnDrawIndicatorForPlayers()
    {
        DrawIndicatorCount++;

        if (OwningPlayer)
        {
            OwningPlayer.SpawnDrawIndicator();
        }
    }


    [PunRPC]
    private void ShowHandsForPlayers(bool show)
    {

        SeeOwnHands = show;
        if (OwningPlayer)
        {
            if (show)
            {
                OwningPlayer.ShowPlayerHands();
            }
            else
            {
                OwningPlayer.HidePlayerHands();
            }
        } 
    }


    public void UpgradePlayerGroupsRPC()
    {
        this.photonView.RPC("UpgradePlayerGroups", RpcTarget.All);
    }

    [PunRPC]
    private void UpgradePlayerGroups()
    {
        if (OwningPlayer)
        {
            OwningPlayer.SetupForGroupRPC(OwningPlayer.PlayerGroups[OwningPlayer.GroupIndex]);
        }
    }


    private void UpgradePlayerGroupsToMasterClient()
    {
        NetworkPlayer[] nPlayers = FindObjectsOfType<NetworkPlayer>();
        foreach (NetworkPlayer n in nPlayers)
        {
            n.SetupForGroupRPC(n.PlayerGroups[n.GroupIndex], true);
        }
    }

    private void UpgradeDrawIndicatorsToMasterClient()
    {
        NetworkPlayer[] nPlayers = FindObjectsOfType<NetworkPlayer>();
        foreach (NetworkPlayer n in nPlayers)
        {
            n.SetupNewDrawIndicaorRPC(n.DrawIndicatorIndex);
        }
    }
}
