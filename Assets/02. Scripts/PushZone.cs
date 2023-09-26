using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushZone : MonoBehaviour
{
    MeshRenderer meshRenderer;
    PhotonView pv;
    float pushPower = 0; // 밀어낼 힘
    float delayTime;  // 지연될 시간

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pv = GetComponent<PhotonView>();
        delayTime = Random.Range(0f, 2f);  // 0초부터 2초까지
    }

    private void OnEnable()
    {
        // 생성이 되면 아래 코루틴 함수 호출
        StartCoroutine(DesidePushPower());
    }

    // 시간에 따라 밀어내는 힘을 다르게 하기 위한 코루틴 함수
    IEnumerator DesidePushPower()
    {
        yield return new WaitForSeconds(delayTime);

        while(GameManager.instance.isGameover == false)
        {
            meshRenderer.material.color = Color.green; 
            pushPower = 5f;
            yield return new WaitForSeconds(2f);

            meshRenderer.material.color = Color.yellow;
            pushPower = 15f;
            yield return new WaitForSeconds(1.5f);

            meshRenderer.material.color = Color.red;
            pushPower = 25f;
            yield return new WaitForSeconds(1f);

            meshRenderer.material.color = Color.gray;
            pushPower = 0f;
            yield return new WaitForSeconds(1.5f);

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // 플레이어와 부딪힐 때 플레이어를 밀어 오브젝트와 플레이어와의 첫 접점의 노말 방향으로 밀어냄
        if (collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            ContactPoint contact = collision.contacts[0];

            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(contact.normal * -pushPower, ForceMode.Impulse);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // 플레이어가 밀려난 후 Maze2 오브젝트 찾아서 MazeMake 스크립트의 PushZoneMove함수 호출
        if (collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().PushZoneMove(gameObject);
            //Destroy(this.gameObject);
        }
    }

}
