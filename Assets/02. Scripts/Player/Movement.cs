using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviourPunCallbacks, IPunObservable
{
    public float rotSpeed = 300f;  // 회전 속도
    public float moveSpeed = 10f;  // 이동속도
    public float oriMoveSpeed = 10f;  // 기본 이동속도
    public float jumpPower = 7f;  // 점프파워
    public float oriJumpPawer = 7f;   // 기본 점프파워
    public float damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도
    public Text playerName;

    float rayLength = 1.5f;  // 플레이어 사방으로 쏘는 레이캐스트 길이
    float h;  // 수평이동변수
    float v;  // 수직이동변수
    float rX; // 마우스 수평이동
    int jumpCount = 0;  // 점프 횟수

    bool isJumping = true;  // 점프 가능한지 판단할 변수
    bool isStep;  // step에 올라탔는지 판단
    bool isBorder;  // 가로막혔는지 판단
    bool isJumpZone;  // jumpzone인지 판단
    bool dashDelay;  // 대쉬딜레이인지 판단

    AudioSource audioSource;
    Rigidbody rb;  // 리지드바디 컴포넌트
    new Transform transform;  // 트랜스폼 컴포넌트
    PhotonView pv;
    GameObject step;  // step 게임오브젝트
    Vector3 distance;  // step과 플레이어 사이 거리변수
    Vector3 receivePos;
    Quaternion receiveRot;

    private void Awake()
    {
        // 트랜스컴포넌트의 정보 가져오기
        rb = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();

        oriMoveSpeed = moveSpeed;
        oriJumpPawer = jumpPower;
        playerName.text = pv.Owner.NickName;

        if (pv.IsMine)
            playerName.color = Color.green;

        else
            playerName.color = Color.red;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(transform.position); // 1
            stream.SendNext(transform.rotation); // 2
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            Jump();
            Move();

            if (step != null)  // step이 있다면 키보드 입력 없으면 계산한 거리만큼 유지
            {
                if (isStep == true && h == 0 && v == 0)
                {
                    transform.position = step.transform.position - distance;
                }
            }
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * damping);
            transform.rotation = Quaternion.Slerp(transform.rotation, receiveRot, Time.deltaTime * damping);

            if (step != null)  // step이 있다면 키보드 입력 없으면 계산한 거리만큼 유지
            {
                if (isStep == true && h == 0 && v == 0)
                {
                    transform.position = step.transform.position - distance;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (pv.IsMine)
            pv.RPC("StopToOb", RpcTarget.All);  // 장애물에 부딪히는 걸 감지하고 움직임을 제한할 함수
    }

    void Move()
    {
        // 각 입력값할당
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        rX = Input.GetAxis("Mouse X");

        // 이동 = z축 수직이동 + x축 수평이동
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        // 앞으로 나아갈 때만 장애물이 있는지 확인 후 이동 제어
        if (v > 0)
        {
            if (!isBorder)
            {
                transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.Self);
            }
        }
        else
        {
            transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.Self);
        }

        // 마우스 왼쪽 버튼을 통해 캐릭터 회전
        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up * rotSpeed * rX * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // 스페이스바를 눌렀을 때
        {
            if (!isJumping && !isJumpZone)   // 점프
            {
                audioSource.PlayOneShot(SoundManager.instance.jumpClip, 0.5f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("점프가능");
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                jumpCount++;
                isJumping = true;
            }

            else if (isJumping && jumpCount == 1) // 대쉬
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("대시가능");
                Vector3 dir = new Vector3(h, 0f, v);  // h, v 축의 입력에 따라 새로운 방향 V3 변수 저장
                rb.AddForce(dir * (jumpPower / 2), ForceMode.Impulse);  // 그 방향으로 힘을 가함
                dashDelay = true;
                jumpCount++;
            }

            else if (!isJumping && isJumpZone) // 파워 점프
            {
                audioSource.PlayOneShot(SoundManager.instance.jumpClip, 0.5f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("파워점프");
                rb.AddForce(Vector3.up * (jumpPower * 2), ForceMode.Impulse);
                isJumping = true;
                jumpCount++;
            }
        }
    }

    // 대쉬를 하고 난 후 다시 움직이기까지 시간 지연
    IEnumerator DashDelay()
    {
        moveSpeed = 0f;
        jumpPower = 0f;

        Debug.Log("대쉬딜레이IN (moveSpeed): " + moveSpeed);
        Debug.Log("대쉬딜레이IN (jumpPower): " + jumpPower);

        yield return new WaitForSeconds(0.5f);

        dashDelay = false;
        moveSpeed = oriMoveSpeed;
        jumpPower = oriJumpPawer;
        Debug.Log("대쉬딜레이OUT (moveSpeed): " + moveSpeed);
        Debug.Log("대쉬딜레이OUT (jumpPower): " + jumpPower);
        isJumping = false;
    }

    [PunRPC]
    // 레이캐스트를 쏴서 OBSTACLE 레이어에 닿으면 이동을 제한하기 위한 함수
    void StopToOb()
    {
        Debug.DrawRay(transform.position, transform.forward * rayLength, Color.red);
        isBorder = (Physics.Raycast(transform.position, transform.forward, rayLength, LayerMask.GetMask("OBSTACLE")));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("GROUND")  // 충돌한 오브젝트의 태그가 땅이거나
            && collision.contacts[0].normal.y > 0.7f)  // 기울기가 45보다 작을 경우
        {
            isJumping = false;                           // 점프 값을 false로 변경
            isJumpZone = false;
            jumpCount = 0;
            if (dashDelay)
            {
                StartCoroutine(DashDelay());
            }
        }

        else if (collision.gameObject.CompareTag("STEP")) // STEP인 경우 step의 포지션과 플레이어 포지션 거리 계산
        {
            isJumping = false;
            isJumpZone = false;
            isStep = true;
            jumpCount = 0;
            step = collision.gameObject;
            distance = step.transform.position - transform.position;
            Debug.Log(step);

            if (dashDelay)
            {
                StartCoroutine(DashDelay());
            }
        }

        else if (collision.gameObject.CompareTag("JUMPZONE"))
        {
            isJumping = false;
            isJumpZone = true;
            isStep = true;
            jumpCount = 0;
            step = collision.gameObject;
            distance = step.transform.position - transform.position;
            if (dashDelay)
            {
                StartCoroutine(DashDelay());
            }
        }

        // 플레이어 밀치기 기능
        /*else if(collision.gameObject.CompareTag("PLAYER"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 dir = collision.contacts[0].normal;
            // 부딪힌 노말 벡터 방향으로 플레이어 밀어냄
            playerRb.AddForce(-dir.normalized * 5, ForceMode.Impulse);
        }*/
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("STEP") ||
            collision.gameObject.CompareTag("JUMPZONE")) // STEP인 경우 step의 포지션과 플레이어 포지션 거리 계산
        {
            step = collision.gameObject;
            distance = step.transform.position - transform.position;
        }

    }

    private void OnCollisionExit(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("STEP") ||
            collision.gameObject.CompareTag("JUMPZONE"))
        {
            isStep = false;
            step = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!pv.IsMine)
            return;

        if (other.CompareTag("BULLET"))
        {
            StartCoroutine(BulletDamage());
        }
    }

    IEnumerator BulletDamage()
    {
        // 스피드 반전 함수
        // pv.RPC("SpeedCtrl", RpcTarget.All);
        moveSpeed = -moveSpeed;

        yield return new WaitForSeconds(3f);

        moveSpeed = oriMoveSpeed;
    }

    /*[PunRPC]
    void SpeedCtrl()
    {
        moveSpeed = -moveSpeed;

        *//*if (pv.IsMine) // 내 클라 플레이어인 경우 다른 클라에 있는 플레이어에도 호출
            pv.RPC("SpeedCtrl", RpcTarget.Others);*//*
    }*/
}
