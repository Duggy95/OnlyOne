using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ObRotate : MonoBehaviourPun, IPunObservable
{
    public float x;
    public float y;
    public float z;
    public float damping = 100f; // 수신된 좌표로 이동할 때 사용할 감도

    float startDelay;
    // receiveDelay;
    bool isRotate;

    PhotonView pv;
    new Transform transform;
    Quaternion receiveRot;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        transform = GetComponent<Transform>();
        StartCoroutine(RotateCo());
    }


    void Update()
    {
        if (pv.IsMine && isRotate)
        {
            transform.Rotate(x * Time.deltaTime, y * Time.deltaTime, z * Time.deltaTime, Space.Self);
        }
        else if (!pv.IsMine)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, receiveRot, Time.deltaTime * damping);
        }
    }

    IEnumerator RotateCo()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (gameObject.tag != "STEP")
                startDelay = Random.Range(0f, 2f);  // 딜레이 시간 랜덤

            yield return new WaitForSeconds(startDelay);

            isRotate = true;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 송신
        {
            stream.SendNext(transform.rotation);
        }
        else // 수신 리딩부분
        {
            // 보낸 순서대로 수신
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
