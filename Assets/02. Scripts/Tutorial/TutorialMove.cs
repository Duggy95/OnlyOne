using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMove : MonoBehaviour
{
    float rotSpeed = 300f;
    float moveSpeed = 10f;
    float oriMoveSpeed = 10f;
    float jumpPower = 7f;
    float oriJumpPawer = 7f;
    float h;
    float v;
    float rX;
    int jumpCount = 0;
    bool dashDelay;
    bool isJumping = true;

    Rigidbody rb;
    new Transform transform;
    AudioSource audioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        transform = GetComponent<Transform>();
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        Jump();
        Move();
    }

    void Move()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        rX = Input.GetAxis("Mouse X");

        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);

        transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime, Space.Self);

        if (Input.GetMouseButton(0))
        {
            transform.Rotate(Vector3.up * rotSpeed * rX * Time.deltaTime);
        }
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isJumping)
            {
                audioSource.PlayOneShot(SoundManager.instance.jumpClip, 0.5f);

                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("점프가능");
                rb.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                jumpCount++;
                isJumping = true;
            }

            else if (isJumping && jumpCount == 1)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                Debug.Log("대시가능");
                Vector3 dir = new Vector3(h, 0f, v);
                rb.AddForce(dir * (jumpPower / 2), ForceMode.Impulse);
                dashDelay = true;
                jumpCount++;
            }
        }
    }

    IEnumerator DashDelay()
    {
        moveSpeed = 0f;
        jumpPower = 0f;
        Debug.Log("대쉬딜레이IN (moveSpeed): " + moveSpeed);
        Debug.Log("대쉬딜레이IN (jumpPower): " + jumpPower);

        yield return new WaitForSeconds(1f);

        dashDelay = false;
        moveSpeed = oriMoveSpeed;
        jumpPower = oriJumpPawer;
        Debug.Log("대쉬딜레이OUT (moveSpeed): " + moveSpeed);
        Debug.Log("대쉬딜레이OUT (jumpPower): " + jumpPower);
        isJumping = false;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("GROUND"))
        {
            isJumping = false;
            jumpCount = 0;
            if (dashDelay)
            {
                StartCoroutine(DashDelay());
            }
        }
    }
}
