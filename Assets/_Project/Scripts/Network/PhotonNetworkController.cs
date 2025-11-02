using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonNetworkController : MonoBehaviourPunCallbacks
{
    public static event Action ConnectedToMaster;
    public static event Action JoinedLobby;
    public static event Action<Room> CreatedRoom;
    public static event Action<Room> JoinedRoom;
    public static event Action<List<RoomInfo>> RoomListUpdated;

    private static List<RoomInfo> CachedRoomList = new();
    private static List<string> CachedRoomNames = new();
    
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    #region Static Helper Functions

    public static void SetPlayerName(string playerName)
    {
        PhotonNetwork.NickName = playerName;
    }

    public static void CreateGame(PhotonGameConfig photonGameConfig)
    {
        string roomCode = Utilites.GenerateAlphanumericCode(CachedRoomNames);
        Debug.Log($"Photon: {PhotonNetwork.LocalPlayer.NickName} is trying to create a room with Room Code {roomCode}");
        PhotonNetwork.CreateRoom(roomCode, new RoomOptions
        {
            IsOpen = true,
            IsVisible = true,
            MaxPlayers = photonGameConfig.PlayerCount,
            PublishUserId = true
        });
    }

    public static void JoinRoom(string roomName)
    {
        Debug.Log($"Photon: {PhotonNetwork.LocalPlayer.NickName} requested to join room with Room code {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

    public static List<RoomInfo> GetRoomListPhoton()
    {
        return CachedRoomList;
    }

    #endregion

    #region Callbacks

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Photon: Connected To Master");
        ConnectedToMaster?.Invoke();
        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Photon: Connected To Lobby");
        JoinedLobby?.Invoke();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log($"Photon: Room Creation Successful, Current room {PhotonNetwork.CurrentRoom.Name}");
        CreatedRoom?.Invoke(PhotonNetwork.CurrentRoom);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log($"Photon: This Player Joined Room {PhotonNetwork.CurrentRoom.Name}");
        JoinedRoom?.Invoke(PhotonNetwork.CurrentRoom);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Photon: This Player Left Room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"Photon: Player {newPlayer.NickName} Entered Room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log($"Photon: Player {otherPlayer.NickName} Entered Room {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log($"Photon: Room List Updated got {roomList.Count} rooms");
        CachedRoomList = roomList;
        CachedRoomNames = roomList.Select(r => r.Name).ToList();
        RoomListUpdated?.Invoke(roomList);
    }

    #endregion
}

public class PhotonGameConfig
{
    public int PlayerCount { get; set; }
}
