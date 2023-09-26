using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpaner : MonoBehaviour
{
    public GameObject[] ballPrefabs; // 볼프리팹 배열
    public Transform throwPos;   // 공이 나올 위치

    float this_x;
    float this_y;
    float this_z;
    float randomTime = 1f;     

    void Start()
    {
        this_x = transform.localScale.x;
        this_z = transform.localScale.z;
        this_y = transform.localScale.y;
    }

    void OnEnable()
    {
        StartCoroutine(Throw());
    }

    IEnumerator Throw()
    {
        while (GameManager.instance.isGameover == false)    // 게임이 끝나지 않는 동안
        {
            Debug.Log(GameManager.instance.isGameover);
            if (GameManager.instance.isGameover == true)   // 게임이 끝났으면 함수 종료
            {
                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            // 스케일을 x, z는 키우고 y는 줄임
            transform.localScale = new Vector3(transform.localScale.x * 1.02f, transform.localScale.y / 1.02f, transform.localScale.z * 1.02f);
            Debug.Log(transform.localScale.x);

            // 기존 x 값보다 1.2배 초과하여 커졌을 경우
            if (transform.localScale.x > this_x * 1.2)
            {
                // 현재 스케일을 기존 스케일로 변경
                transform.localScale = new Vector3(this_x, this_y, this_z);
                yield return new WaitForSeconds(0.1f);
                // ball 랜덤 생성을 위해 배열 할당
                GameObject balls = ballPrefabs[Random.Range(0, ballPrefabs.Length)];
                // ballClone에 인스턴스화된 balls 할당
                GameObject ballClone = Instantiate(balls, throwPos.position, throwPos.rotation);
                // 인스턴스화된 객체의 리지드바디 가져와서 할당
                Rigidbody ballClone_rb = ballClone.GetComponent<Rigidbody>();

                // 던질 힘
                int speed = 2000;
                // 생성된 ballClone에 힘 가함
                ballClone_rb.AddForce(ballClone.transform.up * speed); 
                // 랜덤 시간 이후 반복
                randomTime = Random.Range(1f, 3f);
                yield return new WaitForSeconds(randomTime);
            }
        }
    }
}

