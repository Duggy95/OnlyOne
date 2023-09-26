using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // �� ���� �۾� �� �� �ʿ�

public class GameManager : MonoBehaviourPun
{
    // GameManager �̱���
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindAnyObjectByType<GameManager>();
            }
            return m_instance;
        }
    }

    static GameManager m_instance;
    public Transform makeMap;
    public Transform startGround;

    [HideInInspector]
    public GameObject player;

    public bool isGameover { get; private set; }  // ���� �������� Ȯ���� bool��
    public bool playerSafe = false;  // �÷��̾ Safe������ Ȯ��

    PhotonView pv;
    Scene scene;
    int curScene;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            if (instance != this) // �̱���� �� �ڽ��� �ƴ϶�� ����
            {
                Destroy(gameObject);
            }
        }
        pv = GetComponent<PhotonView>();

        CreatePlayer();
        CreateMap();
    }

    private void CreatePlayer()
    {
        player = PhotonNetwork.Instantiate("Player", startGround.position
            + new Vector3(Random.Range(-7, 7), 10, Random.Range(-2, 2)), startGround.rotation);
    }

    void CreateMap()
    {
        scene = SceneManager.GetActiveScene();  // �� ���� �ҷ�����
        curScene = scene.buildIndex;  // ���� �� ���� ����

        /*if (curScene == 7)
        {
            PhotonNetwork.Instantiate("Six/MakeMap", Vector3.zero, Quaternion.identity);
            makeMap = GameObject.FindGameObjectWithTag("ZERO").transform;
        }
*/
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (curScene == 2)
            PhotonNetwork.Instantiate("One/Map_One", makeMap.position, makeMap.rotation);

        else if (curScene == 3)
            PhotonNetwork.Instantiate("Two/Map_Two", makeMap.position, makeMap.rotation);

        else if (curScene == 4)
            PhotonNetwork.Instantiate("Three/Map_Three", makeMap.position, makeMap.rotation);

        else if (curScene == 5)
            PhotonNetwork.Instantiate("Four/Map_Four", makeMap.position, makeMap.rotation);

        else if (curScene == 6)
            PhotonNetwork.Instantiate("Five/Map_Five", makeMap.position, makeMap.rotation);

        else if (curScene == 7)
            PhotonNetwork.Instantiate("Six/Map_Six", makeMap.position, makeMap.rotation);
    }

    public void EndGame()
    {
        isGameover = true;
        Debug.Log("���� ��");
    }
}
