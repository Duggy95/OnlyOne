using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MazeCoin : MonoBehaviourPun
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 코인을 지나가면 Maze2 오브젝트 찾아서 MazeMake 스크립트의 CoinCount 함수 호출과 동시에 게임 오브젝트 삭제
        if(other.gameObject.CompareTag("PLAYER") && other.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().CoinCount(1, gameObject);
        }
    }
}
