using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    readonly string version = "1.0";
    string userId;

    public InputField userIF;
    public InputField roomIF;
    public Transform scrollContent;
    public Text noRoomTxt;
    public GameObject tutorialGround;
    public GameObject tutorialPlayer;
    public Canvas mainCanvas;
    public Canvas tutorialCanvas;
    public Camera mainCam;
    public Camera tutorialCam;
    /*public Button tutorialBtn;
    public Button tutorialEsc;
    public Button exit;*/

    Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    GameObject roomItemPrefab;
    AudioSource audioSource;
    AudioClip clickSound;

    bool isOnJoinedLobby = false;
    bool isMasterConn = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        clickSound = SoundManager.instance.UIClickClip;
        tutorialPlayer.GetComponent<Rigidbody>().useGravity = false;
        tutorialGround.SetActive(false);
        tutorialPlayer.SetActive(false);

        /*mainCam.enabled = true;
        tutorialCam.enabled = false;
        mainCanvas.enabled = true;
        tutorialCanvas.enabled = false;*/

        mainCam.gameObject.SetActive(true);
        tutorialCam.gameObject.SetActive(false);
        mainCanvas.gameObject.SetActive(true);
        tutorialCanvas.gameObject.SetActive(false);

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = version;

        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    private void Start()
    {
        userId = PlayerPrefs.GetString("USER_ID", "USER_" + Random.Range(0, 7));
        userIF.text = userId;

        PhotonNetwork.NickName = userId;
    }

    private void Update()
    {
        if (rooms.Count == 0)
            noRoomTxt.gameObject.SetActive(true);
        else
            noRoomTxt.gameObject.SetActive(false);
    }

    public void SetUserId()
    {
        if (string.IsNullOrEmpty(userIF.text))
            userId = "USER_" + Random.Range(1, 7);

        else
            userId = userIF.text;

        PlayerPrefs.SetString("USER_ID", userId);
    }

    string SetRoomName()
    {
        if (string.IsNullOrEmpty(roomIF.text))
            roomIF.text = "ROOM_" + Random.Range(1, 101);

        return roomIF.text;
    }

    public override void OnConnectedToMaster()
    {
        print("������ ���� ����");
        PhotonNetwork.JoinLobby();
        isMasterConn = true;
    }

    public override void OnJoinedLobby()
    {
        print("�κ� ���� ����");
        isOnJoinedLobby = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        print("�� ���� ����");
    }

    public override void OnCreatedRoom()
    {
        print("�� ���� �Ϸ�");
    }

    public override void OnJoinedRoom()
    {
        print("�� ���� �Ϸ�");
        print("���� ���� �� = " + PhotonNetwork.CurrentRoom.PlayerCount);

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            print(player.Value.NickName + "-" + player.Value.ActorNumber);
        }
        // ������ Ŭ���̾�Ʈ�� ��� �� ������ ������ �ε�
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(1);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        GameObject tempRoom = null; // ������ ������� �ӽ� ����� ����

        foreach (var room in roomList)
        {
            if(room.RemovedFromList) // ���� ������ ���
            {
                // ��ųʸ����� �� �̸����� �˻��Ͽ� �ӽ����� ������ ����
                rooms.TryGetValue(room.Name, out tempRoom);
                Destroy(tempRoom); // �� ������ ������ ����
                rooms.Remove(room.Name); // ��ųʸ����� ����
            }

            else  // �� ���� ����
            {
                // Contains >> �����ϸ� true
                // rooms��� ��ųʸ��� �ش� ���̸��� ����X = ���� �߰�
                if (!rooms.ContainsKey(room.Name))
                {
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent);
                    roomPrefab.GetComponent<RoomData>().RoomInfo = room;
                    rooms.Add(room.Name, roomPrefab);
                }
                // rooms��� ��ųʸ��� �ش� ���̸��� ����X = �� ���� ����
                else
                {
                    rooms.TryGetValue(room.Name,out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = room;
                }
            }
            print("Room = " + room.Name + "(" + room.PlayerCount + "/" + room.MaxPlayers + ")");
        }
    }

    #region UI_BUTTON_EVENT

    public void OnClickExit()
    {
        audioSource.PlayOneShot(clickSound, 1.0f); 
        Application.Quit();
    }

    public void OnClickTutorial()
    {
        audioSource.PlayOneShot(clickSound, 1.0f);
        tutorialGround.SetActive(true);
        tutorialPlayer.SetActive(true);
    }

    public void OnLoginClick()
    {
        audioSource.PlayOneShot(clickSound, 1.0f);

        if (!isOnJoinedLobby || !isMasterConn)
            return;
        SetUserId();
        //PhotonNetwork.JoinRandomRoom();
    }

    public void OnMakeRoomClick()
    {
        audioSource.PlayOneShot(clickSound, 1.0f);

        if (!isOnJoinedLobby || !isMasterConn)
            return;

        SetUserId();

        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 6;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }

    #endregion
}
