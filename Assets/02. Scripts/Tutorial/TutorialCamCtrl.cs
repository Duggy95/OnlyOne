using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCamCtrl : MonoBehaviour
{
    public float moveDamping = 15f; // �̵� �ӵ� ���
    public float rotateDamping = 10f; // ȸ�� �ӵ� ���
    public float distance = 20f; // ���� ������ �Ÿ�
    public float height = 20f; // ���� ������ ����
    public float targetOffset = 10f; // ���� ��ǥ�� ������
    public Transform player;

    Transform tr;

    void Start()
    {
        tr = GetComponent<Transform>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
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
