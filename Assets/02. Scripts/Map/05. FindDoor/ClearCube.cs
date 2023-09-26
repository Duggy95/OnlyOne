using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ClearCube : MonoBehaviourPun
{
    MeshRenderer meshRenderer;
    PhotonView pv;
    Color originColor;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        meshRenderer = GetComponent<MeshRenderer>();
        originColor = meshRenderer.material.color;
    }

    // 플레이어일 때 색을 투명하게 변경, 5초 뒤 기존 색으로 변경
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PLAYER"))
        {
            pv.RPC("Clear", RpcTarget.All);
            StartCoroutine(RestoringColor());
        }
    }

    [PunRPC]
    void Clear()
    {
        meshRenderer.material.color = Color.clear;
    }

    [PunRPC]
    void Origin()
    {
        meshRenderer.material.color = originColor;
    }

    IEnumerator RestoringColor()
    {
        yield return new WaitForSeconds(5f);

        pv.RPC("Origin", RpcTarget.All);
    }

}
