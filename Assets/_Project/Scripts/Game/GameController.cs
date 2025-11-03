using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GameController : MonoBehaviourPun
{
    public static GameController Instance { get; private set; }
    
    [SerializeField] private GameCanvasController gameCanvasController;
    [SerializeField] private GamePlayer localGamePlayerPrefab;
    [SerializeField] private GamePlayer remoteGamePlayerPrefab;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnGameStarted += SetupGame;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStarted -= SetupGame;
    }

    #region NetworkCalls

    public void SendStartGameToAll()
    {
        if (!PhotonNetworkController.IsMasterClient())
        {
            Logger.Error("Only Master Clients can Start the Game");
            return;
        }
        
        photonView.RPC(nameof(GameStartedRPC), RpcTarget.All);
    }

    #endregion

    #region RPCs
    
    [PunRPC]
    private void GameStartedRPC()
    {
        GameEvents.RaiseGameStarted();
    }

    #endregion
    

    private void SetupGame()
    {
        SpawnPlayers();
    }

    private void SpawnPlayers()
    {
        Player[] players = PhotonNetworkController.GetPlayersCurrentRoom();
        List<Transform> playerSlots = GetPlayerSlotsFromPlayerCount(players.Length);
        
        int localPlayerIndex = Array.FindIndex(players, p => p.IsLocal);

        for (int i = 0; i < players.Length; i++)
        {
            // This makes sure we always start with Local Player to make them seat always first which is in the bottom
            int playerIndex = (localPlayerIndex + i) % players.Length;
            Player player = players[playerIndex];
            Transform playerSlot = playerSlots[i];

            GamePlayer gamePlayer = Instantiate(player.IsLocal ? localGamePlayerPrefab : remoteGamePlayerPrefab, playerSlot);
            gamePlayer.Init(player.NickName, player.ActorNumber, player.UserId);
        }
    }

    private List<Transform> GetPlayerSlotsFromPlayerCount(int playerCount)
    {
        foreach (PlayerCountToSlots item in gameCanvasController.PlayerCountToSlotsArray)
        {
            if (item.PlayerCount == playerCount)
            {
                return item.PlayerSlotTransforms;
            }
        }

        Logger.Error($"Could not find Slots Map for {playerCount} Players");
        return null;
    }
}
