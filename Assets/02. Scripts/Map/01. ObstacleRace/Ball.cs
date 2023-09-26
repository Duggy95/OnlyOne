using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    float bombPower = 1000f;  // 폭탄 파워

    void Start()
    {
        // 생성되면 10초 뒤 삭제
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어와 부딪히면
        if (collision.gameObject.CompareTag("PLAYER"))
        {
            // 밀려나도록 구현
            Rigidbody player_rb = collision.gameObject.GetComponent<Rigidbody>();
            if (player_rb != null)
            {
                player_rb.AddExplosionForce(bombPower, player_rb.transform.position, transform.localScale.x, 0f);
            }
        }
    }
}