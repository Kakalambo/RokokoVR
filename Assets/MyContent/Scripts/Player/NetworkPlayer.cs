using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Wave.Essence.Hand.StaticGesture;
using Wave.Essence.Hand.Model;
using Wave.Essence.Hand;

[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : MonoBehaviourPun
{
    [Header("GameObjects")]
    [SerializeField] private GameObject HandMeshL;
    [SerializeField] private GameObject HandMeshR;
    [SerializeField] private GameObject HandTransforms;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject PlayerModel;
    [SerializeField] private GameObject PlayerModelHead;
    private PlayerController playerController;

    [Header("Drawing")]
    [SerializeField] private ParticleSystem particleL;
    [SerializeField] private ParticleSystem particleR;

    public bool canDraw { get; set; } = true;
    [SerializeField] private Transform HandLDrawTranform;
    [SerializeField] private Transform HandRDrawTranform;

    public PlayerGroup group { get; set; }

    private PhotonView thisphotonView;
    private string handStateL;
    private string handStateR;

    // Start is called before the first frame update
    void Awake()
    {
        GetVariables();
        SetupForClient(thisphotonView.IsMine);
        SetupForGroupRPC();
    }

    void GetVariables()
    {
        thisphotonView = gameObject.GetComponent<PhotonView>();
        playerController = FindObjectOfType<PlayerController>();
    }

    private void SetupForGroupRPC()
    {
        if (this.photonView.IsMine)
        {
            group = playerController.PlayerGroups[Random.Range(0, playerController.PlayerGroups.Length)];
            thisphotonView.RPC("SetupForGroup", RpcTarget.All,  group.GroupID, group.GroupColor.r, group.GroupColor.g, group.GroupColor.b, group.GroupColor.a);
        }
    }

    [PunRPC]
    private void SetupForGroup(int gID, float r, float g, float b, float a)
    {

        group.GroupColor = new Color(r, g, b, a);
        group.GroupID = gID;
        particleL.startColor = group.GroupColor;
        Renderer[] modelChildrenRenderer = PlayerModel.transform.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in modelChildrenRenderer)
        {
            renderer.material.color = group.GroupColor;
        }
    }

    private void SetupForClient(bool isOwner)
    {
        if (!isOwner)
        {
            HandMeshL.SetActive(false);
            HandMeshR.SetActive(false);
            particleL.gameObject.transform.SetParent(transform);
            particleR.gameObject.transform.SetParent(transform);
            HandTransforms.SetActive(false);
            Camera.SetActive(false);
        }
        else
        {
            
            playerController.OwningPlayer = this;
            MeshRenderer[] playerMeshRenderers = PlayerModel.transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in playerMeshRenderers)
            {
                mr.enabled = false;
               
            }

            SkinnedMeshRenderer[] playerSkinnedMeshRenderers = PlayerModel.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer smr in playerSkinnedMeshRenderers)
            {
                smr.enabled = false;
            }
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (thisphotonView.IsMine)
        {
            UpdateHandGestures();
            SetModelPosition();
        }
    }

    void SetModelPosition()
    {
        PlayerModel.transform.position = Camera.transform.position;
        PlayerModel.transform.rotation = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
        //PlayerModelHead.transform.rotation = Quaternion.Euler(Camera.transform.rotation.eulerAngles);
    }

    void UpdateHandGestures()
    {
        particleL.transform.position = HandLDrawTranform.position;
        particleR.transform.position = HandRDrawTranform.position;


        handStateL = WXRGestureHand.GetSingleHandGesture(true);
        handStateR = WXRGestureHand.GetSingleHandGesture(false);

        CheckForDraw();
    }

    public void DisableOrEnableParticlesRPC(bool canIDraw)
    {
        thisphotonView.RPC("DisableOrEnableParticles", RpcTarget.All, canIDraw);
    }

    [PunRPC]
    public void DisableOrEnableParticles(bool canIDraw)
    {
        canDraw = canIDraw;
        if (!canIDraw)
        {
            particleL.Play();
            particleR.Play();
            particleL.Stop();
            particleR.Stop();
            var emissionL = particleL.emission;
            var emissionR = particleR.emission;
            emissionL.enabled = false;
            emissionR.enabled = false;
        }
        else
        {
            particleL.Play();
            particleR.Play();
            var emissionL = particleL.emission;
            var emissionR = particleR.emission;
            emissionL.enabled = true;
            emissionR.enabled = true;
        }
    }


    void CheckForDraw()
    {
        if (!canDraw)
        {
            return;
        }

        

    // Left hand
      if(!particleL.isPlaying)
        {
            if (canDraw && handStateL == "IndexUp")
            {
                thisphotonView.RPC("SetParticleSystemL", RpcTarget.All, true);
            }
        }

      else if (particleL.isPlaying)
        {
            if (!canDraw || handStateL != "IndexUp")
            {
                thisphotonView.RPC("SetParticleSystemL", RpcTarget.All, false);
            }
        }

        // Right hand
        if (!particleR.isPlaying)
        {
            if (canDraw && handStateR == "IndexUp")
            {
                thisphotonView.RPC("SetParticleSystemR", RpcTarget.All, true);
            }
        }

        else if (particleR.isPlaying)
        {
            if (!canDraw || handStateR != "IndexUp")
            {
                thisphotonView.RPC("SetParticleSystemR", RpcTarget.All, false);
            }
        }
    }   

    [PunRPC]
    private void SetParticleSystemL(bool isplaying)
    {
        if (isplaying)
        {
            particleL.Play();
        }
        else
        {
            particleL.Pause();
        }
    }

    [PunRPC]
    private void SetParticleSystemR(bool isplaying)
    {
        if (isplaying)
        {
            particleR.Play();
        }
        else
        {
            particleR.Pause();
        }
    }

    public void FindAndHideOtherPlayers(bool HideGroup)
    {
        NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();

        foreach (NetworkPlayer nPlayer in allPlayers)
        {
            if (!nPlayer.thisphotonView.IsMine)
            {
                if (HideGroup)
                {
                    nPlayer.HidePlayer();
                }
                else if (nPlayer.group.GroupID != group.GroupID)
                {
                    nPlayer.HidePlayer();
                }
            }
        }
    }

    public void FindAndShowOtherPlayers(bool ShowGroup)
    {
        NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();

        foreach (NetworkPlayer nPlayer in allPlayers)
        {
            if (!nPlayer.thisphotonView.IsMine)
            {
                if (ShowGroup)
                {
                    nPlayer.ShowPlayer();
                }
                else if (nPlayer.group.GroupID != group.GroupID)
                {
                    nPlayer.ShowPlayer();
                }
            }
        }
    }

    public void HidePlayer()
    {
        particleL.gameObject.SetActive(false);
        particleR.gameObject.SetActive(false);
        MeshRenderer[] playerMeshRenderers = PlayerModel.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in playerMeshRenderers)
        {
            mr.enabled = false;
        }

        SkinnedMeshRenderer[] playerSkinnedMeshRenderers = PlayerModel.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in playerSkinnedMeshRenderers)
        {
            smr.enabled = false;
        }
    }

    public void ShowPlayer()
    {
        particleL.gameObject.SetActive(true);
        particleR.gameObject.SetActive(true);
        MeshRenderer[] playerMeshRenderers = PlayerModel.transform.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in playerMeshRenderers)
        {
            mr.enabled = true;
        }

        SkinnedMeshRenderer[] playerSkinnedMeshRenderers = PlayerModel.transform.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer smr in playerSkinnedMeshRenderers)
        {
            //smr.enabled = true;
        } 
    }
}
