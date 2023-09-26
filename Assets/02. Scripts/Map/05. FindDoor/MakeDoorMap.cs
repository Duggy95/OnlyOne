using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;

public class MakeDoorMap : MonoBehaviourPun
{
    public GameObject doorPrefab;  // 문 프리팹
    public GameObject ground;  // 플랫폼
    public GameObject endGround;  // 결승지점
    public GameObject savePoint;  // 세이브포인트
    public GameObject clearCube;  // 투명 큐브
    public GameObject doorPos;

    PhotonView pv;
    GameObject _ground;  // 생성되는 맵의 바닥플랫폼
    List<BoxCollider> cubeBox = new List<BoxCollider>();  // 큐브의 트리거 제어를 위한 박스콜라이더 리스트
    List<GameObject> doorList_l = new List<GameObject>(); // 반복되어 생성되는 문의 리스트 각각 저장
    //List<GameObject> doorList_m = new List<GameObject>(); // 반복되어 생성되는 문의 리스트 각각 저장


    int doorPrefabCount = 10;  // 생성될 문의 총 수
    int doorCount;  // 실제 문의 수
    int fakeDoorCount; // 가짜문 수
    int makeCount;  // 반복할 횟수
    int count = 0;  // 반복된 횟수
    int cubeWidth = 2;  // 큐브가 생성될 가로 수
    int cubeHeight = 3;  // 큐브가 생성될 세로 수

    float groundSizeX; // 플랫폼 하나당 x 사이즈
    float groundSizeZ = 100f; // 플랫폼 하나당 y사이즈
    float clearCubeX = 3;  // 큐브 사이즈
    float groundDistance;  // 플랫폼끼리의 거리
    float doorSizeX = 5;  // 문 사이즈

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    void Start()
    {
        groundDistance = clearCubeX * clearCubeX * cubeHeight + clearCubeX * 3;   // 플랫폼과의 거리는 큐브의 사이즈 * 큐브의 열 수  * 3
        groundSizeX = doorPrefabCount * doorSizeX;   // 플랫폼의 x사이즈는 생성되는 문 수 * 사이즈
        makeCount = doorPrefabCount / 2; // 반복할 횟수 (버림 적용)
        transform.position = new Vector3(-groundSizeX / 2f, 0, 0); // 처음 맵이 생성될 위치 조정
        doorCount = doorPrefabCount / 2;   // 실제 문의 수는 생성될 문의 수 / 2

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(MakeMap());  // 맵을 생성할 함수 호출
    }

    IEnumerator MakeMap()
    {
        for (int i = 0; i < makeCount; i++)
        {
            pv.RPC("SetGround", RpcTarget.All, i);
            yield return new WaitForSeconds(0.1f);
            /*groundSizeX = doorPrefabCount * doorSizeX;   // 플랫폼의 x사이즈는 생성되는 문 수 * 사이즈
            fakeDoorCount = doorPrefabCount - doorCount;  // 가짜 문은 총 문 - 실제 문

            _ground = Instantiate(ground, transform);  // 플랫폼 생성
            _ground.transform.position += new Vector3((groundSizeX / 2f) + (doorSizeX / 2 * i), 0, (groundSizeZ + groundDistance) * count); // 처음 맵이 생성될 위치 조정
            _ground.transform.localScale = new Vector3(groundSizeX, 1, groundSizeZ);  // 사이즈 조정*/

            // 문을 생성
            for (int j = 0; j < doorPrefabCount; j++)
            {
                // 문을 또다른 게임오브젝트의 자식으로 생성하여 위치 조정하고 자식 해제, 리스트 추가
                doorPos.transform.position = _ground.transform.position + new Vector3((-groundSizeX / 2f) + (doorSizeX * 1.5f) + doorSizeX * j - 5, -1 + 4.5f, (groundSizeZ / 2) * 0.5f);
                GameObject _door = PhotonNetwork.Instantiate("Five/Door", doorPos.transform.position, doorPos.transform.rotation);
                _door.transform.parent = null;
                _door.name = i + "_" + j + "door";
                pv.RPC("AddDoor", RpcTarget.All, _door.GetComponent<PhotonView>().ViewID);
                //doorList_m.Add(_door);
            }

            // pv.RPC("AddDoor", RpcTarget.All);
            yield return new WaitForSeconds(0.1f);

            for (int j = 0; j < fakeDoorCount; j++)    // 가짜 문의 수만큼 랜덤으로 정해진 문의 리밋 사용
            {
                // 랜덤으로 문을 정해 힌지컴포넌트 제거와 키네마틱 설정을 해주고 리스트에서 해당 인덱스 삭제
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

            // 세이브포인트 위치 조정 (각 플랫폼의 처음과 끝)
            for (int j = -1; j < 2; j += 2)
            {
                pv.RPC("MakeSave", RpcTarget.All, j);
                /*GameObject _savePoint_i = Instantiate(savePoint, transform);
                _savePoint_i.transform.position = _ground.transform.position + new Vector3(0, 0, (groundSizeZ / 2) * 0.7f * j);*/
            }

            // 큐브를 생성하여 위치를 조정해주고 둘 중 하나의 트리거 true로 설정, 반복때마다 리스트 초기화
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

            /*count++;  // 반복횟수 +1
            doorCount--;  // 실제 문의 수 -1
            doorPrefabCount--;  // 생성될 문의 수 -1*/
        }
        //doorList_m.Clear();
        // 결승지점의 포지션 지정
        pv.RPC("EndGround", RpcTarget.All);
        //endGround.transform.position = _ground.transform.position + new Vector3(0, 0, groundSizeZ / 2 + groundDistance + clearCubeX * clearCubeX);
    }

    [PunRPC]
    void SetGround(int i)
    {
        groundSizeX = doorPrefabCount * doorSizeX;   // 플랫폼의 x사이즈는 생성되는 문 수 * 사이즈
        fakeDoorCount = doorPrefabCount - doorCount;  // 가짜 문은 총 문 - 실제 문

        _ground = Instantiate(ground, transform);  // 플랫폼 생성
        _ground.transform.position += new Vector3((groundSizeX / 2f) + (doorSizeX / 2 * i), 0, (groundSizeZ + groundDistance) * count); // 처음 맵이 생성될 위치 조정
        _ground.transform.localScale = new Vector3(groundSizeX, 1, groundSizeZ);  // 사이즈 조정
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
        count++;  // 반복횟수 +1
        doorCount--;  // 실제 문의 수 -1
        doorPrefabCount--;  // 생성될 문의 수 -1
        doorList_l.Clear();
    }

    [PunRPC]
    void EndGround()
    {
        endGround.transform.position = _ground.transform.position + new Vector3(0, 0, groundSizeZ / 2 + groundDistance + clearCubeX * clearCubeX);
    }
}


