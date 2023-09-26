using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CoinFOV : MonoBehaviourPun
{
    public float viewRange;
    [Range(0, 360)]
    public float viewAngle;
    public int playerLayer;
    public bool isHide;
    Transform coinTr;
    List<GameObject> players = new List<GameObject>();

    void Start()
    {
        if (gameObject.CompareTag("MOMCOIN"))
        {
            viewRange = 50f;
            viewAngle = 180f;
        }
        else if (gameObject.CompareTag("BABYCOIN"))
        {
            viewRange = 20f;
            viewAngle = 240f;
        }

        coinTr = GetComponent<Transform>();
        playerLayer = LayerMask.NameToLayer("PLAYER");
    }


    // 플레이어 추적 가능한 상태인지 판단하는 메소드
    public bool isFindPlayer()
    {
        bool isFind = false;

        // 설정된 반경만큼 OverlapSphere 메소드를 활용하여 플레이어 탐지
        Collider[] colls = Physics.OverlapSphere(coinTr.position, viewRange, 1 << playerLayer);

        if (colls.Length >= 1)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dir = (colls[i].transform.position - coinTr.position).normalized;
                // 적 시야각에 플레이어가 존재하는지 판단
                // 플레이어를 향하는 방향과 정면의 각도가 viewangle 의 반보다 작다면
                if (Vector3.Angle(coinTr.forward, dir) < viewAngle * 0.5f)
                {
                    isFind = true;
                }
            }
        }
        return isFind;
    }

    // 플레이어 보이는 상태인지 판단하는 메소드
    public bool isHidePlayer(GameObject player)
    {
        isHide = true;

        RaycastHit hit;
        // 플레이어를 향하는 방향
        Vector3 dir = (player.transform.position - coinTr.position).normalized;
        // 코인의 위치에서 플레이어방향으로 viewrange 길이만큼 레이캐스트를 쏴서 
        if (Physics.Raycast(coinTr.position, dir, out hit, viewRange))
        {
            // 충돌 정보를 받아와서 충돌물체의 태그가 플레이어라면 isHide = false
            if (hit.collider.CompareTag("PLAYER"))
            {
                isHide = false;
                Debug.Log("플레이어가 안보임 : " + isHide);
            }
            else
            {
                isHide = true;
                Debug.Log("플레이어가 안보임 : " + isHide);
            }
        }

        return isHide;
    }

    public Vector3 CirclePoint(float angle)
    {
        angle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
