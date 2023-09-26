using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraCtrl : MonoBehaviourPun
{
    public float moveDamping = 15f; // �̵� �ӵ� ���
    public float rotateDamping = 10f; // ȸ�� �ӵ� ���
    public float distance = 20f; // ���� ������ �Ÿ�
    public float height = 15f; // ���� ������ ����
    public float targetOffset = 10f; // ���� ��ǥ�� ������
    public Transform player;

    Transform tr;

    //float startDelay = 1f;

    void Start()
    {
        tr = GetComponent<Transform>();
        player = GameManager.instance.player.transform;
    }

    void LateUpdate()
    {
        Debug.Log("ī�޶� ��ǥ : " + player.GetComponent<PhotonView>().ViewID);

        if (GameManager.instance.isGameover)
            return;

        if (player == null)
            player = GameManager.instance.player.transform;

        var Campos = player.position - (player.forward * distance) + (player.up * height);
        // �̵��ӵ� ��� ����
        // Slerp�� ���鼱�� �����Լ�
        // Slerp(�������, ��������, ���)
        // ȸ���� ���ʹϾ�
        tr.position = Vector3.Slerp(tr.position, Campos, Time.deltaTime * moveDamping);
        tr.rotation = Quaternion.Slerp(tr.rotation, player.rotation, Time.deltaTime * rotateDamping);
        // ������ ��ġ�� ���ԵǸ� ���� �߹ٴ��� ���Ƿ� offset��ŭ ������ ������ ����
        tr.LookAt(player.transform.position + (player.up * targetOffset));
    }
}
