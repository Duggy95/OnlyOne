using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CatchCoinPortal : MonoBehaviourPun
{
    public Transform[] portalPos;
    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾ ���� �� ������ ��ġ�� �÷��̾� �̵�
        if(other.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            Transform playerTr = other.GetComponent<Transform>();

            int num = Random.Range(0, portalPos.Length);
            int ranNum_X = Random.Range(-3, 3);
            int ranNum_Z = Random.Range(-3, 3);

            Vector3 ranPos = new Vector3(ranNum_X, 3, ranNum_Z);

            playerTr.position = portalPos[num].position + ranPos;
        }
    }

}
