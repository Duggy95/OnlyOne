using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SpawnCoin : MonoBehaviourPun
{
    public GameObject momCoinPrefab;  // momCoinPrefab
    public GameObject babyCoinPrefab;  // babyCoinPrefab

    List<GameObject> momList = new List<GameObject>();
    List<GameObject> babyList = new List<GameObject>();
    PhotonView pv;

    int groundWeith = 80;  // 플랫폼 가로 길이
    int groundHeight = 80;  // 플랫폼 세로 길이
    int groundY = 0;
    int momCount = 5;  // 생성될 개수
    int babyCount = 3;
    // float momRad = 3f;
    // float babyRad = 1.5f;
    float ranX;  // 좌표x
    float ranZ;  // 좌표z
    Vector3 spwanPos;  // 스폰될 위치

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        for (int i = 0; i < momCount; i++)
        {
            // 랜덤한 좌표를 스폰될 오브젝트 크기에서 추출
            ranX = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
            ranZ = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
            spwanPos = new Vector3(ranX, groundY, ranZ);
            // 랜덤한 좌표에 인스턴스
            GameObject _momPrefab = PhotonNetwork.Instantiate("Six/MomCoin", transform.position + spwanPos, transform.rotation);
            // 자식으로 설정
            int id = _momPrefab.GetComponent<PhotonView>().ViewID;
            pv.RPC("MomParent", RpcTarget.All, id);
        }

        for (int i = 0; i < babyCount; i++)
        {
            ranX = Random.Range(-(groundWeith / 4) - 5, (groundHeight / 4) - 5);
            ranZ = Random.Range(-(groundWeith / 4) - 5, (groundHeight / 4) - 5);
            spwanPos = new Vector3(ranX, groundY, ranZ);

            GameObject _babyPrefab = PhotonNetwork.Instantiate("Six/BabyCoin", transform.position + spwanPos, transform.rotation);
            int id = _babyPrefab.GetComponent<PhotonView>().ViewID;
            pv.RPC("BabyParent", RpcTarget.All, id);
        }
    }

    [PunRPC]
    void MomParent(int id)
    {
        GameObject[] _moms = GameObject.FindGameObjectsWithTag("MOMCOIN");

        for (int i = 0; i < _moms.Length; i++)
        {
            if (_moms[i].GetComponent<PhotonView>().ViewID == id)
            {
                momList.Add(_moms[i]);
                _moms[i].transform.parent = transform;
            }
        }
    }

    [PunRPC]
    void BabyParent(int id)
    {
        GameObject[] _babys = GameObject.FindGameObjectsWithTag("BABYCOIN");

        for (int i = 0; i < _babys.Length; i++)
        {
            if (_babys[i].GetComponent<PhotonView>().ViewID == id)
            {
                babyList.Add(_babys[i]);
                _babys[i].transform.parent = transform;
            }
        }
    }

    public void ChangeSpawnPos(GameObject Baby)
    {
        int id = Baby.GetComponent<PhotonView>().ViewID;
        pv.RPC("DestroyRPC", RpcTarget.MasterClient, id);
        Debug.Log("BabyCoin 삭제함수 호출");

        StartCoroutine(ReSpawn());
    }

    IEnumerator ReSpawn()
    {
        yield return new WaitForSeconds(10);

        // 위치 랜덤으로 변경
        // 랜덤한 좌표를 만들 함수 호출하여 좌표 받아옴
        // 생성
        ranX = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
        ranZ = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
        spwanPos = new Vector3(ranX, groundY, ranZ);

        pv.RPC("SpawnRPC", RpcTarget.MasterClient, spwanPos);
        Debug.Log("코인 재생성");
    }

    [PunRPC]
    void DestroyRPC(int i)
    {
        for (int j = 0; j < babyList.Count; j++)
        {
            if (i == babyList[j].GetComponent<PhotonView>().ViewID)
            {
                PhotonNetwork.Destroy(babyList[j].gameObject);
                babyList.RemoveAt(j);
                Debug.Log("BabyCoin 삭제");
            }
        }
    }

    [PunRPC]
    void SpawnRPC(Vector3 pos)
    {
        GameObject _babyPrefab = PhotonNetwork.Instantiate("Six/BabyCoin", transform.position + pos, transform.rotation);
        int id = _babyPrefab.GetComponent<PhotonView>().ViewID;
        pv.RPC("BabyParent", RpcTarget.All, id);
    }
}
