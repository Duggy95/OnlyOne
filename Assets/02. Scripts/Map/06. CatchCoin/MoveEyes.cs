using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEyes : MonoBehaviour
{
    public GameObject end; // 이동하는 끝 지점
    public GameObject start; // 이동하는 끝 지점
    /*public float _damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도

    Vector3 receivePos;*/
    Vector3 startPos;  // 시작할 위치 저장
    Vector3 endPos;  // 돌아올 위치 저장

    float fraction = 0;  // 좌우로 움질일 비율 (0 ~ 1)) 사이
    float moveSpeed;  // 이동속도
    int count = 0;


    void Start()
    {
        moveSpeed = 1f;  // 이동속도 랜덤
    }

    void Update()
    {

        // 비율은 프레임 시간 * 이동속도만큼 증가하여 오브젝트가 이동할 속도 저장
        fraction += Time.deltaTime * moveSpeed;

        startPos = start.transform.position;  // 시작 위치는 시작 지점 위치
        endPos = end.transform.position;  // 돌아올 위치는 끝 지점 위치

        if (fraction >= 0 && fraction < 1)  // 비율이 0 이상 1 이하일 때
        {
            if (count % 2 == 0)
            {
                // 위치 = 시작지점부터 끝지점까지 이동속도만큼 움직임
                transform.position = Vector3.Lerp(startPos, endPos, fraction);
            }
            else if (count % 2 != 0)
            {
                transform.position = Vector3.Lerp(endPos, startPos, fraction);
            }
        }
        else if (fraction >= 1)
        {
            fraction = 0;
            count++;
        }

    }
}
