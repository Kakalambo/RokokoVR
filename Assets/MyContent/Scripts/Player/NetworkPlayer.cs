using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Wave.Essence.Hand.StaticGesture;
using Wave.Essence.Hand.Model;
using Wave.Essence.Hand;

[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : MonoBehaviourPun, IPunObservable
{
    [Header("GameObjects")]
    [SerializeField] private GameObject HandMeshL;
    [SerializeField] private GameObject HandMeshR;
    [SerializeField] private GameObject ParticleMeshL;
    [SerializeField] private GameObject ParticleMeshR;
    [SerializeField] private GameObject HandTransforms;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject PlayerModel;
    [SerializeField] private GameObject PlayerModelHead;
    [SerializeField] private GameObject PlayerIndicator;
    private PlayerController playerController;

    [Header("Drawing")]
    [SerializeField] private ParticleSystem particleL;
    [SerializeField] private ParticleSystem particleR;
    [SerializeField] private GameObject DrawIndicatorPrefab;
    private int oldID;
    private Vector3 playerColor =  new Vector3(255,255,255);
    private DrawIndicator drawIndicator;
    public SpawnPlace spawnPlace{ get; set;}

    public bool canDraw { get; set; } = true;
    [SerializeField] private Transform HandLDrawTranform;
    [SerializeField] private Transform HandRDrawTranform;

    public PlayerGroup group;

    private PhotonView thisphotonView;
    private string handStateL;
    private string handStateR;

    // Start is called before the first frame update
    void Awake()
    {
        GetVariables();
        SetupForClient(thisphotonView.IsMine);

    }

    void GetVariables()
    {
        thisphotonView = gameObject.GetComponent<PhotonView>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void SetupForGroupRPC(PlayerGroup newGroup)
    {
        if (this.photonView.IsMine)
        {
            group = newGroup;
            playerColor = new Vector3( group.GroupColor.r, group.GroupColor.g, group.GroupColor.b);
            thisphotonView.RPC("SetupForGroup", RpcTarget.All,  group.GroupID, group.GroupColor.r, group.GroupColor.g, group.GroupColor.b, group.GroupColor.a);
        }
    }

    [PunRPC]
    private void SetupForGroup(int gID, float r, float g, float b, float a)
    {
        if (group == null)
        {
            group = new PlayerGroup();
        }
        group.GroupColor = new Color(r, g, b, a);
        group.GroupID = gID;
        particleL.startColor = group.GroupColor;
        particleR.startColor = group.GroupColor;
        Renderer[] modelChildrenRenderer = PlayerModel.transform.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in modelChildrenRenderer)
        {
            renderer.material.color = group.GroupColor;
        }

        ParticleMeshL.GetComponent<Renderer>().material.color = group.GroupColor;
        ParticleMeshR.GetComponent<Renderer>().material.color = group.GroupColor;

        PlayerIndicator.GetComponent<Renderer>().material.color = new Color(r, g, b, .1f); ;
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

            ParticleMeshL.GetComponent<SkinnedMeshRenderer>().enabled = false;
            ParticleMeshR.GetComponent<SkinnedMeshRenderer>().enabled = false;
            PlayerIndicator.SetActive(false);
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

        Vector3 pos = PlayerIndicator.transform.position;
        PlayerIndicator.transform.position = new Vector3(pos.x, 0, pos.z);
        //PlayerModelHead.transform.rotation = Quaternion.Euler(Camera.transform.rotation.eulerAngles);
    }

    void UpdateHandGestures()
    {
        particleL.transform.position = HandLDrawTranform.position;
        particleR.transform.position = HandRDrawTranform.position;


        handStateL = WXRGestureHand.GetSingleHandGesture(true);
        handStateR = WXRGestureHand.GetSingleHandGesture(false);;

        CheckForDraw();
    }

    public void DisableOrEnableParticlesRPC(bool canIDraw)
    {
        thisphotonView.RPC("DisableOrEnableParticles", RpcTarget.All, canIDraw);
    }

    public void PlayOrPauseParticlesRPC(bool play)
    {
        thisphotonView.RPC("PlayOrPauseParticleSystems", RpcTarget.All, play);
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
            if (drawIndicator != null)
            {
                drawIndicator.DestroyIndicator(0);
            }
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
            var emissionL = particleL.emission;
            emissionL.enabled = true;
        }
        else
        {
            particleL.Stop();
            var emissionL = particleL.emission;
            emissionL.enabled = false;
        }
    }

    [PunRPC]
    private void SetParticleSystemR(bool isplaying)
    {
        if (isplaying)
        {
            particleR.Play();
            var emissionR = particleL.emission;
            emissionR.enabled = true;
        }
        else
        {
            particleR.Stop();
            var emissionR = particleL.emission;
            emissionR.enabled = false;
        }
    }

    [PunRPC]
    private void PlayOrPauseParticleSystems(bool play)
    {
        if (play)
        {
            particleR.Play();
            particleL.Play();
        }
        else
        {
            canDraw = false;
            particleR.Pause();
            particleL.Pause();
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
        PlayerIndicator.SetActive(false);
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
        PlayerIndicator.SetActive(true);
    }

    public void ShowPlayerHands()
    {
        HandMeshL.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        HandMeshR.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
    }

    public void HidePlayerHands()
    {
        HandMeshL.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        HandMeshR.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
    }

    public void SpawnDrawIndicator()
    {
        if (drawIndicator != null)
        {
            drawIndicator.DestroyIndicator(0);
        }

        GameObject g = Instantiate(DrawIndicatorPrefab, Camera.transform.position, Camera.transform.rotation, Camera.transform);
        drawIndicator = g.GetComponent<DrawIndicator>();

        drawIndicator.transform.localPosition += new Vector3(0, -0.2f, 1) * .5f;
        drawIndicator.transform.localRotation = Quaternion.Euler(0, 180, 0);
        drawIndicator.transform.parent = null;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerColor);
            stream.SendNext(canDraw);
            stream.SendNext(group.GroupID);
        }
        else
        {
            this.playerColor = (Vector3)stream.ReceiveNext();
            this.canDraw = (bool)stream.ReceiveNext();
            this.group.GroupID = (int)stream.ReceiveNext(); 

            if (oldID != group.GroupID)
            {
                oldID = group.GroupID;
                //SetupForGroup(group.GroupID, this.playerColor.x, this.playerColor.y, this.playerColor.z, 255);
            }
        }
    }
}
