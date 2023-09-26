using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveEyes : MonoBehaviour
{
    public GameObject end; // �̵��ϴ� �� ����
    public GameObject start; // �̵��ϴ� �� ����
    /*public float _damping = 10f; // ���ŵ� ��ǥ�� �̵��� �� ����� ����

    Vector3 receivePos;*/
    Vector3 startPos;  // ������ ��ġ ����
    Vector3 endPos;  // ���ƿ� ��ġ ����

    float fraction = 0;  // �¿�� ������ ���� (0 ~ 1)) ����
    float moveSpeed;  // �̵��ӵ�
    int count = 0;


    void Start()
    {
        moveSpeed = 1f;  // �̵��ӵ� ����
    }

    void Update()
    {

        // ������ ������ �ð� * �̵��ӵ���ŭ �����Ͽ� ������Ʈ�� �̵��� �ӵ� ����
        fraction += Time.deltaTime * moveSpeed;

        startPos = start.transform.position;  // ���� ��ġ�� ���� ���� ��ġ
        endPos = end.transform.position;  // ���ƿ� ��ġ�� �� ���� ��ġ

        if (fraction >= 0 && fraction < 1)  // ������ 0 �̻� 1 ������ ��
        {
            if (count % 2 == 0)
            {
                // ��ġ = ������������ ���������� �̵��ӵ���ŭ ������
                transform.position = Vector3.Lerp(startPos, endPos, fraction);
            }
            else if (count % 2 != 0)
            {
                transform.position = Vector3.Lerp(endPos, startPos, fraction);
            }
        }
        else if (fraction >= 1)
        {
            fraction = 0;
            count++;
        }

    }
}
