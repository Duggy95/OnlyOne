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

    int groundWeith = 80;  // �÷��� ���� ����
    int groundHeight = 80;  // �÷��� ���� ����
    int groundY = 0;
    int momCount = 5;  // ������ ����
    int babyCount = 3;
    // float momRad = 3f;
    // float babyRad = 1.5f;
    float ranX;  // ��ǥx
    float ranZ;  // ��ǥz
    Vector3 spwanPos;  // ������ ��ġ

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
            // ������ ��ǥ�� ������ ������Ʈ ũ�⿡�� ����
            ranX = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
            ranZ = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
            spwanPos = new Vector3(ranX, groundY, ranZ);
            // ������ ��ǥ�� �ν��Ͻ�
            GameObject _momPrefab = PhotonNetwork.Instantiate("Six/MomCoin", transform.position + spwanPos, transform.rotation);
            // �ڽ����� ����
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
        Debug.Log("BabyCoin �����Լ� ȣ��");

        StartCoroutine(ReSpawn());
    }

    IEnumerator ReSpawn()
    {
        yield return new WaitForSeconds(10);

        // ��ġ �������� ����
        // ������ ��ǥ�� ���� �Լ� ȣ���Ͽ� ��ǥ �޾ƿ�
        // ����
        ranX = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
        ranZ = Random.Range(-(groundWeith / 2) - 5, (groundHeight / 2) - 5);
        spwanPos = new Vector3(ranX, groundY, ranZ);

        pv.RPC("SpawnRPC", RpcTarget.MasterClient, spwanPos);
        Debug.Log("���� �����");
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
                Debug.Log("BabyCoin ����");
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
