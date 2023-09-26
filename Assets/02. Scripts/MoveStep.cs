using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MoveStep : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject end; // �̵��ϴ� �� ����
    public GameObject start; // �̵��ϴ� �� ����
    public float damping = 10f; // ���ŵ� ��ǥ�� �̵��� �� ����� ����

    Vector3 startPos;  // ������ ��ġ ����
    Vector3 endPos;  // ���ƿ� ��ġ ����
    Vector3 receivePos;


    PhotonView pv;
    new Transform transform;  // Ʈ������ ������Ʈ

    float fraction;  // �¿�� ������ ���� (0 ~ 1)) ����
    float moveSpeed;  // �̵��ӵ�
    float startDelay;  // ������ �ð��� ������


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(transform.position); // 1
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
            receivePos = (Vector3)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        transform = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        startPos = start.transform.position;  // ���� ��ġ�� ���� ���� ��ġ
        endPos = end.transform.position;  // ���ƿ� ��ġ�� �� ���� ��ġ
        startDelay = Random.Range(0f, 1f);  // ������ �ð� ����
        moveSpeed = Random.Range(0.1f, 0.2f);  // �̵��ӵ� ����
    }

    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (startDelay > 0)  // ���۵����̰� 0���� ũ��
            {
                startDelay -= Time.deltaTime;  // ������ �ð� ���ݸ�ŭ ����
                return;
            }
            Move();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * damping);
        }
    }
    void Move()
    {
        // ������ ������ �ð� * �̵��ӵ���ŭ �����Ͽ� ������Ʈ�� �̵��� �ӵ� ����
        fraction += Time.deltaTime * moveSpeed;

        if (fraction >= 0 && fraction <= 1)  // ������ 0 �̻� 1 ������ ��
        {
            // ��ġ = ������������ ���������� �̵��ӵ���ŭ ������
            transform.position = Vector3.Lerp(startPos, endPos, fraction);
        }
        else if (fraction > 1) // ������ 1�� �Ѿ��
        {
            // ���� ������ �� ������ ���� ���� �������ְ� ������ �ٽ� 0���� �ʱ�ȭ
            Vector3 temp = startPos;
            startPos = endPos;
            endPos = temp;
            fraction = 0;
        }

    }
}
