using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // 씬 관련 작업 시 꼭 필요

public class GameManager : MonoBehaviourPun
{
    // GameManager 싱글톤
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

    public bool isGameover { get; private set; }  // 게임 종료인지 확인할 bool값
    public bool playerSafe = false;  // 플레이어가 Safe존인지 확인

    PhotonView pv;
    Scene scene;
    int curScene;

    private void Awake()
    {
        if (photonView.IsMine)
        {
            if (instance != this) // 싱글톤된 게 자신이 아니라면 삭제
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
        scene = SceneManager.GetActiveScene();  // 씬 정보 불러오기
        curScene = scene.buildIndex;  // 현재 씬 정보 저장

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
        Debug.Log("게임 끝");
    }
}
