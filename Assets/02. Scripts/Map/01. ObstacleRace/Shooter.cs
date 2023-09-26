using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Shooter : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject bulletPrefab; // �߻��� ������Ʈ
    // �߻� ������Ʈ
    public List<Transform> shootPos = new List<Transform>();
    public float damping = 10f; // ���ŵ� ��ǥ�� �̵��� �� ����� ����


    PhotonView pv;
    new Transform transform;
    float rotSpeed = 120;  // ȸ�� �ӵ�
    float shootSpeed = 15; // �߻� �ӵ�
    float destroyTime;  // ���� �� ������ �ð�
    bool isRotate;  // ȸ�� ���� ����
    Quaternion receiveRot;

    private void Awake()
    {
        transform = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();
        GetComponentsInChildren<Transform>(shootPos);
        shootPos.RemoveAt(0);
        Debug.Log(shootPos.Count);
        StartCoroutine(CheckState());
    }

    void Update()
    {
        if (pv.IsMine && isRotate)
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0, Space.Self);

        else if (!pv.IsMine)
            transform.rotation = Quaternion.Slerp(transform.rotation, receiveRot, Time.deltaTime * damping);
    }

    IEnumerator CheckState()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            float startDelay = Random.Range(1, 3);
            yield return new WaitForSeconds(startDelay);

            while (GameManager.instance.isGameover == false)
            {
                yield return null;

                int stateNum = Random.Range(0, 2);

                //StartCoroutine(Action(stateNum));
                //pv.RPC("Action", RpcTarget.All, stateNum);
                pv.RPC("Action", RpcTarget.AllBuffered, stateNum);
                Debug.Log(stateNum);

                yield return new WaitForSeconds(2);  // 2�ʸ��� ������ ���·� ����
            }
        }
    }

    // ���¿� ���� �� ���� ����, 1�� �� �߻� �Լ� ȣ��
    [PunRPC]
    void Action(int stateNum)
    {
        switch (stateNum)
        {
            case 0:  // �ε� ����
                isRotate = false;
                destroyTime = 1f;
                //pv.RPC("Shoot", RpcTarget.All);
                break;

            case 1:  // ȸ�� ����
                isRotate = true;
                destroyTime = 2f;
                // pv.RPC("Shoot", RpcTarget.All);
                break;
        }
        Shoot();
        //pv.RPC("Shoot", RpcTarget.OthersBuffered);
    }

    //[PunRPC]
    void Shoot()
    {
        for (int i = 0; i < shootPos.Count; i++)
        {
            // �� �߻� ������Ʈ�� ��ġ�� ����
            GameObject _bullet = Instantiate(bulletPrefab, shootPos[i].position, shootPos[i].rotation);
            // �߻�ü�� ������ �߻� ������Ʈ�� forward�� ��ġ
            _bullet.transform.up = shootPos[i].forward;
            _bullet.transform.parent = shootPos[i].transform; // �ڽĿ�����Ʈ��
            Rigidbody _bullet_Rb = _bullet.GetComponent<Rigidbody>();
            _bullet_Rb.velocity = _bullet.transform.up * shootSpeed;

            Destroy(_bullet, destroyTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(transform.rotation);
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
