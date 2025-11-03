using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class PhotonNetworkController : MonoBehaviourPunCallbacks
{
    public static event Action ConnectedToMaster;
    public static event Action JoinedLobby;
    public static event Action<Room> CreatedRoom;
    public static event Action<Room> JoinedRoom;
    public static event Action<List<RoomInfo>> RoomListUpdated;
    public static event Action<Player> PlayerEnteredRoom;
    public static event Action<Player> PlayerLeftRoom;
    public static event Action<Player> MasterClientSwitched;

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
        Logger.Log($"Photon: {PhotonNetwork.LocalPlayer.NickName} is trying to create a room with Room Code {roomCode}");
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
        Logger.Log($"Photon: {PhotonNetwork.LocalPlayer.NickName} requested to join room with Room code {roomName}");
        PhotonNetwork.JoinRoom(roomName);
    }

    public static void LeaveRoom()
    {
        Logger.Log($"Photon: {PhotonNetwork.LocalPlayer.NickName} requested to leave room with Room code {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LeaveRoom();
    }

    public static List<RoomInfo> GetRoomListPhoton()
    {
        return CachedRoomList;
    }

    public static RoomInfo GetCurrentRoom()
    {
        return PhotonNetwork.CurrentRoom;
    }

    public static int GetPlayerCountInCurrentRoom()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    public static bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }
    
    // TODO: This always calls a LINQ on PlayerList so maybe we can cache it simply by adding a dirty flag if player entered or left room.
    public static Player[] GetPlayersCurrentRoom()
    {
        return PhotonNetwork.PlayerList;
    }

    #endregion

    #region Callbacks

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Logger.Log("Photon: Connected To Master");
        ConnectedToMaster?.Invoke();
        
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Logger.Log("Photon: Connected To Lobby");
        JoinedLobby?.Invoke();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Logger.Log($"Photon: Room Creation Successful, Current room {PhotonNetwork.CurrentRoom.Name}");
        CreatedRoom?.Invoke(PhotonNetwork.CurrentRoom);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Logger.Log($"Photon: This Player Joined Room {PhotonNetwork.CurrentRoom.Name}");
        JoinedRoom?.Invoke(PhotonNetwork.CurrentRoom);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Logger.Log("Photon: This Player Left Room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Logger.Log($"Photon: Player {newPlayer.NickName} Entered Room {PhotonNetwork.CurrentRoom.Name}");
        PlayerEnteredRoom?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Logger.Log($"Photon: Player {otherPlayer.NickName} Entered Room {PhotonNetwork.CurrentRoom.Name}");
        PlayerLeftRoom?.Invoke(otherPlayer);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Logger.Log($"Photon: Room List Updated got {roomList.Count} rooms");
        CachedRoomList = roomList;
        CachedRoomNames = roomList.Select(r => r.Name).ToList();
        RoomListUpdated?.Invoke(roomList);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        Logger.Log($"Photon: New MasterClient {newMasterClient.NickName}");
        MasterClientSwitched?.Invoke(newMasterClient);
    }

    #endregion
}

public class PhotonGameConfig
{
    public int PlayerCount { get; set; }
}
