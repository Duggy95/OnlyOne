using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BombSpawner : MonoBehaviourPun, IPunObservable
{
    public GameObject bombPrefabs;  // ��ź������
    public Transform throwPos;   // ������ ��ġ
    public float damping = 10f;

    new Transform transform;
    Rigidbody rb;
    PhotonView pv;
    Vector3 receiveScail;
    //Vector3 receivePos;

    float this_x;
    float this_y;
    float this_z;
    float randomTime = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        transform = GetComponent<Transform>();
    }

    void Start()
    {
        this_x = transform.localScale.x;
        this_z = transform.localScale.z;
        this_y = transform.localScale.y;

        StartCoroutine(ScaleChange());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // �۽�
        {
            stream.SendNext(transform.localScale); // 1
        }
        else // ���� �����κ�
        {
            // ���� ������� ����
            receiveScail = (Vector3)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (pv.IsMine)
            return;
        else
            transform.localScale = receiveScail;
    }

    IEnumerator ScaleChange()
    {
        if (!PhotonNetwork.IsMasterClient)
            yield break;

        yield return new WaitForSeconds(3f);

        while (GameManager.instance != null && GameManager.instance.isGameover == false)  // �������ᰡ �ƴ� ���� ���ѷ���
        {
            if (GameManager.instance.isGameover == true)
            {
                yield break;
            }

            // �������� x, z�� Ű��� y�� ����
            /* transform.localScale = new Vector3(transform.localScale.x + this_x * (0.02f),
                 transform.localScale.y + this_y / (0.02f), transform.localScale.z + this_z * (0.02f));*/
            /*transform.localScale = new Vector3(transform.localScale.x * 1.02f, transform.localScale.y / 1.02f, transform.localScale.z * 1.02f);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);*/
            // pv.RPC("Scale", RpcTarget.All);
            transform.localScale = new Vector3(transform.localScale.x * 1.02f,
            transform.localScale.y / 1.02f, transform.localScale.z * 1.02f);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            yield return new WaitForSeconds(0.1f);

            // ���� x ������ 1.5�� �ʰ��Ͽ� Ŀ���� ���
            if (transform.localScale.x > this_x * 1.5f)
            {
                // ���� ũ��� �ǵ�����
                transform.localScale = new Vector3(this_x, this_y, this_z);
                //pv.RPC("Origin", RpcTarget.All);

                yield return new WaitForSeconds(0.1f);

                int speed = Random.Range(500, 700);

                pv.RPC("Shoot", RpcTarget.All, speed);

                randomTime = Random.Range(0.5f, 1f);

                yield return new WaitForSeconds(randomTime);
            }
        }

    }
    /* [PunRPC]
     void Scale()
     {
         transform.localScale = new Vector3(transform.localScale.x * 1.02f, 
             transform.localScale.y / 1.02f, transform.localScale.z * 1.02f);
         rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
         // Debug.Log("scale" + transform.localScale.x);
     }*/

    /*[PunRPC]
    void Origin()
    {
        transform.localScale = new Vector3(this_x, this_y, this_z);
        // Debug.Log("origin" + transform.localScale.x);
    }*/

    [PunRPC]
    void Shoot(int speed)
    {
        // ������ ��ź ������ٵ� ������
        GameObject bombClone = Instantiate(bombPrefabs, throwPos.position, throwPos.rotation);
        Rigidbody bombClone_rb = bombClone.GetComponent<Rigidbody>();

        // ��ź �� �������� ���� ����
        bombClone_rb.AddForce(bombClone.transform.up * speed);
        // Debug.Log("shoot");
    }
}
