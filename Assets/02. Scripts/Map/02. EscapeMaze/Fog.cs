using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fog : MonoBehaviourPun
{

    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.FindGameObjectWithTag("MAKEMAP").GetComponentInChildren<MazeMake>().FogAttive();
        }
    }
}
