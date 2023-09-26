using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviourPunCallbacks, IPunObservable
{
    public float rotSpeed = 300f;  // ȸ�� �ӵ�
    public float moveSpeed = 10f;  // �̵��ӵ�
    public float oriMoveSpeed = 10f;  // �⺻ �̵��ӵ�
    public float jumpPower = 7f;  // �����Ŀ�
    public float oriJumpPawer = 7f;   // �⺻ �����Ŀ�
    public float damping = 10f; // ���ŵ� ��ǥ�� �̵��� �� ����� ����
    public Text playerName;

    float rayLength = 1.5f;  // �÷��̾� ������� ��� ����ĳ��Ʈ ����
    float h;  // �����̵�����
    float v;  // �����̵�����
    float rX; // ���콺 �����̵�
    int jumpCount = 0;  // ���� Ƚ��

    bool isJumping = true;  // ���� �������� �Ǵ��� ����
    bool isStep;  // step�� �ö������� �Ǵ�
    bool isBorder;  // ���θ������� �Ǵ�
    bool isJumpZone;  // jumpzone���� �Ǵ�
    bool dashDelay;  // �뽬���������� �Ǵ�

    AudioSource audioSource;
    Rigidbody rb;  // ������ٵ� ������Ʈ
    new Transform transform;  // Ʈ������ ������Ʈ
    PhotonView pv;
    GameObject step;  // step ���ӿ�����Ʈ
    Vector3 distance;  // step�� �÷��̾� ���� �Ÿ�����
    Vector3 receivePos;
    Quaternion receiveRot;

    private void Awake()
    {
        // Ʈ����������Ʈ�� ���� ��������
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
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(transform.position); // 1
            stream.SendNext(transform.rotation); // 2
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
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

            if (step != null)  // step�� �ִٸ� Ű���� �Է� ������ ����� �Ÿ���ŭ ����
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

            if (step != null)  // step�� �ִٸ� Ű���� �Է� ������ ����� �Ÿ���ŭ ����
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
            pv.RPC("StopToOb", RpcTarget.All);  // ��ֹ��� �ε����� �� �����ϰ� �������� ������ �Լ�
    }

    void Move()
    {
        // �� �Է°��Ҵ�
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        rX = Input.GetAxis("Mouse X");

        // �̵� = z�� �����̵� + x�� �����̵�
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        // ������ ���ư� ���� ��ֹ��� �ִ��� Ȯ�� �� �̵� ����
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

        // ���콺 ���� ��ư�� ���� ĳ���� ȸ��
        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up * rotSpeed * rX * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // �����̽��ٸ� ������ ��
        {
            if (!isJumping && !isJumpZone)   // ����
            {
                audioSource.PlayOneShot(SoundManager.instance.jumpClip, 0.5f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("��������");
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                jumpCount++;
                isJumping = true;
            }

            else if (isJumping && jumpCount == 1) // �뽬
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("��ð���");
                Vector3 dir = new Vector3(h, 0f, v);  // h, v ���� �Է¿� ���� ���ο� ���� V3 ���� ����
                rb.AddForce(dir * (jumpPower / 2), ForceMode.Impulse);  // �� �������� ���� ����
                dashDelay = true;
                jumpCount++;
            }

            else if (!isJumping && isJumpZone) // �Ŀ� ����
            {
                audioSource.PlayOneShot(SoundManager.instance.jumpClip, 0.5f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("�Ŀ�����");
                rb.AddForce(Vector3.up * (jumpPower * 2), ForceMode.Impulse);
                isJumping = true;
                jumpCount++;
            }
        }
    }

    // �뽬�� �ϰ� �� �� �ٽ� �����̱���� �ð� ����
    IEnumerator DashDelay()
    {
        moveSpeed = 0f;
        jumpPower = 0f;

        Debug.Log("�뽬������IN (moveSpeed): " + moveSpeed);
        Debug.Log("�뽬������IN (jumpPower): " + jumpPower);

        yield return new WaitForSeconds(0.5f);

        dashDelay = false;
        moveSpeed = oriMoveSpeed;
        jumpPower = oriJumpPawer;
        Debug.Log("�뽬������OUT (moveSpeed): " + moveSpeed);
        Debug.Log("�뽬������OUT (jumpPower): " + jumpPower);
        isJumping = false;
    }

    [PunRPC]
    // ����ĳ��Ʈ�� ���� OBSTACLE ���̾ ������ �̵��� �����ϱ� ���� �Լ�
    void StopToOb()
    {
        Debug.DrawRay(transform.position, transform.forward * rayLength, Color.red);
        isBorder = (Physics.Raycast(transform.position, transform.forward, rayLength, LayerMask.GetMask("OBSTACLE")));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("GROUND")  // �浹�� ������Ʈ�� �±װ� ���̰ų�
            && collision.contacts[0].normal.y > 0.7f)  // ���Ⱑ 45���� ���� ���
        {
            isJumping = false;                           // ���� ���� false�� ����
            isJumpZone = false;
            jumpCount = 0;
            if (dashDelay)
            {
                StartCoroutine(DashDelay());
            }
        }

        else if (collision.gameObject.CompareTag("STEP")) // STEP�� ��� step�� �����ǰ� �÷��̾� ������ �Ÿ� ���
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

        // �÷��̾� ��ġ�� ���
        /*else if(collision.gameObject.CompareTag("PLAYER"))
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 dir = collision.contacts[0].normal;
            // �ε��� �븻 ���� �������� �÷��̾� �о
            playerRb.AddForce(-dir.normalized * 5, ForceMode.Impulse);
        }*/
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!pv.IsMine)
            return;

        if (collision.gameObject.CompareTag("STEP") ||
            collision.gameObject.CompareTag("JUMPZONE")) // STEP�� ��� step�� �����ǰ� �÷��̾� ������ �Ÿ� ���
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
        // ���ǵ� ���� �Լ�
        // pv.RPC("SpeedCtrl", RpcTarget.All);
        moveSpeed = -moveSpeed;

        yield return new WaitForSeconds(3f);

        moveSpeed = oriMoveSpeed;
    }

    /*[PunRPC]
    void SpeedCtrl()
    {
        moveSpeed = -moveSpeed;

        *//*if (pv.IsMine) // �� Ŭ�� �÷��̾��� ��� �ٸ� Ŭ�� �ִ� �÷��̾�� ȣ��
            pv.RPC("SpeedCtrl", RpcTarget.Others);*//*
    }*/
}
