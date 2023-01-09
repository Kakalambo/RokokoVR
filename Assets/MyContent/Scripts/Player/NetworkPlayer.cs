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
    [SerializeField] private SkinnedMeshRenderer HandMeshLSkinnedMeshRenderer;
    [SerializeField] private SkinnedMeshRenderer HandMeshRSkinnedMeshRenderer;
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
    [SerializeField] private GameObject[] DrawIndicatorPrefabs;
    [SerializeField] public int DrawIndicatorIndex = 0;
    public PlayerGroup[] PlayerGroups;
    [SerializeField] public int GroupIndex = 0;

    private bool isEmittingL = false;
    private bool isEmittingR = false;

    private int oldID;
    private DrawIndicator drawIndicator;
    public SpawnPlace spawnPlace{ get; set;}

    public bool canDraw { get; set; } = true;
    [SerializeField] private Transform HandLDrawTranform;
    [SerializeField] private Transform HandRDrawTranform;

    public PlayerGroup group;

    [HideInInspector] public PhotonView thisphotonView;
    private string handStateL;
    private string handStateR;
    private bool isLeftValid;
    private bool isRightValid;

    // Start is called before the first frame update
    void Awake()
    {
        GetVariables();
        SetupForClient(thisphotonView.IsMine);

        particleL.Play();
        particleR.Play();
    }

    void GetVariables()
    {
        thisphotonView = gameObject.GetComponent<PhotonView>();
        playerController = FindObjectOfType<PlayerController>();
    }

    public void SetupForGroupRPC(PlayerGroup newGroup, bool isMaster = false)
    {
        if (this.photonView.IsMine || isMaster)
        {
            thisphotonView.RPC("SetupForGroup", RpcTarget.All,  newGroup.GroupID);
        }
    }


    public void SetupNewDrawIndicaorRPC(int newDrawIndicatorIndex)
    {
        thisphotonView.RPC("SetupNewDrawIndicator", RpcTarget.All, newDrawIndicatorIndex);  
    }

    [PunRPC]
    private void SetupNewDrawIndicator(int newDrawIndicatorIndex)
    {
        DrawIndicatorIndex = newDrawIndicatorIndex;
    }

    [PunRPC]
    private void SetupForGroup(int gID)
    {
        GroupIndex = gID;
        group = PlayerGroups[GroupIndex];
        particleL.startColor = group.GroupColor;
        particleR.startColor = group.GroupColor;
        Renderer[] modelChildrenRenderer = PlayerModel.transform.GetComponentsInChildren<Renderer>();
        foreach(Renderer renderer in modelChildrenRenderer)
        {
            renderer.material.color = group.GroupColor;
        }

        ParticleMeshL.GetComponent<Renderer>().material.color = group.GroupColor;
        ParticleMeshR.GetComponent<Renderer>().material.color = group.GroupColor;

        PlayerIndicator.GetComponent<Renderer>().material.color = new Color(group.GroupColor.r, group.GroupColor.g, group.GroupColor.b, .1f); ;
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
            PlayerIndicator.GetComponent<MeshRenderer>().enabled = false;
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

        Vector3 pos = PlayerModel.transform.position;
        PlayerIndicator.transform.position = new Vector3(pos.x, 0, pos.z);
        //PlayerModelHead.transform.rotation = Quaternion.Euler(Camera.transform.rotation.eulerAngles);
    }

    void UpdateHandGestures()
    {
        particleL.transform.position = HandLDrawTranform.position;
        particleL.transform.rotation = Camera.transform.rotation;

        particleR.transform.position = HandRDrawTranform.position;
        particleR.transform.rotation = Camera.transform.rotation;


        handStateL = WXRGestureHand.GetSingleHandGesture(true);
        handStateR = WXRGestureHand.GetSingleHandGesture(false);

        CheckIfHandsShouldBeVisibleForOthers();
        CheckForDraw();
    }

    private void CheckIfHandsShouldBeVisibleForOthers()
    {
        if (thisphotonView.IsMine)
        {
            bool isLTracking = ((HandManager.Instance != null) &&
                            (HandManager.Instance.IsHandPoseValid(true)));

            bool isRTracking = ((HandManager.Instance != null) &&
                            (HandManager.Instance.IsHandPoseValid(false)));

            bool isLVisible = HandMeshL.activeSelf && isLTracking;
            bool isRVisible = HandMeshR.activeSelf&& isRTracking;
            
            if (isLeftValid != isLVisible)
            {
                isLeftValid = isLVisible;
                ShowParticleHandsForOthersRPC(true, isLeftValid);
            }

            if (isRightValid != isRVisible)
            {
                isRightValid = isRVisible;
                ShowParticleHandsForOthersRPC(false, isRightValid);
            }
            
        }
    }

    public void ShowParticleHandsForOthersRPC(bool isLeft, bool show)
    {
        thisphotonView.RPC("HideParticleHandsForOthers", RpcTarget.All, isLeft, show);
    }

    [PunRPC]
    public void HideParticleHandsForOthers(bool isLeft, bool show)
    {
        if (thisphotonView.IsMine)
            return;

        if (isLeft)
        {
            ParticleMeshL.GetComponent<SkinnedMeshRenderer>().enabled = show;
        }
        else
        {
            ParticleMeshR.GetComponent<SkinnedMeshRenderer>().enabled = show;
        }   
    }

    public void DisableOrEnableParticlesRPC(bool canIDraw)
    {
        thisphotonView.RPC("DisableOrEnableParticles", RpcTarget.All, canIDraw);
    }

    

    public void SetParticleLocally(bool enable)
    {
        particleL.gameObject.SetActive(enable);
        particleR.gameObject.SetActive(enable);

        if (enable)
        {
            particleL.Play();
            particleR.Play();
        }
    }

    [PunRPC]
    public void DisableOrEnableParticles(bool canIDraw)
    {
        canDraw = canIDraw;
        if (!canIDraw)
        {
            isEmittingR = false;
            isEmittingL = false;

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
            isEmittingR = true;
            isEmittingL = true;

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

        if (canDraw && handStateL == "IndexUp")
        {
            thisphotonView.RPC("SetParticleSystemL", RpcTarget.All, true);
        }
        else if (!canDraw || handStateL != "IndexUp")
        {
            thisphotonView.RPC("SetParticleSystemL", RpcTarget.All, false);
        }

        if (canDraw && handStateR == "IndexUp")
        {
            thisphotonView.RPC("SetParticleSystemR", RpcTarget.All, true);
        }
        else if (!canDraw || handStateR != "IndexUp")
        {
            thisphotonView.RPC("SetParticleSystemR", RpcTarget.All, false);
        }

        return;
        // Left hand
        if (!particleL.isPlaying)
        {
            if (canDraw && handStateL == "IndexUp")
            {
                thisphotonView.RPC("SetParticleSystemL", RpcTarget.All, true);
            }
        }

      else if (particleL.isPlaying )
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

        else if (particleR.isPlaying )
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
        var emissionL = particleL.emission;

        if (isplaying)
        {
            if (!isEmittingL || !particleL.isPlaying)
            {
                //particleL.Play();
                isEmittingL = true;
                emissionL.enabled = true;
            }
        }
        else
        {
            if (isEmittingL || particleL.isPlaying)
            {
                isEmittingL = false;
                emissionL.enabled = false;
            }
        }
    }

    [PunRPC]
    private void SetParticleSystemR(bool isplaying)
    {
        var emissionR = particleR.emission;

        if (isplaying)
        {
            if (!isEmittingR || !particleR.isPlaying)
            {
                //particleR.Play();
                isEmittingR = true;
                emissionR.enabled = true;
            }
            
        }
        else
        {
            if (isEmittingR || particleR.isPlaying)
            {
                isEmittingR = false;
                emissionR.enabled = false;
            }
            
        }
    }

    public void PlayOrPauseParticlesRPC(bool play)
    {
        thisphotonView.RPC("PlayOrPauseParticleSystems", RpcTarget.All, play);
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

    public void FindAndHideOtherPlayers(bool AlsoHideGroup)
    {
        NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();

        foreach (NetworkPlayer nPlayer in allPlayers)
        {
            if (!nPlayer.thisphotonView.IsMine)
            {
                if (AlsoHideGroup)
                {
                    nPlayer.HidePlayer();
                }
                else if (nPlayer.GroupIndex != GroupIndex)
                {
                    nPlayer.HidePlayer();
                }
            }
        }
    }

    public void FindAndShowOtherPlayers(bool AlsoShowOtherGroup)
    {
        NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();

        foreach (NetworkPlayer nPlayer in allPlayers)
        {
            if (!nPlayer.thisphotonView.IsMine)
            {
                if (AlsoShowOtherGroup)
                {
                    
                    nPlayer.ShowPlayer();
                    
                }
                else if (nPlayer.GroupIndex == GroupIndex)
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
        HandMeshL.SetActive(true);
        HandMeshR.SetActive(true);
    }

    public void HidePlayerHands()
    {
        HandMeshL.SetActive(false);
        HandMeshR.SetActive(false);
    }

    public void SpawnDrawIndicator()
    {
        if (drawIndicator != null)
        {
            drawIndicator.DestroyIndicator(0);
        }

        Vector3 spawnDirection = (new Vector3(0, Camera.transform.position.y, 0) - Camera.transform.position).normalized;
        Vector3 spawnPosition = Camera.transform.position + (spawnDirection * .4f);
        Quaternion spawnRotation = Quaternion.LookRotation(spawnDirection* -1, Vector3.up);


        GameObject g = Instantiate(DrawIndicatorPrefabs[DrawIndicatorIndex], spawnPosition, spawnRotation);
        drawIndicator = g.GetComponent<DrawIndicator>();

        g.transform.position += (Vector3.down * 0f) * g.transform.lossyScale.y;

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(playerColor);
            //stream.SendNext(canDraw);
            //stream.SendNext(group.GroupID);
        }
        else
        {
            //this.playerColor = (Vector3)stream.ReceiveNext();
            //this.canDraw = (bool)stream.ReceiveNext();
            //this.group.GroupID = (int)stream.ReceiveNext(); 

            if (oldID != group.GroupID)
            {
               // oldID = group.GroupID;
                //SetupForGroup(group.GroupID, this.playerColor.x, this.playerColor.y, this.playerColor.z, 255);
            }
        }
    }
}
