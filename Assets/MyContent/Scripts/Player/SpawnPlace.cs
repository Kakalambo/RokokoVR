using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPlace : MonoBehaviour
{
    public PlayerGroup group;
    public GameObject Marker;
    // Start is called before the first frame update
    void Awake()
    {
        Marker.SetActive(false);
    }
}
