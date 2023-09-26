using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MazeCoin : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾ ������ �������� Maze2 ������Ʈ ã�Ƽ� MazeMake ��ũ��Ʈ�� CoinCount �Լ� ȣ��� ���ÿ� ���� ������Ʈ ����
        if(other.gameObject.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().CoinCount(1, gameObject);
        }
    }
}
