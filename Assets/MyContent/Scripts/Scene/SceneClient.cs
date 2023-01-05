using Photon.Pun;
using UnityEngine;

public class SceneClient : MonoBehaviourPunCallbacks, IPunObservable
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
    private Material SkyboxGame;
    public Material Letters;


    // Start is called before the first frame update
    void Start()
    {
        Skybox_ex = 0f;
        Direct_Intense1 = 0.05f;
        Direct_Intense2 = 0.01f;
        AudioVisFade = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        RenderSettings.skybox.SetFloat("_Exposure", Skybox_ex);
        Direct1.intensity = Direct_Intense1;
        Direct2.intensity = Direct_Intense2;
        AudioVis.transform.localScale = new Vector3(AudioVisFade, AudioVisFade, AudioVisFade);
        Letters.SetFloat("_Metallic",Letter_Metallic);
        Letters.SetFloat("_Glossiness",Letter_Smoothness);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(Skybox_ex);
            stream.SendNext(Direct_Intense1);
            stream.SendNext(Direct_Intense2);
            stream.SendNext(AudioVisFade);
          //  stream.SendNext(Letter_Metallic);
          //  stream.SendNext(Letter_Smoothness);
        }
        else
        {
            this.Skybox_ex = (float)stream.ReceiveNext();
            this.Direct_Intense1 = (float)stream.ReceiveNext();
            this.Direct_Intense2 = (float)stream.ReceiveNext();
            this.AudioVisFade = (float)stream.ReceiveNext();
         //   this.Letter_Metallic = (float)stream.ReceiveNext();
         //   this.Letter_Smoothness = (float)stream.ReceiveNext();
        }
    }
}
