using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.UI.Image;


public class MakeRingMap : MonoBehaviourPun
{
    public GameObject cubePrefabs;   // 생성될 맵을 구성할 큐브 프리팹
    public GameObject ringPrefabs;   // 링 프리팹
    public GameObject goldRingPrefabs;   // 골드링 프리팹
    public GameObject endGround;  // 결승지점
    public GameObject wall;  // 결승지점으로 가는 길을 막을 벽
    public Text ringCountText; // ring 카운트 텍스트

    public int ringCount = 0; // 현재 링 개수

    int[] ranNum = { 0, 25, 50, 75, 100, 125, 150, 175, 200, 225, 255 };
    int ringMax = 10;  // 최대로 생성될 수 있는 링 개수
    int goldRingMax = 3;  // 최대로 생성될 수 있는 링 개수
    int ringCountMax = 30; // 최대로 채워야하는 링 개수
    int width = 10;   // 생성될 맵의 행
    int height = 10;  // 생성될 맵의 열
    float size = 10f;   // 생성될 맵을 구성하는 큐브의 사이즈
    float ySize = 20f;  // 생성될 맵을 구성하는 큐브의 높이
    float ranColorR;
    float ranColorG;
    float ranColorB;
    //float ringSize = 2f;
    bool isSet; // 각 큐브 SetActive 여부

    // 기준의 위치 저장
    float x;
    float y;
    float z;


    List<GameObject> cubes = new List<GameObject>();   // 생성된 큐브 프리팹 리스트
    List<GameObject> randomCubeActive = new List<GameObject>();  // 비활성화할 큐브 리스트
    List<GameObject> _rings = new List<GameObject>();  // 생성된 링 프리팹 리스트
    List<GameObject> _goldRings = new List<GameObject>();  // 생성된 골드링 프리팹 리스트
    List<GameObject> ringAll = new List<GameObject>();   // 생성된 모든 링의 리스트
    List<GameObject> ringPos = new List<GameObject>();   // 생성된 모든 링의 위치 리스트

    GameObject randomCube;  // 랜덤으로 지정된 큐브 오브젝트
    PhotonView pv;
    AudioSource audioSource;
    Vector3 distance;  // 링과 큐브 사이 거리

    // 링(일반, 골드 합쳐서)이 생성되면 위치정보를 리스트에 담고
    // 링 리무브되면 해당 위치정보도 삭제
    // 위치가 겹치지않게 해 볼 것

    private void Awake()
    {
        ringCountText = GameObject.Find("RingCountText").GetComponent<Text>();

        ringCountText.text = "Ring : " + ringCount + " / " + ringCountMax;

        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;

        // 큐브 간격 조정
        size *= 2f;
        ySize *= 2f;

        distance = new Vector3(0f, y + 25, 0f);
        // 결승선과 벽 위치 지정

        endGround.transform.position = new Vector3(x + ((width * size) / 2f), y + 5, z + (height * size) + size + 5);
        wall.transform.position = new Vector3(x + ((width * size) / 2f), y + size, z + (height * size) + 1);

        if (!PhotonNetwork.IsMasterClient)
            return;

        MakeMap();
        StartCoroutine(MakeRing());
        StartCoroutine(MakeGoldRing());
        // 시간에 따라 큐브 비활성화 할 코루틴 함수 호출
        StartCoroutine(RandomCubeSetActive());
    }

