using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using static UnityEngine.UI.Image;


public class MakeRingMap : MonoBehaviourPun
{
    public GameObject cubePrefabs;   // ������ ���� ������ ť�� ������
    public GameObject ringPrefabs;   // �� ������
    public GameObject goldRingPrefabs;   // ��帵 ������
    public GameObject endGround;  // �������
    public GameObject wall;  // ����������� ���� ���� ���� ��
    public Text ringCountText; // ring ī��Ʈ �ؽ�Ʈ

    public int ringCount = 0; // ���� �� ����

    int[] ranNum = { 0, 25, 50, 75, 100, 125, 150, 175, 200, 225, 255 };
    int ringMax = 10;  // �ִ�� ������ �� �ִ� �� ����
    int goldRingMax = 3;  // �ִ�� ������ �� �ִ� �� ����
    int ringCountMax = 30; // �ִ�� ä�����ϴ� �� ����
    int width = 10;   // ������ ���� ��
    int height = 10;  // ������ ���� ��
    float size = 10f;   // ������ ���� �����ϴ� ť���� ������
    float ySize = 20f;  // ������ ���� �����ϴ� ť���� ����
    float ranColorR;
    float ranColorG;
    float ranColorB;
    //float ringSize = 2f;
    bool isSet; // �� ť�� SetActive ����

    // ������ ��ġ ����
    float x;
    float y;
    float z;


    List<GameObject> cubes = new List<GameObject>();   // ������ ť�� ������ ����Ʈ
    List<GameObject> randomCubeActive = new List<GameObject>();  // ��Ȱ��ȭ�� ť�� ����Ʈ
    List<GameObject> _rings = new List<GameObject>();  // ������ �� ������ ����Ʈ
    List<GameObject> _goldRings = new List<GameObject>();  // ������ ��帵 ������ ����Ʈ
    List<GameObject> ringAll = new List<GameObject>();   // ������ ��� ���� ����Ʈ
    List<GameObject> ringPos = new List<GameObject>();   // ������ ��� ���� ��ġ ����Ʈ

    GameObject randomCube;  // �������� ������ ť�� ������Ʈ
    PhotonView pv;
    AudioSource audioSource;
    Vector3 distance;  // ���� ť�� ���� �Ÿ�

    // ��(�Ϲ�, ��� ���ļ�)�� �����Ǹ� ��ġ������ ����Ʈ�� ���
    // �� ������Ǹ� �ش� ��ġ������ ����
    // ��ġ�� ��ġ���ʰ� �� �� ��

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

        // ť�� ���� ����
        size *= 2f;
        ySize *= 2f;

        distance = new Vector3(0f, y + 25, 0f);
        // ��¼��� �� ��ġ ����

        endGround.transform.position = new Vector3(x + ((width * size) / 2f), y + 5, z + (height * size) + size + 5);
        wall.transform.position = new Vector3(x + ((width * size) / 2f), y + size, z + (height * size) + 1);

        if (!PhotonNetwork.IsMasterClient)
            return;

