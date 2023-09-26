using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CoinFOV : MonoBehaviourPun
{
    public float viewRange;
    [Range(0, 360)]
    public float viewAngle;
    public int playerLayer;
    public bool isHide;
    Transform coinTr;
    List<GameObject> players = new List<GameObject>();

    void Start()
    {
        if (gameObject.CompareTag("MOMCOIN"))
        {
            viewRange = 50f;
            viewAngle = 180f;
        }
        else if (gameObject.CompareTag("BABYCOIN"))
        {
            viewRange = 20f;
            viewAngle = 240f;
        }

        coinTr = GetComponent<Transform>();
        playerLayer = LayerMask.NameToLayer("PLAYER");
    }


    // �÷��̾� ���� ������ �������� �Ǵ��ϴ� �޼ҵ�
    public bool isFindPlayer()
    {
        bool isFind = false;

        // ������ �ݰ游ŭ OverlapSphere �޼ҵ带 Ȱ���Ͽ� �÷��̾� Ž��
        Collider[] colls = Physics.OverlapSphere(coinTr.position, viewRange, 1 << playerLayer);

        if (colls.Length >= 1)
        {
            for (int i = 0; i < colls.Length; i++)
            {
                Vector3 dir = (colls[i].transform.position - coinTr.position).normalized;
                // �� �þ߰��� �÷��̾ �����ϴ��� �Ǵ�
                // �÷��̾ ���ϴ� ����� ������ ������ viewangle �� �ݺ��� �۴ٸ�
                if (Vector3.Angle(coinTr.forward, dir) < viewAngle * 0.5f)
                {
                    isFind = true;
                }
            }
        }
        return isFind;
    }

    // �÷��̾� ���̴� �������� �Ǵ��ϴ� �޼ҵ�
    public bool isHidePlayer(GameObject player)
    {
        isHide = true;

        RaycastHit hit;
        // �÷��̾ ���ϴ� ����
        Vector3 dir = (player.transform.position - coinTr.position).normalized;
        // ������ ��ġ���� �÷��̾�������� viewrange ���̸�ŭ ����ĳ��Ʈ�� ���� 
        if (Physics.Raycast(coinTr.position, dir, out hit, viewRange))
        {
            // �浹 ������ �޾ƿͼ� �浹��ü�� �±װ� �÷��̾��� isHide = false
            if (hit.collider.CompareTag("PLAYER"))
            {
                isHide = false;
                Debug.Log("�÷��̾ �Ⱥ��� : " + isHide);
            }
            else
            {
                isHide = true;
                Debug.Log("�÷��̾ �Ⱥ��� : " + isHide);
            }
        }

        return isHide;
    }

    public Vector3 CirclePoint(float angle)
    {
        angle += transform.eulerAngles.y;

        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