    void MakeMap()
    {
        // 생성 원점 변경
        Vector3 transPos = new Vector3(x - ((width * size) / 2f) + (size / 2f), y, z + 3);
        pv.RPC("PosZero", RpcTarget.All, transPos);
        // width * height 맵 생성
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject _cube = PhotonNetwork.Instantiate("Four/Cube", transform.position +
                    new Vector3(i * size, y, j * size), transform.rotation);
            }
        }
        pv.RPC("AddCube", RpcTarget.All);

        for (int i = 0; i < cubes.Count; i++)
        {
            ranColorR = ranNum[Random.Range(0, ranNum.Length)] / 255f;
            ranColorG = ranNum[Random.Range(0, ranNum.Length)] / 255f;
            ranColorB = ranNum[Random.Range(0, ranNum.Length)] / 255f;
            pv.RPC("CubeColor", RpcTarget.All, i, ranColorR, ranColorG, ranColorB);
        }
    }

    [PunRPC]
    void PosZero(Vector3 pos)
    {
        transform.position = pos;
    }

    // 생성된 큐브 리스트에 추가
    [PunRPC]
    void AddCube()
    {
        GameObject[] _cube = GameObject.FindGameObjectsWithTag("JUMPZONE");
        for (int i = 0; i < _cube.Length; i++)
        {
            cubes.Add(_cube[i]);
            _cube[i].transform.parent = transform;
            _cube[i].name = "cube_" + i;
        }
    }

    // 생성된 큐브 색 바꿈
    [PunRPC]
    void CubeColor(int i, float r, float g, float b)
    {
        MeshRenderer cubeMesh = cubes[i].GetComponent<MeshRenderer>();

        cubeMesh.material.color = new Color(r, g, b);
    }

    // 시간마다 랜덤한 큐브 액티브 컨트롤
    IEnumerator RandomCubeSetActive()
    {
        yield return new WaitForSeconds(5f); // 게임 시작 5초 뒤부터 시작

        while (GameManager.instance.isGameover == false) // 게임이 진행되는 동안
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            yield return new WaitForSeconds(2f);  // 3초 뒤 

            // 비활성화될 큐브의 수는 랜덤으로 
            int randomNum = Random.Range(width + height, (width + height) * 2);
            Debug.Log("비활성화 될 큐브 수 : " + randomNum);

            // 랜덤으로 정해진 큐브를 리스트에 저장
            for (int i = 0; i < randomNum; i++)
            {
                int _randomNum = Random.Range(0, cubes.Count);
                pv.RPC("AddList", RpcTarget.All, _randomNum);
            }

            // 비활성화 되기 전 하얀색으로 변경
            for (int i = 0; i < randomCubeActive.Count; i++)
            {
                pv.RPC("MeshCtrl", RpcTarget.All, i);
            }

            yield return new WaitForSeconds(1f);  // 3초 뒤 

            // 저장된 리스트의 큐브 비활성화
            for (int i = 0; i < randomCubeActive.Count; i++)
            {
                isSet = false;
                pv.RPC("ActiveCtrl", RpcTarget.All, i, isSet);
            }

            yield return new WaitForSeconds(5f);

            // 5초 뒤 다시 활성화와 동시에 랜덤 색 부여
            for (int i = 0; i < randomCubeActive.Count; i++)
            {
                ranColorR = ranNum[Random.Range(0, ranNum.Length)] / 255f;
                ranColorG = ranNum[Random.Range(0, ranNum.Length)] / 255f;
                ranColorB = ranNum[Random.Range(0, ranNum.Length)] / 255f;

                isSet = true;

                pv.RPC("ActiveCtrl", RpcTarget.All, i, isSet);
                pv.RPC("OriginColor", RpcTarget.All, i, ranColorR, ranColorG, ranColorB);
            }

            pv.RPC("ClearList", RpcTarget.All);
        }
    }

    // 액티브 컨트롤할 큐브 리스트 추가
    [PunRPC]
    void AddList(int num)
    {
        GameObject _randomCube = cubes[num];
        randomCubeActive.Add(_randomCube);
    }

    // 끝난 뒤 초기화
    [PunRPC]
    void ClearList()
    {
        randomCubeActive.Clear();
    }

    [PunRPC]
    void MeshCtrl(int i)
    {
        randomCubeActive[i].GetComponent<MeshRenderer>().material.color = Color.white;
    }

    [PunRPC]
    void OriginColor(int i, float r, float g, float b)
    {
        randomCubeActive[i].GetComponent<MeshRenderer>().material.color = new Color(r, g, b);
    }

    // 액티브 조절
    [PunRPC]
    void ActiveCtrl(int i, bool set)
    {
        randomCubeActive[i].SetActive(set);
    }

    IEnumerator MakeRing()
    {
        yield return new WaitForSeconds(3f);

        for (int i = _rings.Count; i < ringMax; i++)   // 현재 링 개수부터 최대 개수 까지
        {
            int randomNum = Random.Range(0, cubes.Count);  //랜덤 인덱스 저장

            Ring(randomNum);
        }
    }

    void Ring(int num)
    {
        randomCube = cubes[num];
        GameObject _ring = PhotonNetwork.Instantiate("Four/Ring", new Vector3(randomCube.transform.position.x, transform.position.y,
            randomCube.transform.position.z) + distance, transform.rotation);  // 링 생성
        //_ring.transform.parent = cubes[num].transform;

        Vector3 pos = _ring.transform.position;
        for (int j = 0; j < ringPos.Count; j++)  // 링 포지션 저장된 리스트 만큼 반복
        {
            // 만약 리스트의 포지션이랑 현재 링 포지션이랑 같으면
            if (pos == ringPos[j].transform.position)
            {
                PhotonNetwork.Destroy(_ring.gameObject); // 오브젝트 삭제
                _ring = null;  // null 값 저장
                num = Random.Range(0, cubes.Count);
                Ring(num);
                break;  // 반복문 탈출
            }
        }

        if (_ring != null)  // 링이 null 값이 아닌 경우
        {
            //ringAll.Add(_ring);
            /*Debug.Log("모든 ring의 개수 : " + ringAll.Count);
            Debug.Log("_ring 리스트의 크기 : " + _rings.Count);*/
            pv.RPC("AddRing", RpcTarget.All, pos);
        }
    }

    [PunRPC]
    void AddRing(Vector3 pos)
    {
        GameObject[] rings = GameObject.FindGameObjectsWithTag("RING");
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i].transform.position == pos)
            {
                _rings.Add(rings[i]);
                ringPos.Add(rings[i]);
            }
        }
        Debug.Log("_ring 리스트의 크기 : " + _rings.Count);
        Debug.Log("모든 ring의 개수 : " + ringPos.Count);
    }


    IEnumerator MakeGoldRing()
    {
        yield return new WaitForSeconds(3f);

        for (int i = _goldRings.Count; i < goldRingMax; i++)   // 현재 링 개수부터 최대 개수 까지
        {
            int randomNum = Random.Range(0, cubes.Count);  //랜덤 인덱스 저장

            GoldRing(randomNum);
        }
    }

    void GoldRing(int num)
    {
        randomCube = cubes[num];
        GameObject _goldRing = PhotonNetwork.Instantiate("Four/GoldRing",
            new Vector3(randomCube.transform.position.x, transform.position.y,
            randomCube.transform.position.z) + distance, transform.rotation);  // 링 생성
        // _goldRing.transform.parent = cubes[num].transform;
        Vector3 pos = _goldRing.transform.position;

        for (int j = 0; j < ringPos.Count; j++)   // 링 포지션 저장된 리스트 만큼 반복
        {
            // 만약 리스트의 포지션이랑 현재 링 포지션이랑 같으면
            if (pos == ringPos[j].transform.position)
            {
                PhotonNetwork.Destroy(_goldRing.gameObject);  // 오브젝트 삭제
                //_goldRing = null;  // null 값 저장
                num = Random.Range(0, cubes.Count);
                GoldRing(num);
                break;  // 반복문 탈출
            }
        }

        if (_goldRing != null)   // 링이 null 값이 아닌 경우
        {
            /*ringAll.Add(_goldRing);
            _goldRings.Add(_goldRing);
            Debug.Log("_goldRing 리스트의 크기 : " + _goldRings.Count);
            Debug.Log("모든 ring의 개수 : " + ringAll.Count);*/
            pv.RPC("AddGold", RpcTarget.All, pos);
        }
    }

    [PunRPC]
    void AddGold(Vector3 pos)
    {
        GameObject[] rings = GameObject.FindGameObjectsWithTag("GOLDRING");
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i].transform.position == pos)
            {
                _goldRings.Add(rings[i]);
                ringPos.Add(rings[i]);
            }
        }
        Debug.Log("_goldRing 리스트의 크기 : " + _goldRings.Count);
        Debug.Log("모든 ring의 개수 : " + ringPos.Count);
    }

    public void RingCount(int score, GameObject ring)  // 링 개수 업데이트 함수
    {
        audioSource.PlayOneShot(SoundManager.instance.scoreClip, 0.5f);

        ringCount += score;  // 스코어 추가
        ringCountText.text = "Ring : " + ringCount + " / " + ringCountMax;

        if (ringCount >= ringCountMax)  // 링 개수를 다 채우면 벽 비활성화
        {
            wall.gameObject.SetActive(false);
        }

        Vector3 pos = ring.transform.position;

        if (ring.gameObject.tag == "RING")
        {
            pv.RPC("ReMake", RpcTarget.All, pos, 1);
        }

        else if (ring.gameObject.tag == "GOLDRING")
        {
            pv.RPC("ReMake", RpcTarget.All, pos, 2);
        }
    }

    [PunRPC]
    void ReMake(Vector3 pos, int _tag)
    {
        for (int i = 0; i < ringPos.Count; i++)
        {
            if (ringPos[i].transform.position == pos)
            {
                PhotonNetwork.Destroy(ringPos[i].gameObject);
                ringPos.RemoveAt(i); // 리스트에서 해당 게임오브젝트 제거
                break;
            }
        }

        if (_tag == 1)
        {
            for (int j = 0; j < _rings.Count; j++)
            {
                if (_rings[j].transform.position == pos)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // PhotonNetwork.Destroy(_rings[j].gameObject);
                        StartCoroutine(MakeRing()); // 링 생성 코루틴 호출
                    }

                    _rings.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                    Debug.Log("_ring 리스트의 크기 : " + _rings.Count);
                    break;
                }
            }
        }

        else if (_tag == 2)
        {
            for (int j = 0; j < _goldRings.Count; j++)
            {
                if (_goldRings[j].transform.position == pos)
                {
                    _goldRings.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                    Debug.Log("_goldRing 리스트의 크기 : " + _goldRings.Count);

                    if (PhotonNetwork.IsMasterClient)
                    {
                        // PhotonNetwork.Destroy(_goldRings[j].gameObject);
                        StartCoroutine(MakeGoldRing()); // 링 생성 코루틴 호출
                    }

                    break;
                }
            }
        }
    }