        MakeMap();
        StartCoroutine(MakeRing());
        StartCoroutine(MakeGoldRing());
        // �ð��� ���� ť�� ��Ȱ��ȭ �� �ڷ�ƾ �Լ� ȣ��
        StartCoroutine(RandomCubeSetActive());
    }

    void MakeMap()
    {
        // ���� ���� ����
        Vector3 transPos = new Vector3(x - ((width * size) / 2f) + (size / 2f), y, z + 3);
        pv.RPC("PosZero", RpcTarget.All, transPos);
        // width * height �� ����
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

    // ������ ť�� ����Ʈ�� �߰�
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

    // ������ ť�� �� �ٲ�
    [PunRPC]
    void CubeColor(int i, float r, float g, float b)
    {
        MeshRenderer cubeMesh = cubes[i].GetComponent<MeshRenderer>();

        cubeMesh.material.color = new Color(r, g, b);
    }

    // �ð����� ������ ť�� ��Ƽ�� ��Ʈ��
    IEnumerator RandomCubeSetActive()
    {
        yield return new WaitForSeconds(5f); // ���� ���� 5�� �ں��� ����

        while (GameManager.instance.isGameover == false) // ������ ����Ǵ� ����
        {
            if (GameManager.instance.isGameover == true)
                yield break;

            yield return new WaitForSeconds(2f);  // 3�� �� 

            // ��Ȱ��ȭ�� ť���� ���� �������� 
            int randomNum = Random.Range(width + height, (width + height) * 2);
            Debug.Log("��Ȱ��ȭ �� ť�� �� : " + randomNum);

            // �������� ������ ť�긦 ����Ʈ�� ����
            for (int i = 0; i < randomNum; i++)
            {
                int _randomNum = Random.Range(0, cubes.Count);
                pv.RPC("AddList", RpcTarget.All, _randomNum);
            }

            // ��Ȱ��ȭ �Ǳ� �� �Ͼ������ ����
            for (int i = 0; i < randomCubeActive.Count; i++)
            {
                pv.RPC("MeshCtrl", RpcTarget.All, i);
            }

            yield return new WaitForSeconds(1f);  // 3�� �� 

            // ����� ����Ʈ�� ť�� ��Ȱ��ȭ
            for (int i = 0; i < randomCubeActive.Count; i++)
            {
                isSet = false;
                pv.RPC("ActiveCtrl", RpcTarget.All, i, isSet);
            }

            yield return new WaitForSeconds(5f);

            // 5�� �� �ٽ� Ȱ��ȭ�� ���ÿ� ���� �� �ο�
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

    // ��Ƽ�� ��Ʈ���� ť�� ����Ʈ �߰�
    [PunRPC]
    void AddList(int num)
    {
        GameObject _randomCube = cubes[num];
        randomCubeActive.Add(_randomCube);
    }

    // ���� �� �ʱ�ȭ
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

    // ��Ƽ�� ����
    [PunRPC]
    void ActiveCtrl(int i, bool set)
    {
        randomCubeActive[i].SetActive(set);
    }

    IEnumerator MakeRing()
    {
        yield return new WaitForSeconds(3f);

        for (int i = _rings.Count; i < ringMax; i++)   // ���� �� �������� �ִ� ���� ����
        {
            int randomNum = Random.Range(0, cubes.Count);  //���� �ε��� ����

            Ring(randomNum);
        }
    }

    void Ring(int num)
    {
        randomCube = cubes[num];
        GameObject _ring = PhotonNetwork.Instantiate("Four/Ring", new Vector3(randomCube.transform.position.x, transform.position.y,
            randomCube.transform.position.z) + distance, transform.rotation);  // �� ����
        //_ring.transform.parent = cubes[num].transform;

        Vector3 pos = _ring.transform.position;
        for (int j = 0; j < ringPos.Count; j++)  // �� ������ ����� ����Ʈ ��ŭ �ݺ�
        {
            // ���� ����Ʈ�� �������̶� ���� �� �������̶� ������
            if (pos == ringPos[j].transform.position)
            {
                PhotonNetwork.Destroy(_ring.gameObject); // ������Ʈ ����
                _ring = null;  // null �� ����
                num = Random.Range(0, cubes.Count);
                Ring(num);
                break;  // �ݺ��� Ż��
            }
        }

        if (_ring != null)  // ���� null ���� �ƴ� ���
        {
            //ringAll.Add(_ring);
            /*Debug.Log("��� ring�� ���� : " + ringAll.Count);
            Debug.Log("_ring ����Ʈ�� ũ�� : " + _rings.Count);*/
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
        Debug.Log("_ring ����Ʈ�� ũ�� : " + _rings.Count);
        Debug.Log("��� ring�� ���� : " + ringPos.Count);
    }


    IEnumerator MakeGoldRing()
    {
        yield return new WaitForSeconds(3f);

        for (int i = _goldRings.Count; i < goldRingMax; i++)   // ���� �� �������� �ִ� ���� ����
        {
            int randomNum = Random.Range(0, cubes.Count);  //���� �ε��� ����

            GoldRing(randomNum);
        }
    }

    void GoldRing(int num)
    {
        randomCube = cubes[num];
        GameObject _goldRing = PhotonNetwork.Instantiate("Four/GoldRing",
            new Vector3(randomCube.transform.position.x, transform.position.y,
            randomCube.transform.position.z) + distance, transform.rotation);  // �� ����
        // _goldRing.transform.parent = cubes[num].transform;
        Vector3 pos = _goldRing.transform.position;

        for (int j = 0; j < ringPos.Count; j++)   // �� ������ ����� ����Ʈ ��ŭ �ݺ�
        {
            // ���� ����Ʈ�� �������̶� ���� �� �������̶� ������
            if (pos == ringPos[j].transform.position)
            {
                PhotonNetwork.Destroy(_goldRing.gameObject);  // ������Ʈ ����
                //_goldRing = null;  // null �� ����
                num = Random.Range(0, cubes.Count);
                GoldRing(num);
                break;  // �ݺ��� Ż��
            }
        }

        if (_goldRing != null)   // ���� null ���� �ƴ� ���
        {
            /*ringAll.Add(_goldRing);
            _goldRings.Add(_goldRing);
            Debug.Log("_goldRing ����Ʈ�� ũ�� : " + _goldRings.Count);
            Debug.Log("��� ring�� ���� : " + ringAll.Count);*/
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
        Debug.Log("_goldRing ����Ʈ�� ũ�� : " + _goldRings.Count);
        Debug.Log("��� ring�� ���� : " + ringPos.Count);
    }

    public void RingCount(int score, GameObject ring)  // �� ���� ������Ʈ �Լ�
    {
        audioSource.PlayOneShot(SoundManager.instance.scoreClip, 0.5f);

        ringCount += score;  // ���ھ� �߰�
        ringCountText.text = "Ring : " + ringCount + " / " + ringCountMax;

        if (ringCount >= ringCountMax)  // �� ������ �� ä��� �� ��Ȱ��ȭ
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
                ringPos.RemoveAt(i); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
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
                        StartCoroutine(MakeRing()); // �� ���� �ڷ�ƾ ȣ��
                    }

                    _rings.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                    Debug.Log("_ring ����Ʈ�� ũ�� : " + _rings.Count);
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
                    _goldRings.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                    Debug.Log("_goldRing ����Ʈ�� ũ�� : " + _goldRings.Count);

                    if (PhotonNetwork.IsMasterClient)
                    {
                        // PhotonNetwork.Destroy(_goldRings[j].gameObject);
                        StartCoroutine(MakeGoldRing()); // �� ���� �ڷ�ƾ ȣ��
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
                ringPos.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                Debug.Log("_ring ����Ʈ�� ũ�� : " + ringPos.Count);
                break;
            }
        }

        for (int j = 0; j < _rings.Count; j++)
        {
            if (_rings[j].GetComponent<PhotonView>().ViewID == id)
            {
                _rings.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                Debug.Log("_ring ����Ʈ�� ũ�� : " + _rings.Count);
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
                ringPos.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                Debug.Log("_ring ����Ʈ�� ũ�� : " + ringPos.Count);
                break;
            }
        }

        for (int j = 0; j < _goldRings.Count; j++)
        {
            if (_goldRings[j].GetComponent<PhotonView>().ViewID == id)
            {
                _goldRings.RemoveAt(j); // ����Ʈ���� �ش� ���ӿ�����Ʈ ����
                Debug.Log("_goldRing ����Ʈ�� ũ�� : " + _goldRings.Count);
                break;
            }
        }
    }*/
}
