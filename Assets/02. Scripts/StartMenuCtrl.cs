using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class StartMenuCtrl : MonoBehaviourPunCallbacks
{
    // 게임 시작화면을 컨트롤할 스크립트

    public GameObject gameName;  // 게임 이름
    public GameObject menuPanel; // 게임 시작 화면에 보여질 게임 메뉴판
    public GameObject chooseMode; // 게임 선택모드
    public GameObject GameMode; // 선택모드에서 게임을 선택할 화면
    public Text roomName;   // 룸 이름
    public Text connectInfo;  // 몇 명이 들어와 있는지
    public Button startBtn;  // 시작 버튼
    public Button exitBtn;   // 나가기 버튼
    public Text log;   // 로그 텍스트

    int[] scenes = { 2, 3, 4, 5, 6, 7 }; // 씬의 수
    PhotonView pv;
    AudioSource audioSource;
    AudioClip clickSound;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;

        SetRoomInfo();
        // 나가기 버튼에 이벤트 동적 할당
        exitBtn.onClick.AddListener(() => OnExitClick());
    }

    void Start()
    {
        menuPanel.SetActive(false);
        chooseMode.SetActive(false);
        GameMode.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
        {
            startBtn.gameObject.SetActive(true);
        }
        else
        {
            startBtn.gameObject.SetActive(false);
        }
    }

    void SetRoomInfo()
    {
        Room room = PhotonNetwork.CurrentRoom;
        roomName.text = room.Name;
        connectInfo.text = $"({room.PlayerCount} / {room.MaxPlayers})"; // $"()" value값은 {} 안에
    }

    void OnExitClick()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    // 유저 접속 및 탈주 로그 누적 갱신
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#00ff00>{newPlayer.NickName}</color> 가 접속";
        log.text += msg;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#ff0000>{otherPlayer.NickName}</color> 가 나감";
        log.text += msg;
    }

    // x 키 눌렀을 때
    public void OnClickESC()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("ViewESC", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void ViewESC()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        gameName.SetActive(true);
        exitBtn.gameObject.SetActive(true);
        menuPanel.SetActive(false);
        chooseMode.SetActive(false);
        GameMode.SetActive(false);

        if(PhotonNetwork.IsMasterClient)
            startBtn.gameObject.SetActive(true);
    }

    // 게임시작 버튼 눌렀을때
    public void StartBtnClick()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("ViewStartBtn", RpcTarget.AllBuffered);

        else
            return;
    }

    [PunRPC]
    void ViewStartBtn()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        gameName.SetActive(false);
        exitBtn.gameObject.SetActive(false);
        startBtn.gameObject.SetActive(false);
        menuPanel.SetActive(true);
        chooseMode.SetActive(true);
        GameMode.SetActive(false);
    }

    /*public void GameEsc()
    {
        Application.Quit();
        Debug.Log("게임 종료");
    }*/

    public void OnClickRandomMode()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("PlayRandomMode", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void PlayRandomMode()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        SceneManager.LoadScene(scenes[Random.Range(0, scenes.Length)]);
    }

    public void OnClickChooseMode()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("ViewChooseMode", RpcTarget.AllBuffered);

        else
            return;
    }

    [PunRPC]
    void ViewChooseMode()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        chooseMode.SetActive(false);
        GameMode.SetActive(true);
    }

    public void OnClickScene_One()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_One", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_One()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(2);
    }

    public void OnClickScene_Two()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_Two", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_Two()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(3);
    }

    public void OnClickScene_Three()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_Three", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_Three()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(4);
    }

    public void OnClickScene_Four()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_Four", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_Four()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(5);
    }

    public void OnClickScene_Five()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_Five", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_Five()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(6);
    }


    public void OnClickScene_Six()
    {
        if (PhotonNetwork.IsMasterClient)
            pv.RPC("Map_Six", RpcTarget.All);

        else
            return;
    }

    [PunRPC]
    void Map_Six()
    {
        audioSource.PlayOneShot(clickSound, 1f);

        PhotonNetwork.LoadLevel(7);
    }

}
