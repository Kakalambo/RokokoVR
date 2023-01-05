using Photon.Pun;
using UnityEngine;

public class SceneControl : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView pv;
    private float Skybox_ex;

    public Light Direct1;
    public Light Direct2;
    
    public GameObject AudioVis;
    
    private float Direct_Intense2;
    private float Direct_Intense1;
    private float AudioVisFade;
    private float Letter_Metallic;
    private float Letter_Smoothness;
    
    public Material SkyboxMaterial1;
    public Material SkyboxMaterial2;
    public Material Letters;

    
    
    // Start is called before the first frame update
    void Start()
    {
       EasyController.esconsend.Send_data(3,0); 
       EasyController.esconsend.Send_data(4,0);
       EasyController.esconsend.Send_data(5,0);
       EasyController.esconsend.Send_data(6,0);
    }

    // Update is called once per frame
    void Update()
    {
        Skybox_ex = EasyController.escon.get_state(18, 0f, 0.35f);
        Direct_Intense1 = EasyController.escon.get_state(0, 0.05f, 0.9f);
        Direct_Intense2 = EasyController.escon.get_state(10, 0.01f, 0.7f);
        AudioVisFade = EasyController.escon.get_state(5, 0f, 1.1f);
        Letter_Metallic = EasyController.escon.get_state(0, 0f, 0.9f);
        Letter_Smoothness = EasyController.escon.get_state(0, 0f, 0.7f);
        
        RenderSettings.skybox.SetFloat("_Exposure", Skybox_ex);
        Direct1.intensity = Direct_Intense1;
        Direct2.intensity = Direct_Intense2;
        AudioVis.transform.localScale = new Vector3(AudioVisFade, AudioVisFade, AudioVisFade);
        Letters.SetFloat("_Metallic",Letter_Metallic);
        Letters.SetFloat("_Glossiness",Letter_Smoothness);

        if (Input.GetKeyDown(KeyCode.Y))
        {
            this.photonView.RPC("Skybox_Day", RpcTarget.All);

        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            this.photonView.RPC("Skybox_Night", RpcTarget.All);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Skybox_ex);
            stream.SendNext(Direct_Intense1);
            stream.SendNext(Direct_Intense2);
            stream.SendNext(AudioVisFade);
           // stream.SendNext(Letter_Metallic);
           // stream.SendNext(Letter_Smoothness);
        }
        else
        {
            this.Skybox_ex = (float)stream.ReceiveNext();
            this.Direct_Intense1 = (float)stream.ReceiveNext();
            this.Direct_Intense2 = (float)stream.ReceiveNext();
            this.AudioVisFade = (float)stream.ReceiveNext();
          //  this.Letter_Metallic = (float)stream.ReceiveNext();
           // this.Letter_Smoothness = (float)stream.ReceiveNext();
        }
    }
    
    [PunRPC]
    private void Skybox_Day()
    {
        RenderSettings.skybox = SkyboxMaterial1;
    }
    
    [PunRPC]
    private void Skybox_Night()
    {
        RenderSettings.skybox = SkyboxMaterial2;
    }
}
