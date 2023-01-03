using System;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Hub;

public class Shader_Control : MonoBehaviourPunCallbacks, IPunObservable
{
    private PhotonView pv;

    public Material Tryout1_Material;
    public Material Tryout2_Material;


    private float Try_Out_12;
    
    private float local_fade12;
    
    private float local_fade34;
    private float local_fade12_34;
    
    private float local_1_threshold;
    private float local_2_threshold;
    private float local_3_threshold;
    private float local_4_threshold;
    
    private float local_deform;
    private float local_deform_time;
    private float local_deform_scale;

    private float local_color_R1;
    private float local_color_G1;
    private float local_color_B1;
    
    private float local_color_R2;
    private float local_color_G2;
    private float local_color_B2;



    // Start is called before the first frame update
    void Start()
    {
        Try_Out_12 = 0;
        EasyController.esconsend.Send_data(1,65);
        EasyController.esconsend.Send_data(2,65);
        EasyController.esconsend.Send_data(3,0);
        EasyController.esconsend.Send_data(4,0);
        EasyController.esconsend.Send_data(5,0);
        EasyController.esconsend.Send_data(6,0);
        EasyController.esconsend.Send_data(7,0);
        EasyController.esconsend.Send_data(8,0);
        EasyController.esconsend.Send_data(9,0);
        EasyController.esconsend.Send_data(10,0);
        EasyController.esconsend.Send_data(11,0);
        EasyController.esconsend.Send_data(12,0);
        EasyController.esconsend.Send_data(13,0);
        EasyController.esconsend.Send_data(14,0);
        EasyController.esconsend.Send_data(15,0);
        EasyController.esconsend.Send_data(16,0);
        EasyController.esconsend.Send_data(17,0);
        EasyController.esconsend.Send_data(18,0);
        local_color_G1 = 0f;
        local_color_G2 = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.photonView.RPC("TryOut1_Avatar_On", RpcTarget.All);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            this.photonView.RPC("TryOut2_Avatar_On", RpcTarget.All);
        }

        //TryOut1
        if (Try_Out_12 == 0) 
        {
            //Control & Sync main fader
            local_fade12 = EasyController.escon.get_state(15, 0f, 1f); 
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Fader_1_2", local_fade12);

            local_fade34 = EasyController.escon.get_state(17, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Fader_3_4", local_fade34);

            local_fade12_34 = EasyController.escon.get_state(16, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Fader_1_2_3_4", local_fade12_34);
       
            local_deform_time = EasyController.escon.get_state(1, 0f, 1f);
            local_deform_scale = EasyController.escon.get_state(2, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_TimeSpeed", local_deform_time);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_NoiseScale", local_deform_scale);

       //Control & Sync state fader
            if (local_fade12_34 == 0)
            {
                if (local_fade12 == 0)
                {
                    local_1_threshold = EasyController.escon.get_state(7, 0f, 2f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_1_Threshold", local_1_threshold);
                }

                if (local_fade12 == 1)
                {
                    local_2_threshold = EasyController.escon.get_state(8, 0f, 0.8f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_Threshold", local_2_threshold);
                }
            }

            if (local_fade12_34 == 1)
            {
                if (local_fade34 == 0)
                {
                    local_3_threshold = EasyController.escon.get_state(11, -2f, 0.5f)*-1;
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_3_Threshold", local_3_threshold);
               
                    local_deform = EasyController.escon.get_state(11, -2f, 0.5f)*-1;
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Threshold_Deform", local_deform);

                    if (local_3_threshold == -0.5f)
                    {
                        local_deform = EasyController.escon.get_state(3, -0.5f, 2f);
                        gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Threshold_Deform", local_deform);
                    } 
                }

                if (local_fade34 == 1)
                {
                    local_4_threshold = EasyController.escon.get_state(12, 0, 0.45f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_4_Threshold", local_4_threshold);
               
                    local_deform = EasyController.escon.get_state(12, -2, -0.45f)*-1;
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Threshold_Deform", local_deform);
                }
            } 
        }
        
        //TryOut2
        if (Try_Out_12 == 1)
        {
            //Control & Sync main fader
            local_fade12 = EasyController.escon.get_state(15, 0f, 1f); 
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_Fader_1_2", local_fade12);

            local_fade34 = EasyController.escon.get_state(17, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_Fader_3_4", local_fade34);

            local_fade12_34 = EasyController.escon.get_state(16, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_Fader_1_2_3_4", local_fade12_34);
       
            local_deform_time = EasyController.escon.get_state(1, 0f, 1f);
            local_deform_scale = EasyController.escon.get_state(2, 0f, 1f);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_TimeSpeed", local_deform_time);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_NoiseScale", local_deform_scale);
            
            if (local_fade12_34 == 0)
            {
                if (local_fade12 == 0)
                {
                    local_1_threshold = EasyController.escon.get_state(7, 2f, 0f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_1_Threshold", local_1_threshold);
                }

                if (local_fade12 == 1)
                {
                    local_2_threshold = EasyController.escon.get_state(8, 0f, 2f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_2_Threshold", local_2_threshold);
                }
            }
            
            if (local_fade12_34 == 1)
            {
                if (local_fade34 == 0)
                {
                    local_deform = EasyController.escon.get_state(11, 0f, 1f);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_2_Threshold_Deform", local_deform);

                }

                if (local_fade34 == 1)
                {
                    local_color_G1 = EasyController.escon.get_state(12, 0f, 1f);
                    if (local_color_G1 == 1)
                    {
                        gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_2_4_Color_1", Color.yellow);
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_2_4_Color_1", Color.red);

                    }
                    local_color_G2 = EasyController.escon.get_state(13, 0f, 1f);

                    if (local_color_G2 == 1)
                    {
                        gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_2_4_Color_2", Color.green);
                    }
                    else
                    {
                        gameObject.GetComponent<Renderer>().sharedMaterial.SetColor("_2_4_Color_2", Color.blue);

                    }
                }
            } 
            
            
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
         stream.SendNext(local_fade12);
         stream.SendNext(local_fade34);
         stream.SendNext(local_fade12_34);
         stream.SendNext(local_1_threshold);
         stream.SendNext(local_2_threshold);
         stream.SendNext(local_deform);
         stream.SendNext(local_3_threshold);
         stream.SendNext(local_4_threshold);
         stream.SendNext(local_deform_time);
         stream.SendNext(local_deform_scale);
         stream.SendNext(local_color_G1);
         stream.SendNext(local_color_G2);

 
        }
        else
        {
         this.local_fade12 = (float)stream.ReceiveNext();
         this.local_fade34 = (float)stream.ReceiveNext();
         this.local_fade12_34 = (float)stream.ReceiveNext();
         this.local_1_threshold = (float)stream.ReceiveNext();
         this.local_2_threshold = (float)stream.ReceiveNext();
         this.local_deform = (float)stream.ReceiveNext();
         this.local_3_threshold = (float)stream.ReceiveNext();
         this.local_4_threshold = (float)stream.ReceiveNext();
         this.local_deform_time = (float)stream.ReceiveNext();
         this.local_deform_scale = (float)stream.ReceiveNext();
         this.local_color_G1 = (float)stream.ReceiveNext();
         this.local_color_G2 = (float)stream.ReceiveNext();

        }
    }
    [PunRPC]
    private void TryOut1_Avatar_On()
    {
        gameObject.GetComponent<Renderer>().material = Tryout1_Material;
        Try_Out_12 = 0f;
    }
    
    [PunRPC]
    private void TryOut2_Avatar_On()
    {
        gameObject.GetComponent<Renderer>().material = Tryout2_Material;
        Try_Out_12 = 1f;
    }
 }