/*    [PunRPC]
    void RemoveRing(int id)
    {
        for (int j = 0; j < ringPos.Count; j++)
        {
            if (ringPos[j].GetComponent<PhotonView>().ViewID == id)
            {
                ringPos.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                Debug.Log("_ring 리스트의 크기 : " + ringPos.Count);
                break;
            }
        }

        for (int j = 0; j < _rings.Count; j++)
        {
            if (_rings[j].GetComponent<PhotonView>().ViewID == id)
            {
                _rings.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                Debug.Log("_ring 리스트의 크기 : " + _rings.Count);
                break;
            }
        }
    }

    [PunRPC]
    void RemoveGold(int id)
    {
        for (int j = 0; j < ringPos.Count; j++)
        {
            if (ringPos[j].GetComponent<PhotonView>().ViewID == id)
            {
                ringPos.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                Debug.Log("_ring 리스트의 크기 : " + ringPos.Count);
                break;
            }
        }

        for (int j = 0; j < _goldRings.Count; j++)
        {
            if (_goldRings[j].GetComponent<PhotonView>().ViewID == id)
            {
                _goldRings.RemoveAt(j); // 리스트에서 해당 게임오브젝트 제거
                Debug.Log("_goldRing 리스트의 크기 : " + _goldRings.Count);
                break;
            }
        }
    }*/
}
