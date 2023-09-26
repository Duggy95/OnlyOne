using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Cell : MonoBehaviourPun, IPunObservable
{
    public GameObject forwardWall;
    public GameObject backWall;
    public GameObject rightWall;
    public GameObject leftWall;
    public Vector2Int index;
    public bool isForwardWall = true;
    public bool isBackWall = true;
    public bool isRightWall = true;
    public bool isLeftWall = true;

    bool receiveF;
    bool receiveB;
    bool receiveR;
    bool receiveL;

    MeshRenderer[] meshRenderers;
    PhotonView pv;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        ShowWalls();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(isForwardWall); // 1
            stream.SendNext(isBackWall); // 1
            stream.SendNext(isRightWall); // 1
            stream.SendNext(isLeftWall); // 1
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receiveF = (bool)stream.ReceiveNext();
            receiveB = (bool)stream.ReceiveNext();
            receiveR = (bool)stream.ReceiveNext();
            receiveL = (bool)stream.ReceiveNext();
        }
    }

    //[PunRPC]
    public void ShowWalls()  // 각 셀의 벽의 활성화 상태를 보여줄 함수
    {
        if (PhotonNetwork.IsMasterClient)
        {
            forwardWall.SetActive(isForwardWall);
            backWall.SetActive(isBackWall);
            rightWall.SetActive(isRightWall);
            leftWall.SetActive(isLeftWall);
        }

        else
        {
            forwardWall.SetActive(receiveF);
            backWall.SetActive(receiveB);
            rightWall.SetActive(receiveR);
            leftWall.SetActive(receiveL);
        }
    }

    public bool CheckAllWall()  // 네 개의 벽면이 모두 있는지 확인할 함수
    {
        // if (PhotonNetwork.IsMasterClient)
        return isForwardWall && isBackWall && isRightWall && isLeftWall;

        /*else
            return receiveF && receiveB && receiveR && receiveL;*/
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER") && other.gameObject.GetComponent<PhotonView>().IsMine
            && gameObject.transform.parent.name == "Maze1")  // 플레이어가 셀 지나가면 그 땅 색을 노랑으로 바꿈
        {
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                if (meshRenderers[i].CompareTag("GROUND"))
                    meshRenderers[i].material.color = Color.yellow;
            }
        }
    }
}
