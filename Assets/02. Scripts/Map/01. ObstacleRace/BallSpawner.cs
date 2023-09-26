using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSpaner : MonoBehaviour
{
    public GameObject[] ballPrefabs; // �������� �迭
    public Transform throwPos;   // ���� ���� ��ġ

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
        while (GameManager.instance.isGameover == false)    // ������ ������ �ʴ� ����
        {
            Debug.Log(GameManager.instance.isGameover);
            if (GameManager.instance.isGameover == true)   // ������ �������� �Լ� ����
            {
                yield break;
            }

            yield return new WaitForSeconds(0.1f);

            // �������� x, z�� Ű��� y�� ����
            transform.localScale = new Vector3(transform.localScale.x * 1.02f, transform.localScale.y / 1.02f, transform.localScale.z * 1.02f);
            Debug.Log(transform.localScale.x);

            // ���� x ������ 1.2�� �ʰ��Ͽ� Ŀ���� ���
            if (transform.localScale.x > this_x * 1.2)
            {
                // ���� �������� ���� �����Ϸ� ����
                transform.localScale = new Vector3(this_x, this_y, this_z);
                yield return new WaitForSeconds(0.1f);
                // ball ���� ������ ���� �迭 �Ҵ�
                GameObject balls = ballPrefabs[Random.Range(0, ballPrefabs.Length)];
                // ballClone�� �ν��Ͻ�ȭ�� balls �Ҵ�
                GameObject ballClone = Instantiate(balls, throwPos.position, throwPos.rotation);
                // �ν��Ͻ�ȭ�� ��ü�� ������ٵ� �����ͼ� �Ҵ�
                Rigidbody ballClone_rb = ballClone.GetComponent<Rigidbody>();

                // ���� ��
                int speed = 2000;
                // ������ ballClone�� �� ����
                ballClone_rb.AddForce(ballClone.transform.up * speed); 
                // ���� �ð� ���� �ݺ�
                randomTime = Random.Range(1f, 3f);
                yield return new WaitForSeconds(randomTime);
            }
        }
    }
}

