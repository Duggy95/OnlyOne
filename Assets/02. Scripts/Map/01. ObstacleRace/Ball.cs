using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    float bombPower = 1000f;  // ��ź �Ŀ�

    void Start()
    {
        // �����Ǹ� 10�� �� ����
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // �÷��̾�� �ε�����
        if (collision.gameObject.CompareTag("PLAYER"))
        {
            // �з������� ����
            Rigidbody player_rb = collision.gameObject.GetComponent<Rigidbody>();
            if (player_rb != null)
            {
                player_rb.AddExplosionForce(bombPower, player_rb.transform.position, transform.localScale.x, 0f);
            }
        }
    }
}