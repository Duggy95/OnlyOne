using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ChangeGroundColor : MonoBehaviourPun
{
    MeshRenderer meshRenderer;
    PhotonView pv;
    public Color[] randomColor = { Color.blue, Color.green, Color.red, Color.yellow };  // ���� ���� ����

    //RandomColorGround randomColorGround;
    int randomNum;

    void Start()
    {
        //randomColorGround = GameObject.Find("Map_Three").GetComponent<RandomColorGround>();
        pv = GetComponent<PhotonView>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    [PunRPC]
    void ChangeColor(int num)
    {
        meshRenderer.material.color = randomColor[num];
        Debug.Log("��" + randomColor[num]);
    }

    // ���콺 ��Ŭ�� �� �÷��̾ �ִ� Ÿ���� ���� �����ϰ� ����
    private void OnCollisionStay(Collision collision)
    {
        if (Input.GetMouseButtonDown(1) && collision.gameObject.CompareTag("PLAYER"))
        {
            randomNum = Random.Range(0, randomColor.Length);
            pv.RPC("ChangeColor", RpcTarget.All, randomNum);
        }
    }
}
