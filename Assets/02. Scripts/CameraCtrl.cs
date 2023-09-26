using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraCtrl : MonoBehaviourPun
{
    public float moveDamping = 15f; // 이동 속도 계수
    public float rotateDamping = 10f; // 회전 속도 계수
    public float distance = 20f; // 추적 대상과의 거리
    public float height = 15f; // 추적 대상과의 높이
    public float targetOffset = 10f; // 추적 좌표의 오프셋
    public Transform player;

    Transform tr;

    //float startDelay = 1f;

    void Start()
    {
        tr = GetComponent<Transform>();
        player = GameManager.instance.player.transform;
    }

    void LateUpdate()
    {
        Debug.Log("카메라 목표 : " + player.GetComponent<PhotonView>().ViewID);

        if (GameManager.instance.isGameover)
            return;

        if (player == null)
            player = GameManager.instance.player.transform;

        var Campos = player.position - (player.forward * distance) + (player.up * height);
        // 이동속도 계수 적용
        // Slerp는 구면선형 보간함수
        // Slerp(출발지점, 도착지점, 계수)
        // 회전은 쿼터니언
        tr.position = Vector3.Slerp(tr.position, Campos, Time.deltaTime * moveDamping);
        tr.rotation = Quaternion.Slerp(tr.rotation, player.rotation, Time.deltaTime * rotateDamping);
        // 포지션 위치를 보게되면 모델의 발바닥을 보므로 offset만큼 위쪽을 보도록 조정
        tr.LookAt(player.transform.position + (player.up * targetOffset));
    }
}
