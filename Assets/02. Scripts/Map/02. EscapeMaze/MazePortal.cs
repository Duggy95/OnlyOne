using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MazePortal : MonoBehaviourPun
{
    // ��Ż ���� �� ���� �̷��� ���� ���������� �̵�
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().RandomPos();
        }
    }

}
