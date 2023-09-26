using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushZone : MonoBehaviour
{
    MeshRenderer meshRenderer;
    PhotonView pv;
    float pushPower = 0; // �о ��
    float delayTime;  // ������ �ð�

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        pv = GetComponent<PhotonView>();
        delayTime = Random.Range(0f, 2f);  // 0�ʺ��� 2�ʱ���
    }

    private void OnEnable()
    {
        // ������ �Ǹ� �Ʒ� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(DesidePushPower());
    }

    // �ð��� ���� �о�� ���� �ٸ��� �ϱ� ���� �ڷ�ƾ �Լ�
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
        // �÷��̾�� �ε��� �� �÷��̾ �о� ������Ʈ�� �÷��̾���� ù ������ �븻 �������� �о
        if (collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            ContactPoint contact = collision.contacts[0];

            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(contact.normal * -pushPower, ForceMode.Impulse);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // �÷��̾ �з��� �� Maze2 ������Ʈ ã�Ƽ� MazeMake ��ũ��Ʈ�� PushZoneMove�Լ� ȣ��
        if (collision.gameObject.CompareTag("PLAYER") && collision.gameObject.GetComponent<PhotonView>().IsMine)
        {
            GameObject.Find("Maze2").GetComponent<MazeMake>().PushZoneMove(gameObject);
            //Destroy(this.gameObject);
        }
    }

}
