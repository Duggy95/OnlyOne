using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Shooter : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject bulletPrefab; // 발사할 오브젝트
    // 발사 오브젝트
    public List<Transform> shootPos = new List<Transform>();
    public float damping = 10f; // 수신된 좌표로 이동할 때 사용할 감도


    PhotonView pv;
    new Transform transform;
    float rotSpeed = 120;  // 회전 속도
    float shootSpeed = 15; // 발사 속도
    float destroyTime;  // 생성 후 삭제될 시간
    bool isRotate;  // 회전 가능 여부
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

                yield return new WaitForSeconds(2);  // 2초마다 랜덤한 상태로 이전
            }
        }
    }

    // 상태에 따라 각 변수 설정, 1초 뒤 발사 함수 호출
    [PunRPC]
    void Action(int stateNum)
    {
        switch (stateNum)
        {
            case 0:  // 부동 상태
                isRotate = false;
                destroyTime = 1f;
                //pv.RPC("Shoot", RpcTarget.All);
                break;

            case 1:  // 회전 상태
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
            // 각 발사 오브젝트의 위치에 생성
            GameObject _bullet = Instantiate(bulletPrefab, shootPos[i].position, shootPos[i].rotation);
            // 발사체의 방향을 발사 오브젝트의 forward에 일치
            _bullet.transform.up = shootPos[i].forward;
            _bullet.transform.parent = shootPos[i].transform; // 자식오브젝트로
            Rigidbody _bullet_Rb = _bullet.GetComponent<Rigidbody>();
            _bullet_Rb.velocity = _bullet.transform.up * shootSpeed;

            Destroy(_bullet, destroyTime);
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
