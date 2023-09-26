using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MoveStep : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject end; // 이동하는 끝 지점
    public GameObject start; // 이동하는 끝 지점
    public float damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도

    Vector3 startPos;  // 시작할 위치 저장
    Vector3 endPos;  // 돌아올 위치 저장
    Vector3 receivePos;


    PhotonView pv;
    new Transform transform;  // 트랜스폼 컴포넌트

    float fraction;  // 좌우로 움질일 비율 (0 ~ 1)) 사이
    float moveSpeed;  // 이동속도
    float startDelay;  // 시작할 시간의 딜레이


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(transform.position); // 1
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receivePos = (Vector3)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        transform = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        startPos = start.transform.position;  // 시작 위치는 시작 지점 위치
        endPos = end.transform.position;  // 돌아올 위치는 끝 지점 위치
        startDelay = Random.Range(0f, 1f);  // 딜레이 시간 랜덤
        moveSpeed = Random.Range(0.1f, 0.2f);  // 이동속도 랜덤
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (startDelay > 0)  // 시작딜레이가 0보다 크면
            {
                startDelay -= Time.deltaTime;  // 프레임 시간 간격만큼 빼줌
                return;
            }
            Move();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * damping);
        }
    }
    void Move()
    {
        // 비율은 프레임 시간 * 이동속도만큼 증가하여 오브젝트가 이동할 속도 저장
        fraction += Time.deltaTime * moveSpeed;

        if (fraction >= 0 && fraction <= 1)  // 비율이 0 이상 1 이하일 때
        {
            // 위치 = 시작지점부터 끝지점까지 이동속도만큼 움직임
            transform.position = Vector3.Lerp(startPos, endPos, fraction);
        }
        else if (fraction > 1) // 비율이 1을 넘어가면
        {
            // 시작 지점과 끝 지점을 서로 교차 저장해주고 비율을 다시 0으로 초기화
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
            fraction = 0;
        }

    }
}
