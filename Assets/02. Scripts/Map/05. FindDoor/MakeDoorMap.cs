using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class MakeDoorMap : MonoBehaviourPun
{
    public GameObject doorPrefab;  // �� ������
    public GameObject ground;  // �÷���
    public GameObject endGround;  // �������
    public GameObject savePoint;  // ���̺�����Ʈ
    public GameObject clearCube;  // ���� ť��
    public GameObject doorPos;

    PhotonView pv;
    GameObject _ground;  // �����Ǵ� ���� �ٴ��÷���
    List<BoxCollider> cubeBox = new List<BoxCollider>();  // ť���� Ʈ���� ��� ���� �ڽ��ݶ��̴� ����Ʈ
    List<GameObject> doorList_l = new List<GameObject>(); // �ݺ��Ǿ� �����Ǵ� ���� ����Ʈ ���� ����
    //List<GameObject> doorList_m = new List<GameObject>(); // �ݺ��Ǿ� �����Ǵ� ���� ����Ʈ ���� ����


    int doorPrefabCount = 10;  // ������ ���� �� ��
    int doorCount;  // ���� ���� ��
    int fakeDoorCount; // ��¥�� ��
    int makeCount;  // �ݺ��� Ƚ��
    int count = 0;  // �ݺ��� Ƚ��
    int cubeWidth = 2;  // ť�갡 ������ ���� ��
    int cubeHeight = 3;  // ť�갡 ������ ���� ��

    float groundSizeX; // �÷��� �ϳ��� x ������
    float groundSizeZ = 100f; // �÷��� �ϳ��� y������
    float clearCubeX = 3;  // ť�� ������
    float groundDistance;  // �÷��������� �Ÿ�
    float doorSizeX = 5;  // �� ������

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    void Start()
    {
        groundDistance = clearCubeX * clearCubeX * cubeHeight + clearCubeX * 3;   // �÷������� �Ÿ��� ť���� ������ * ť���� �� ��  * 3
        groundSizeX = doorPrefabCount * doorSizeX;   // �÷����� x������� �����Ǵ� �� �� * ������
        makeCount = doorPrefabCount / 2; // �ݺ��� Ƚ�� (���� ����)
        transform.position = new Vector3(-groundSizeX / 2f, 0, 0); // ó�� ���� ������ ��ġ ����
        doorCount = doorPrefabCount / 2;   // ���� ���� ���� ������ ���� �� / 2

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(MakeMap());  // ���� ������ �Լ� ȣ��
    }

    IEnumerator MakeMap()
    {
        for (int i = 0; i < makeCount; i++)
        {
            pv.RPC("SetGround", RpcTarget.All, i);
            yield return new WaitForSeconds(0.1f);
            /*groundSizeX = doorPrefabCount * doorSizeX;   // �÷����� x������� �����Ǵ� �� �� * ������
            fakeDoorCount = doorPrefabCount - doorCount;  // ��¥ ���� �� �� - ���� ��

            _ground = Instantiate(ground, transform);  // �÷��� ����
            _ground.transform.position += new Vector3((groundSizeX / 2f) + (doorSizeX / 2 * i), 0, (groundSizeZ + groundDistance) * count); // ó�� ���� ������ ��ġ ����
            _ground.transform.localScale = new Vector3(groundSizeX, 1, groundSizeZ);  // ������ ����*/

            // ���� ����
            for (int j = 0; j < doorPrefabCount; j++)
            {
                // ���� �Ǵٸ� ���ӿ�����Ʈ�� �ڽ����� �����Ͽ� ��ġ �����ϰ� �ڽ� ����, ����Ʈ �߰�
                doorPos.transform.position = _ground.transform.position + new Vector3((-groundSizeX / 2f) + (doorSizeX * 1.5f) + doorSizeX * j - 5, -1 + 4.5f, (groundSizeZ / 2) * 0.5f);
                GameObject _door = PhotonNetwork.Instantiate("Five/Door", doorPos.transform.position, doorPos.transform.rotation);
                _door.transform.parent = null;
                _door.name = i + "_" + j + "door";
                pv.RPC("AddDoor", RpcTarget.All, _door.GetComponent<PhotonView>().ViewID);
                //doorList_m.Add(_door);
            }

            // pv.RPC("AddDoor", RpcTarget.All);
            yield return new WaitForSeconds(0.1f);

            for (int j = 0; j < fakeDoorCount; j++)    // ��¥ ���� ����ŭ �������� ������ ���� ���� ���
            {
                // �������� ���� ���� ����������Ʈ ���ſ� Ű�׸�ƽ ������ ���ְ� ����Ʈ���� �ش� �ε��� ����
                int fakeDoorNum = Random.Range(0, doorList_l.Count);

                pv.RPC("HingeCtrl", RpcTarget.All, fakeDoorNum);

                /*HingeJoint[] doorHinge = doorList_i[fakeDoorNum].GetComponentsInChildren<HingeJoint>();
                Rigidbody[] doorRb = doorList_i[fakeDoorNum].GetComponentsInChildren<Rigidbody>();
                doorRb[0].isKinematic = true;
                doorRb[1].isKinematic = true;*/
                /*Destroy(doorHinge[0]);
                Destroy(doorHinge[1]);*/
                //doorList_m.RemoveAt(fakeDoorNum);
                yield return new WaitForSeconds(0.1f);
            }

            // ���̺�����Ʈ ��ġ ���� (�� �÷����� ó���� ��)
            for (int j = -1; j < 2; j += 2)
            {
                pv.RPC("MakeSave", RpcTarget.All, j);
                /*GameObject _savePoint_i = Instantiate(savePoint, transform);
                _savePoint_i.transform.position = _ground.transform.position + new Vector3(0, 0, (groundSizeZ / 2) * 0.7f * j);*/
            }

            // ť�긦 �����Ͽ� ��ġ�� �������ְ� �� �� �ϳ��� Ʈ���� true�� ����, �ݺ������� ����Ʈ �ʱ�ȭ
            for (int x = 0; x < cubeHeight; x++)
            {
                for (int y = 0; y < cubeWidth; y++)
                {
                    GameObject _cube = PhotonNetwork.Instantiate("Five/ClearCube", _ground.transform.position
                        + new Vector3(-clearCubeX * clearCubeX / 2 + ((clearCubeX * clearCubeX) * y), 0, (groundSizeZ / 2) + (clearCubeX * 2) + ((clearCubeX * clearCubeX) * x)), Quaternion.identity);
                    _cube.name = x + "," + y;
                    /*_cube.transform.position = _ground.transform.position
                        + new Vector3(-clearCubeX * clearCubeX / 2 + ((clearCubeX * clearCubeX) * y), 0, (groundSizeZ / 2) + (clearCubeX * 2) + ((clearCubeX * clearCubeX) * x));*/
                    pv.RPC("AddCube", RpcTarget.All, _cube.GetComponent<PhotonView>().ViewID);
                    //cubeBox.Add(_cube.GetComponent<BoxCollider>());
                }

                Debug.Log("CubeList" + cubeBox.Count);

                int cubeNum = Random.Range(0, cubeBox.Count);
                Debug.Log("cubeBox" + cubeBox[cubeNum]);
                pv.RPC("CubeCtrl", RpcTarget.All, cubeNum);
                yield return new WaitForSeconds(0.2f);

                //cubeBox[cubeNum].isTrigger = true;
            }

            pv.RPC("Next", RpcTarget.All);
            yield return new WaitForSeconds(0.1f);

            /*count++;  // �ݺ�Ƚ�� +1
            doorCount--;  // ���� ���� �� -1
            doorPrefabCount--;  // ������ ���� �� -1*/
        }
        //doorList_m.Clear();
        // ��������� ������ ����
        pv.RPC("EndGround", RpcTarget.All);
        //endGround.transform.position = _ground.transform.position + new Vector3(0, 0, groundSizeZ / 2 + groundDistance + clearCubeX * clearCubeX);
    }

    [PunRPC]
    void SetGround(int i)
    {
        groundSizeX = doorPrefabCount * doorSizeX;   // �÷����� x������� �����Ǵ� �� �� * ������
        fakeDoorCount = doorPrefabCount - doorCount;  // ��¥ ���� �� �� - ���� ��

        _ground = Instantiate(ground, transform);  // �÷��� ����
        _ground.transform.position += new Vector3((groundSizeX / 2f) + (doorSizeX / 2 * i), 0, (groundSizeZ + groundDistance) * count); // ó�� ���� ������ ��ġ ����
        _ground.transform.localScale = new Vector3(groundSizeX, 1, groundSizeZ);  // ������ ����
    }

    [PunRPC]
    void AddDoor(int id)
    {
        GameObject[] doors = GameObject.FindGameObjectsWithTag("DOOR");

        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].GetComponent<PhotonView>().ViewID == id)
                doorList_l.Add(doors[i]);
        }

        Debug.Log("doorList : " + doorList_l.Count);
    }

    /*[PunRPC]
    void MakeDoor(int i, int j)
    {
        doorPos.transform.position = _ground.transform.position + new Vector3((-groundSizeX / 2f) + (doorSizeX * 1.5f) + doorSizeX * j, -1, (groundSizeZ / 2) * 0.5f);
        GameObject _door = PhotonNetwork.Instantiate("Five/Door", doorPos.transform.position, doorPos.transform.rotation);
        _door.transform.parent = null;
        _door.name = i + "_" + j + "door";
        doorList_i.Add(_door);
    }*/

    [PunRPC]
    void HingeCtrl(int fake)
    {
        /*for (int i = 0; i < doorList_l.Count; i++)
        {
            if (doorList_l[i].GetComponent<PhotonView>().ViewID == id)
            {*/
        Rigidbody[] doorRb = doorList_l[fake].GetComponentsInChildren<Rigidbody>();

        doorRb[0].isKinematic = true;
        doorRb[1].isKinematic = true;

        doorList_l.RemoveAt(fake);
        /*}
    }*/
        /*HingeJoint[] doorHinge = doorList_i[i].GetComponentsInChildren<HingeJoint>();
        Debug.Log("doorHinge : " + doorHinge.Length);

        *//*doorHinge[0].gameObject.SetActive(false);
        doorHinge[1].gameObject.SetActive(false);*//*
        Destroy(doorHinge[0]);
        Destroy(doorHinge[1]);*/
    }

    [PunRPC]
    void MakeSave(int j)
    {
        GameObject _savePoint_i = Instantiate(savePoint, transform);
        _savePoint_i.transform.position = _ground.transform.position + new Vector3(0, 0, (groundSizeZ / 2) * 0.7f * j);
    }

    [PunRPC]
    void AddCube(int id)
    {
        GameObject[] cubes = GameObject.FindGameObjectsWithTag("STEP");
        for (int i = 0; i < cubes.Length; i++)
        {
            if (cubes[i].GetComponent<PhotonView>().ViewID == id)
                cubeBox.Add(cubes[i].GetComponent<BoxCollider>());
        }
    }

    [PunRPC]
    void CubeCtrl(int i)
    {
        cubeBox[i].isTrigger = true;
        cubeBox.Clear();
    }

    [PunRPC]
    void Next()
    {
        count++;  // �ݺ�Ƚ�� +1
        doorCount--;  // ���� ���� �� -1
        doorPrefabCount--;  // ������ ���� �� -1
        doorList_l.Clear();
    }

    [PunRPC]
    void EndGround()
    {
        endGround.transform.position = _ground.transform.position + new Vector3(0, 0, groundSizeZ / 2 + groundDistance + clearCubeX * clearCubeX);
    }
}


