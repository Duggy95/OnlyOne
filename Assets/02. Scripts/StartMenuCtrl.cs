using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class StartMenuCtrl : MonoBehaviourPunCallbacks
{
    // ���� ����ȭ���� ��Ʈ���� ��ũ��Ʈ

    public GameObject gameName;  // ���� �̸�
    public GameObject menuPanel; // ���� ���� ȭ�鿡 ������ ���� �޴���
    public GameObject chooseMode; // ���� ���ø��
    public GameObject GameMode; // ���ø�忡�� ������ ������ ȭ��
    public Text roomName;   // �� �̸�
    public Text connectInfo;  // �� ���� ���� �ִ���
    public Button startBtn;  // ���� ��ư
    public Button exitBtn;   // ������ ��ư
    public Text log;   // �α� �ؽ�Ʈ

    int[] scenes = { 2, 3, 4, 5, 6, 7 }; // ���� ��
    PhotonView pv;
    AudioSource audioSource;
    AudioClip clickSound;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;

        SetRoomInfo();
        // ������ ��ư�� �̺�Ʈ ���� �Ҵ�
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
        connectInfo.text = $"({room.PlayerCount} / {room.MaxPlayers})"; // $"()" value���� {} �ȿ�
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

    // ���� ���� �� Ż�� �α� ���� ����
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#00ff00>{newPlayer.NickName}</color> �� ����";
        log.text += msg;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        SetRoomInfo();
        string msg = $"\n<color=#ff0000>{otherPlayer.NickName}</color> �� ����";
        log.text += msg;
    }

    // x Ű ������ ��
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

    // ���ӽ��� ��ư ��������
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
        Debug.Log("���� ����");
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
