using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MazePortal : MonoBehaviourPun
{
    // 포탈 진입 시 다음 미로의 랜덤 포지션으로 이동
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().RandomPos();
        }
    }

}
