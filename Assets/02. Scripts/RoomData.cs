using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;


public class RoomData : MonoBehaviour
{
    RoomInfo _roomInfo;
    TMP_Text roomInfoText;
    PhotonManager photonManager;

    public RoomInfo RoomInfo
    {
        get { return _roomInfo; }
        set 
        { 
            _roomInfo = value;
            roomInfoText.text = $"{_roomInfo.Name} ({_roomInfo.PlayerCount} / {_roomInfo.MaxPlayers})";
            GetComponent<Button>().onClick.AddListener(()=>OnEnterRoom(_roomInfo.Name));
        }
    }

    void Awake()
    {
        roomInfoText = GetComponentInChildren<TMP_Text>();
        photonManager = GameObject.Find("PhotonManager").GetComponent<PhotonManager>();

    }

    void OnEnterRoom(string roomName)
    {
        photonManager.SetUserId();

        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 6;
        ro.IsOpen = true;
        ro.IsVisible = true;

        PhotonNetwork.JoinOrCreateRoom(roomName, ro, TypedLobby.Default);
    }
}
