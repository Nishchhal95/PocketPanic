using System;
using Photon.Pun;
using UnityEngine;

public class PhotonNetworkController : MonoBehaviourPunCallbacks
{
    public static event Action ConnectedToMaster;
    public static event Action JoinedLobby;
    
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

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

    #endregion
}
